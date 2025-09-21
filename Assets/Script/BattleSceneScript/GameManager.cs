using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // All, Any, Concat 같은 LINQ 메소드 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    // =================================================================
    // Inspector에서 연결할 필드
    // =================================================================
    [Header("Player & Team Settings")]
    public Player player1;
    public Player player2;

    [Header("Asset References")]
    public CardDataSO dummyCard;
    public GameObject cardPrefab;

    [Header("Scene Transforms")]
    public Transform p1BattlePos;
    public Transform p2BattlePos;

    // =================================================================
    // 내부 변수
    // =================================================================
    private Dictionary<string, CardDataSO> cardDatabase;
    private Queue<CardEffectExecution> effectQueue = new Queue<CardEffectExecution>();
    private int currentMaxSlots;

    // =================================================================
    // 내부 구조체 정의
    // =================================================================
    private struct CardEffectExecution
    {
        public CardEffect effect;
        public Character ownerUser;
        public Character opponentUser;
    }

    private struct EffectToSort
    {
        public CardEffectExecution execution;
        public int speed;
        public int attackPower; // 정렬 목적으로만 사용 (주사위 굴림값)
        public System.Guid randomId;
    }

    // =================================================================
    // 유니티 생명주기 메소드 (초기화)
    // =================================================================
    void Awake()
    {
        LoadAllCardsFromAssets();
    }

    void Start()
    {
        // --- 테스트 시나리오 설정 ---
        player1.characters.Add(new Character("검사", 100, 15, 10, 15));
        player2.characters.Add(new Character("마법사", 80, 20, 5, -15));
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player1.characters[0] });
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player1.characters[0] });
        player1.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player1.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player2.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c001"], user = player2.characters[0] });
        player2.registeredSlots.Add(new RegisteredCardSlot { cardSO = cardDatabase["c002"], user = player2.characters[0] });

        StartCoroutine(BattleRoutine());
    }

    private void LoadAllCardsFromAssets()
    {
        cardDatabase = new Dictionary<string, CardDataSO>();
        var loadedCards = Resources.LoadAll<CardDataSO>("Cards");
        foreach (var cardSO in loadedCards)
        {
            if (!cardDatabase.ContainsKey(cardSO.cardId))
            {
                cardDatabase.Add(cardSO.cardId, cardSO);
            }
        }
    }

    // =================================================================
    // 메인 전투 흐름 (코루틴)
    // =================================================================
    IEnumerator BattleRoutine()
    {
        Debug.Log("--- 전투 시작 ---");

        int p1TotalSlots = player1.baseSlots + player1.bonusSlots;
        int p2TotalSlots = player2.baseSlots + player2.bonusSlots;
        currentMaxSlots = Mathf.Max(p1TotalSlots, p2TotalSlots);
        player1.bonusSlots = 0;
        player2.bonusSlots = 0;

        DetermineAllSlotStates();

        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnStart));
        if (CheckForGameOver()) yield break;

        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.CardReveal));
        if (CheckForGameOver()) yield break;

        for (int i = 0; i < currentMaxSlots; i++)
        {
            Debug.Log($"\n<<<<< 라운드 {i + 1} 시작 >>>>>");
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
            yield return StartCoroutine(ProcessSingleRound(p1Slot, p2Slot, p1CardView, p2CardView));

            p1CardView.DestroyCard();
            p2CardView.DestroyCard();
            yield return new WaitForSeconds(0.5f);

            if (CheckForGameOver()) yield break;
        }

        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnEnd));
        if (CheckForGameOver()) yield break;

        ApplySanityDamage();
        if (CheckForGameOver()) yield break;

        Debug.Log("\n--- 모든 페이즈 및 라운드 정상 종료 ---");
    }

    // =================================================================
    // 핵심 로직 메소드들
    // =================================================================

    void DetermineAllSlotStates()
    {
        foreach (var slot in player1.registeredSlots) DetermineSlotState(slot);
        foreach (var slot in player2.registeredSlots) DetermineSlotState(slot);
    }

    void DetermineSlotState(RegisteredCardSlot slot)
    {
        if (slot.user == null) return; // 더미 카드 등 사용자가 없는 경우
        
        slot.state = (Random.value > 0.5f) ? SlotState.Awakened : SlotState.Corrupted;
        
        if (slot.state == SlotState.Awakened && slot.user.sanity == 15 && slot.cardSO.HasAscendedState)
        {
            slot.state = SlotState.Ascended;
        }
        else if (slot.state == SlotState.Corrupted && slot.user.sanity == -15 && slot.cardSO.HasAbyssalState)
        {
            slot.state = SlotState.Abyssal;
        }
        Debug.Log($"{slot.user.characterName}의 카드 '{GetStateData(slot).stateName}' 최종 상태: {slot.state}");
    }

    IEnumerator ProcessSingleRound(RegisteredCardSlot slot1, RegisteredCardSlot slot2, CardView view1, CardView view2)
{
    // 1. 전투 전 페이즈 실행
    yield return StartCoroutine(ProcessRoundPhase(GamePhase.PreCombat, slot1, slot2));
    if (CheckForGameOver()) yield break;

    // 양쪽 모두 유효한 사용자가 있을 때만 우위 경쟁 진행
    if (slot1.user != null && slot2.user != null)
    {
        CardStateData stateData1 = GetStateData(slot1);
        CardStateData stateData2 = GetStateData(slot2);

        // 2. 우위를 정하기 위한 주사위 굴림
        Debug.Log("--- 우위 경쟁 ---");
        int p1InitiativeRoll = DiceRollParser.Parse(stateData1.attackDice).Roll();
        int p2InitiativeRoll = DiceRollParser.Parse(stateData2.attackDice).Roll();
        Debug.Log($"{slot1.user.characterName}의 주사위: {p1InitiativeRoll}  vs  {slot2.user.characterName}의 주사위: {p2InitiativeRoll}");

        RegisteredCardSlot winnerSlot = null;
        RegisteredCardSlot loserSlot = null;
        CardView winnerView = null;
        CardView loserView = null;

        // 3. 더 높은 값이 나온 사람이 승자가 됨
        if (p1InitiativeRoll > p2InitiativeRoll)
        {
            winnerSlot = slot1; winnerView = view1;
            loserSlot = slot2;  loserView = view2;
            Debug.Log($"우위 경쟁 승자: {winnerSlot.user.characterName}");
        }
        else if (p2InitiativeRoll > p1InitiativeRoll)
        {
            winnerSlot = slot2; winnerView = view2;
            loserSlot = slot1;  loserView = view1;
            Debug.Log($"우위 경쟁 승자: {winnerSlot.user.characterName}");
        }
        // ★★★ 추가된 조건: 값이 동일할 경우 ★★★
        else
        {
            Debug.Log("우위 경쟁 무승부! 이번 라운드의 전투는 무효 처리됩니다.");
            // winnerSlot을 null로 유지하여 아래의 공격 로직이 실행되지 않도록 합니다.
        }

        // 승자가 결정된 경우에만 공격 진행
        if (winnerSlot != null)
        {
            // 4. 공격권을 지닌 사람이 '새로' 피해를 계산
            Debug.Log($"--- {winnerSlot.user.characterName}의 공격 턴 ---");
            int finalDamage = CalculateFinalDamage(winnerSlot, loserSlot);

            // 5. 패자에게 계산된 만큼 피해를 줌
            loserSlot.user.TakeDamage(finalDamage);

            // 시각적 연출
            winnerView.PlayAttackAnimation(loserView.transform.position, winnerView.transform.position);
            yield return new WaitForSeconds(0.5f);
            loserView.PlayDamageEffect();
            if (CheckForGameOver()) yield break;
        }
    }

    yield return new WaitForSeconds(1f);

    // 6. 전투 후 페이즈 실행 (전투 발생 여부와 상관없이 실행)
    yield return StartCoroutine(ProcessRoundPhase(GamePhase.PostCombat, slot1, slot2));
}

    private int CalculateFinalDamage(RegisteredCardSlot attackerSlot, RegisteredCardSlot defenderSlot)
    {
        Character attacker = attackerSlot.user;
        Character defender = defenderSlot.user;
        CardStateData attackerCardState = GetStateData(attackerSlot);

        float damageReduction = GameConstants.MAX_DAMAGE_REDUCTION_RATE * (defender.defense / (float)(defender.defense + GameConstants.DEFENSE_CONSTANT));
        
        DiceRoll diceRoll = DiceRollParser.Parse(attackerCardState.attackDice);
        int rolledDiceValue = diceRoll.Roll();
        
        float attackDamage = attacker.attackPower * (GameConstants.ATTACK_CONSTANT + Mathf.Pow(rolledDiceValue / 5.5f, 1.3f));

        bool isCritical = Random.value < attacker.critRate;
        float critMultiplier = isCritical ? attacker.critMultiplier : 1.0f;
        if(isCritical) Debug.Log($"{attacker.characterName}의 치명타 발생!");

        float resistance = defender.GetResistanceFor(attackerCardState.attackType);

        float finalDamage = attackDamage * (1 - damageReduction) * critMultiplier * resistance;
        
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
                    execution = new CardEffectExecution { effect = tempEffect, ownerUser = ownerSlot.user, opponentUser = opponentSlot?.user },
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

    private int CalculateCombatBonus(Player owner, Player opponent, RegisteredCardSlot ownerSlot, RegisteredCardSlot opponentSlot)
    {
        int totalBonus = 0;
        CardStateData ownerStateData = GetStateData(ownerSlot);
        if (ownerStateData == null || ownerStateData.effects == null) return 0;

        foreach (var effectData in ownerStateData.effects)
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
            if (tempEffect != null)
            {
                totalBonus += tempEffect.GetCombatBonus(ownerSlot, opponentSlot);
            }
        }
        return totalBonus;
    }

    CardStateData GetStateData(RegisteredCardSlot slot)
    {
        if (slot == null || slot.cardSO == null) return null;
        switch (slot.state)
        {
            case SlotState.Ascended: return slot.cardSO.ascendedState;
            case SlotState.Abyssal: return slot.cardSO.abyssalState;
            case SlotState.Corrupted: return slot.cardSO.corruptedState;
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
                character.TakeDamage(sanityDamage);
                Debug.Log($"{character.characterName}이(가) 낮은 정신력으로 {sanityDamage}의 피해를 입습니다!");
            }
        }
    }

    IEnumerator ProcessEffectQueue()
    {
        while (effectQueue.Count > 0)
        {
            CardEffectExecution exec = effectQueue.Dequeue();
            exec.effect.Execute(exec.ownerUser, exec.opponentUser);
            if (CheckForGameOver()) yield break;
            yield return new WaitForSeconds(1f);
        }
    }

    bool CheckForGameOver()
    {
        bool p1HasLivingChars = player1.characters.Any(c => c.currentHp > 0);
        bool p2HasLivingChars = player2.characters.Any(c => c.currentHp > 0);

        if (player1.characters.Count > 0 && !p1HasLivingChars)
        {
            Debug.Log($"게임 종료! {player2.playerName} 승리!");
            Time.timeScale = 0;
            return true;
        }
        if (player2.characters.Count > 0 && !p2HasLivingChars)
        {
            Debug.Log($"게임 종료! {player1.playerName} 승리!");
            Time.timeScale = 0;
            return true;
        }
        return false;
    }
}