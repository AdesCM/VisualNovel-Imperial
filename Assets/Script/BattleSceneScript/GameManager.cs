using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Player & Team Settings")]
    public Player player1;
    public Player player2;

    [Header("Asset References")]
    public CardDataSO dummyCard;
    public GameObject cardPrefab;

    [Header("Scene Transforms")]
    public Transform p1BattlePos;
    public Transform p2BattlePos;

    private Dictionary<string, CardDataSO> cardDatabase;
    private Queue<CardEffectExecution> effectQueue = new Queue<CardEffectExecution>();
    private int currentMaxSlots;

    private struct CardEffectExecution { public CardEffect effect; public Player ownerPlayer; public Player opponentPlayer; public Character ownerUser; public Character opponentUser; }
    private struct EffectToSort { public CardEffectExecution execution; public int speed; public int attackPower; public System.Guid randomId; }

    void Awake()
    {
        LoadAllCardsFromAssets();
    }

    void Start()
    {
        player1.characters.Add(new Character("검사", 100, 13, 10, 0));
        player2.characters.Add(new Character("마법사", 80, 15, 5, 0));
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player1.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player2.characters[0] });
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player1.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player2.characters[0] });
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player1.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player2.characters[0] });
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player1.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player2.characters[0] });
        StartCoroutine(BattleRoutine());
    }

    private void LoadAllCardsFromAssets()
    {
        cardDatabase = new Dictionary<string, CardDataSO>();
        var loadedCards = Resources.LoadAll<CardDataSO>("Cards");
        foreach (var cardSO in loadedCards) { cardDatabase.Add(cardSO.cardId, cardSO); }
    }

    IEnumerator BattleRoutine()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log("--- 전투 시작 ---");

        int p1TotalSlots = player1.baseSlots + player1.bonusSlots;
        int p2TotalSlots = player2.baseSlots + player2.bonusSlots;
        currentMaxSlots = Mathf.Max(p1TotalSlots, p2TotalSlots);
        player1.bonusSlots = 0; player2.bonusSlots = 0;

        DetermineAllSlotStates();

        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 턴 시작 ---");
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnStart));
        if (CheckForGameOver()) yield break;

        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 카드 공개 ---");
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.CardReveal));
        if (CheckForGameOver()) yield break;

        for (int i = 0; i < currentMaxSlots; i++)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log($"\n<<<<< 라운드 {i + 1} 시작 >>>>>");

            bool p1HasSlot = i < player1.registeredSlots.Count;
            bool p2HasSlot = i < player2.registeredSlots.Count;
            RegisteredCardSlot p1Slot = p1HasSlot ? player1.registeredSlots[i] : new RegisteredCardSlot { cardSO = dummyCard, user = null };
            RegisteredCardSlot p2Slot = p2HasSlot ? player2.registeredSlots[i] : new RegisteredCardSlot { cardSO = dummyCard, user = null };

            GameObject p1CardObject = Instantiate(cardPrefab, p1BattlePos.position, Quaternion.identity);
            CardView p1CardView = p1CardObject.GetComponent<CardView>();
            p1CardView.Setup(p1Slot.cardSO, GetStateData(p1Slot));

            GameObject p2CardObject = Instantiate(cardPrefab, p2BattlePos.position, Quaternion.identity);
            CardView p2CardView = p2CardObject.GetComponent<CardView>();
            p2CardView.Setup(p2Slot.cardSO, GetStateData(p2Slot));

            yield return new WaitForSeconds(1f);
            
            // ★★★ 수정된 부분 1: 라운드 인덱스 'i'를 ProcessSingleRound에 전달합니다 ★★★
            yield return StartCoroutine(ProcessSingleRound(p1Slot, p2Slot, p1CardView, p2CardView, i));

            p1CardView.DestroyCard();
            p2CardView.DestroyCard();
            yield return new WaitForSeconds(0.5f);

            if (CheckForGameOver()) yield break;
        }

        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 턴 종료 ---");
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnEnd));
        if (CheckForGameOver()) yield break;

        ApplySanityDamage();
        if (CheckForGameOver()) yield break;

        player1.ClearAllSlotBuffs();
        player2.ClearAllSlotBuffs();
        if (GameConstants.DEBUG_MODE) Debug.Log("\n--- 모든 페이즈 및 라운드 정상 종료 ---");
    }

    void DetermineAllSlotStates()
    {
        foreach (var slot in player1.registeredSlots) DetermineSlotState(slot);
        foreach (var slot in player2.registeredSlots) DetermineSlotState(slot);
    }

    void DetermineSlotState(RegisteredCardSlot slot)
{
    if (slot.user == null) { slot.state = SlotState.Awakened; return; }

    // ★★★ 수정된 부분 ★★★
    // 사용자의 정신력이 0 이상이면 자각(Awakened), 0 미만이면 계시(Revelation)로 결정
    slot.state = (slot.user.sanity >= 0) ? SlotState.Awakened : SlotState.Revelation;
    
    // 각성(Encroachment) / 침식(Corrosion)으로 변하는 조건은 이전과 동일하게 유지됩니다.
    if (slot.state == SlotState.Awakened && slot.user.sanity == 15 && slot.cardSO.HasEncroachmentState)
    {
        slot.state = SlotState.Encroachment;
    }
    else if (slot.state == SlotState.Revelation && slot.user.sanity == -15 && slot.cardSO.HasCorrosionState)
    {
        slot.state = SlotState.Corrosion;
    }
    
    if (GameConstants.DEBUG_MODE) Debug.Log($"{slot.user.characterName}의 카드 '{GetStateData(slot).stateName}' 최종 상태: {slot.state}");
}

    // ★★★ 수정된 부분 2: roundIndex 파라미터를 받도록 수정 ★★★
    IEnumerator ProcessSingleRound(RegisteredCardSlot slot1, RegisteredCardSlot slot2, CardView view1, CardView view2, int roundIndex)
    {
        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 전투 전 ---");
        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PreCombat, slot1, slot2));
        if (CheckForGameOver()) yield break;

        if (slot1.user != null && slot2.user != null)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log("--- [전투] 우위 경쟁 ---");
            int p1InitiativeRoll = DiceRollParser.Parse(GetStateData(slot1).attackDice).Roll();
            int p2InitiativeRoll = DiceRollParser.Parse(GetStateData(slot2).attackDice).Roll();
            if (GameConstants.DEBUG_MODE) Debug.Log($"{slot1.user.characterName} 주사위: {p1InitiativeRoll}  vs  {slot2.user.characterName} 주사위: {p2InitiativeRoll}");

            RegisteredCardSlot winnerSlot = null, loserSlot = null;
            Player winnerPlayer = null, loserPlayer = null; // 슬롯 버프 적용을 위해 Player 정보도 저장
            CardView winnerView = null, loserView = null;
            int winnerIndex = -1, loserIndex = -1; // 슬롯 버프 적용을 위해 인덱스 정보도 저장

            if (p1InitiativeRoll > p2InitiativeRoll)
            {
                winnerSlot = slot1; loserSlot = slot2;
                winnerPlayer = player1; loserPlayer = player2;
                winnerView = view1; loserView = view2;
                winnerIndex = roundIndex; loserIndex = roundIndex;
            }
            else if (p2InitiativeRoll > p1InitiativeRoll)
            {
                winnerSlot = slot2; loserSlot = slot1;
                winnerPlayer = player2; loserPlayer = player1;
                winnerView = view2; loserView = view1;
                winnerIndex = roundIndex; loserIndex = roundIndex;
            }

            if (winnerSlot != null)
            {
                if (GameConstants.DEBUG_MODE) Debug.Log($"[전투] 우위 경쟁 승자: {winnerSlot.user.characterName}");
                if (GameConstants.DEBUG_MODE) Debug.Log($"--- {winnerSlot.user.characterName}의 '{GetStateData(winnerSlot).stateName}' 공격 ---");

                Character attacker = winnerSlot.user;
                Character defender = loserSlot.user;

                // ★★★ 수정된 부분 3: CalculateFinalDamage 호출 시 roundIndex(winnerIndex)를 전달 ★★★
                int finalDamage = CalculateFinalDamage(winnerSlot, loserSlot, winnerPlayer, winnerIndex);
                defender.TakeDamage(finalDamage);

                attacker.ChangeSanity(2);
                defender.ChangeSanity(-3);

                if (GameConstants.DEBUG_MODE)
                {
                    string attackerStatus = $"공격자: {attacker.characterName} | HP: {attacker.currentHp}/{attacker.maxHealth} | 정신력: {attacker.sanity} | 사용 카드: {GetStateData(winnerSlot).stateName}";
                    string defenderStatus = $"방어자: {defender.characterName} | HP: {defender.currentHp}/{defender.maxHealth} | 정신력: {defender.sanity} | 사용 카드: {GetStateData(loserSlot).stateName}";
                    Debug.Log($"[전투 결과]\n{attackerStatus}\n{defenderStatus}");
                }

                if (defender.currentHp <= 0)
                {
                    HandleCharacterDeath(winnerSlot, loserSlot);
                }

                winnerView.PlayAttackAnimation(loserView.transform.position, winnerView.transform.position);
                yield return new WaitForSeconds(0.5f);
                loserView.PlayDamageEffect();
                if (CheckForGameOver()) yield break;
            }
            else
            {
                if (GameConstants.DEBUG_MODE) Debug.Log("[전투] 우위 경쟁 무승부! 전투가 무효 처리됩니다.");
            }
        }

        yield return new WaitForSeconds(1f);

        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 전투 후 ---");
        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PostCombat, slot1, slot2));
    }

    // ★★★ 수정된 부분 4: roundIndex(slotIndex)를 파라미터로 받도록 수정 ★★★
    private int CalculateFinalDamage(RegisteredCardSlot attackerSlot, RegisteredCardSlot defenderSlot, Player attackerPlayer, int slotIndex)
    {
        Character attacker = attackerSlot.user;
        Character defender = defenderSlot.user;
        CardStateData attackerCardState = GetStateData(attackerSlot);

        float damageReduction = GameConstants.MAX_DAMAGE_REDUCTION_RATE * (defender.defense / (float)(defender.defense + GameConstants.DEFENSE_CONSTANT));
        
        DiceRoll diceRoll = DiceRollParser.Parse(attackerCardState.attackDice);
        // ★★★ 수정된 부분 5: 전달받은 slotIndex를 사용해 슬롯 버프 적용 ★★★
        if (attackerPlayer.slotDiceCountBuffs.ContainsKey(slotIndex))
        {
            diceRoll.numberOfDice += attackerPlayer.slotDiceCountBuffs[slotIndex];
            if (GameConstants.DEBUG_MODE) Debug.Log($"[슬롯 버프] 주사위 굴림 +{attackerPlayer.slotDiceCountBuffs[slotIndex]}");
        }
        if (attackerPlayer.slotDiceMaxBuffs.ContainsKey(slotIndex))
        {
            diceRoll.sidesOfDice += attackerPlayer.slotDiceMaxBuffs[slotIndex];
            if (GameConstants.DEBUG_MODE) Debug.Log($"[슬롯 버프] 주사위 최대값 +{attackerPlayer.slotDiceMaxBuffs[slotIndex]}");
        }
        int rolledDiceValue = diceRoll.Roll();
        
        float attackDamage = attacker.attackPower * (GameConstants.ATTACK_CONSTANT + Mathf.Pow(rolledDiceValue / 5.5f, 1.3f));
        bool isCritical = Random.value < attacker.critRate;
        float critMultiplier = isCritical ? attacker.critMultiplier : 1.0f;
        float resistance = defender.GetResistanceFor(attackerCardState.attackType);
        float finalDamage = attackDamage * (1 - damageReduction) * critMultiplier * resistance;

        if (GameConstants.DEBUG_MODE)
        {
            if (isCritical) Debug.Log($"[데미지 계산] {attacker.characterName}의 치명타 발생!");
            Debug.Log($"[데미지 계산] 기본 공격 피해: {attackDamage:F1}, 피해 감소율: {damageReduction * 100:F1}%, 최종 피해: {finalDamage:F1}");
        }

        return Mathf.Max(1, Mathf.FloorToInt(finalDamage));
    }

    IEnumerator ProcessRoundPhase(GamePhase phase, RegisteredCardSlot slot1, RegisteredCardSlot slot2)
    {
        var effectsToProcess = new List<EffectToSort>();
        CollectEffectsFromCard(effectsToProcess, phase, player1, player2, slot1, slot2);
        CollectEffectsFromCard(effectsToProcess, phase, player2, player1, slot2, slot1);
        SortEffects(effectsToProcess);
        foreach (var sortedEffect in effectsToProcess) { effectQueue.Enqueue(sortedEffect.execution); }
        yield return StartCoroutine(ProcessEffectQueue());
    }

    IEnumerator ProcessGlobalPhase(GamePhase phase)
    {
        var effectsToProcess = new List<EffectToSort>();
        foreach (var slot in player1.registeredSlots) if (slot != null) CollectEffectsFromCard(effectsToProcess, phase, player1, player2, slot, null);
        foreach (var slot in player2.registeredSlots) if (slot != null) CollectEffectsFromCard(effectsToProcess, phase, player2, player1, slot, null);
        SortEffects(effectsToProcess);
        foreach (var sortedEffect in effectsToProcess) { effectQueue.Enqueue(sortedEffect.execution); }
        yield return StartCoroutine(ProcessEffectQueue());
    }

    void CollectEffectsFromCard(List<EffectToSort> list, GamePhase phase, Player owner, Player opponent, RegisteredCardSlot ownerSlot, RegisteredCardSlot opponentSlot)
    {
        CardStateData currentStateData = GetStateData(ownerSlot);
        if (currentStateData == null || currentStateData.effects == null) return;
        foreach (var effectData in currentStateData.effects)
        {
            var paramsDict = new Dictionary<string, object>();
            if (effectData.parameters != null)
            {
                foreach (var param in effectData.parameters)
                {
                    if (int.TryParse(param.value, out int intValue)) { paramsDict.Add(param.key, intValue); }
                    else { paramsDict.Add(param.key, param.value); }
                }
            }
            CardEffect tempEffect = EffectFactory.CreateEffect(effectData.effectId, paramsDict);
            if (tempEffect != null && tempEffect.TriggerPhase == phase)
            {
                list.Add(new EffectToSort
                {
                    execution = new CardEffectExecution { effect = tempEffect, ownerPlayer = owner, opponentPlayer = opponent, ownerUser = ownerSlot.user, opponentUser = opponentSlot?.user },
                    speed = ownerSlot.cardSO.speed,
                    attackPower = DiceRollParser.Parse(currentStateData.attackDice).Roll(),
                    randomId = System.Guid.NewGuid()
                });
            }
        }
    }

    void SortEffects(List<EffectToSort> list)
    {
        list.Sort((a, b) =>
        {
            int speedComparison = b.speed.CompareTo(a.speed);
            if (speedComparison != 0) return speedComparison;
            int attackComparison = b.attackPower.CompareTo(a.attackPower);
            if (attackComparison != 0) return attackComparison;
            return a.randomId.CompareTo(b.randomId);
        });
    }

    CardStateData GetStateData(RegisteredCardSlot slot)
    {
        if (slot == null || slot.cardSO == null) return null;
        
        switch (slot.state)
        {
            case SlotState.Encroachment: return slot.cardSO.encroachmentState;
            case SlotState.Corrosion: return slot.cardSO.corrosionState;
            case SlotState.Revelation: return slot.cardSO.revelationState;
            default: return slot.cardSO.awakenedState;
        }
    }

    void ApplySanityDamage()
    {
        foreach (var character in player1.characters.Concat(player2.characters))
        {
            if (character.sanity == -15 && character.currentHp > 0)
            {
                int sanityDamage = Mathf.FloorToInt(character.maxHealth * 0.1f);
                if (GameConstants.DEBUG_MODE) Debug.Log($"{character.characterName}이(가) 낮은 정신력으로 {sanityDamage}의 피해를 입습니다!");
                character.TakeDamage(sanityDamage);
            }
        }
    }

    IEnumerator ProcessEffectQueue()
    {
        while (effectQueue.Count > 0)
        {
            CardEffectExecution exec = effectQueue.Dequeue();
            exec.effect.Execute(exec.ownerPlayer, exec.opponentPlayer, exec.ownerUser, exec.opponentUser);
            if (CheckForGameOver()) yield break;
            yield return new WaitForSeconds(1f);
        }
    }

    bool CheckForGameOver()
    {
        bool p1HasLivingChars = player1.characters.Any(c => c.currentHp > 0);
        bool p2HasLivingChars = player2.characters.Any(c => c.currentHp > 0);
        if (player1.characters.Count > 0 && !p1HasLivingChars) { Debug.Log($"게임 종료! {player2.playerName} 승리!"); Time.timeScale = 0; return true; }
        if (player2.characters.Count > 0 && !p2HasLivingChars) { Debug.Log($"게임 종료! {player1.playerName} 승리!"); Time.timeScale = 0; return true; }
        return false;
    }
    
    void HandleCharacterDeath(RegisteredCardSlot killerSlot, RegisteredCardSlot deadSlot)
    {
        Character killer = killerSlot.user;
        Character deadCharacter = deadSlot.user;

        if (GameConstants.DEBUG_MODE) Debug.Log($"{deadCharacter.characterName}이(가) 처치되었습니다!");

        killer.ChangeSanity(3);

        Player deadCharacterTeam = player1.characters.Contains(deadCharacter) ? player1 : player2;
        
        foreach (var ally in deadCharacterTeam.characters)
        {
            if (ally.currentHp > 0)
            {
                ally.ChangeSanity(-5);
            }
        }
    }
}