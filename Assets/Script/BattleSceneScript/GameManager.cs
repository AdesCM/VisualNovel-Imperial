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
    public BattleUIManager uiManager;

    [Header("Scene Transforms")]
    public Transform p1BattlePos;
    public Transform p2BattlePos;

    private Dictionary<string, CardDataSO> cardDatabase;
    private Dictionary<string, CharacterSO> characterDatabase;
    private Dictionary<string, EquipmentSO> equipmentDatabase;
    
    private Queue<CardEffectExecution> effectQueue = new Queue<CardEffectExecution>();
    private int currentMaxSlots;

    private bool isBattleOver = true; // 현재 전투가 끝났는지 여부를 나타내는 플래그

    private struct CardEffectExecution { public CardEffect effect; public Player ownerPlayer; public Player opponentPlayer; public Character ownerUser; public Character opponentUser; }
    private struct EffectToSort { public CardEffectExecution execution; public int speed; public int attackPower; public System.Guid randomId; }

    void Awake()
    {
        LoadAllCardsFromAssets();
        LoadAllCharactersFromAssets();
        LoadAllEquipmentFromAssets();
    }

    void Start()
    {
        StartCoroutine(GameLoop());
        /*
        // 1. 각 플레이어의 Agent를 찾아 연결합니다.
        player1.agent?.Setup(player1, this);
        player2.agent?.Setup(player2, this);

        // 2. 테스트 시나리오를 설정합니다.
        CharacterSO knightSO = characterDatabase["briram_spear"];
        CharacterSO mageSO = characterDatabase["mage_fire"];
        Character knight = new Character(knightSO);
        Character mage = new Character(mageSO);

        EquipmentSO longsword = equipmentDatabase["longsword_01"];
        knight.EquipItem(longsword);
        
        player1.characters.Add(knight);
        player2.characters.Add(mage);
        
        // 3. AI가 먼저 행동을 결정하고 UI에 표시합니다.
        if (player2.agent != null)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log("--- 적(AI) 턴 준비 시작 ---");
            player2.agent.PrepareTurn();
            DetermineAllSlotStates(player2);
            if (uiManager != null) uiManager.UpdateOpponentStatus();
            int initialMaxSlots = player1.baseSlots + player1.bonusSlots;
            uiManager.InitializeUI(player1, initialMaxSlots);
        }

        // 4. 이제 게임은 사용자(Player1)의 입력을 기다립니다.
        */
    }

    IEnumerator GameLoop()
    {
        // 1. 게임 시작 시 최초 설정
        InitialSetup();

        // 2. 게임이 끝날 때까지 무한 반복
        while (true)
        {
            // 3. 전투가 끝날 때까지 대기
            yield return new WaitUntil(() => isBattleOver);

            // 4. 전투가 끝나면, 다음 턴을 준비
            yield return new WaitForSeconds(2f); // 턴 사이에 잠시 대기
            StartNewTurn();
        }
    }

    void InitialSetup()
    {
        player1.agent?.Setup(player1, this);
        player2.agent?.Setup(player2, this);

        CharacterSO knightSO = characterDatabase["briram_spear"];
        CharacterSO mageSO = characterDatabase["mage_fire"];
        Character knight = new Character(knightSO);
        Character mage = new Character(mageSO);
        player1.characters.Add(knight);
        player2.characters.Add(mage);
        
        if (player2.agent != null)
        {
            player2.agent.PrepareTurn();
            DetermineAllSlotStates(player2);
            if (uiManager != null) uiManager.UpdateOpponentStatus();
        }

        if (uiManager != null)
        {
            int initialMaxSlots = player1.baseSlots + player1.bonusSlots;
            uiManager.InitializeUI(player1, initialMaxSlots); // 초기 UI 생성 및 8장 드로우
        }
    }

    void StartNewTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log("========== 새로운 턴 시작 ==========");

        // 1. 이전 턴의 등록된 카드 모두 삭제
        player1.registeredSlots.Clear();
        player2.registeredSlots.Clear();

        isBattleOver = false;

        // 2. AI에게 다음 턴 행동 준비 명령
        if (player2.agent != null)
        {
            player2.agent.PrepareTurn();
            DetermineAllSlotStates(player2);
            if (uiManager != null) uiManager.UpdateOpponentStatus();
        }

        // 3. 플레이어에게 4장의 카드 드로우 명령
        if (uiManager != null)
        {
            uiManager.DrawNewCards(4);
        }
    }
    

    // ★★★ 요청하신 경로가 적용된 로딩 함수들 ★★★
    private void LoadAllCardsFromAssets()
    {
        cardDatabase = new Dictionary<string, CardDataSO>();
        var loadedCards = Resources.LoadAll<CardDataSO>("SO/Cards");
        foreach (var cardSO in loadedCards) { cardDatabase.Add(cardSO.cardId, cardSO); }
    }
    
    private void LoadAllCharactersFromAssets()
    {
        characterDatabase = new Dictionary<string, CharacterSO>();
        var loadedCharacters = Resources.LoadAll<CharacterSO>("SO/Characters");
        foreach (var charSO in loadedCharacters) { characterDatabase.Add(charSO.characterId, charSO); }
    }
    
    private void LoadAllEquipmentFromAssets()
    {
        equipmentDatabase = new Dictionary<string, EquipmentSO>();
        var loadedEquipment = Resources.LoadAll<EquipmentSO>("SO/Equipments");
        foreach (var equipSO in loadedEquipment) { equipmentDatabase.Add(equipSO.equipmentId, equipSO); }
    }
    
    public void StartCombat()
    {
        // 이미 전투가 진행 중이면(isBattleOver가 false이면) 중복 실행 방지
        if (!isBattleOver) return;

        // "이제 전투를 시작할 준비가 되었다"는 신호를 보냅니다.
        // isBattleOver를 true로 유지하여, GameLoop가 이 신호를 받을 수 있게 합니다.
        
        // 이 함수가 직접 BattleRoutine을 시작하는 대신, isBattleOver 플래그만 관리하도록 할 수 있습니다.
        // 하지만 현재 Agent 시스템과 연동하려면 약간 더 복잡해집니다.
        
        // 더 간단하고 확실한 해결책으로 돌아가겠습니다.
        // BattleRoutine이 끝나고 다음 턴을 준비하는 것으로 역할을 명확히 합니다.

        if (GameConstants.DEBUG_MODE) Debug.Log("--- 턴 종료 버튼 입력: 전투 시작 ---");
        isBattleOver = false; // "전투 시작" 신호
        StartCoroutine(BattleRoutine());
    }
    
    public CardDataSO GetCardData(string cardId)
    {
        if (cardDatabase.ContainsKey(cardId)) { return cardDatabase[cardId]; }
        Debug.LogError($"CardDatabase에 ID가 '{cardId}'인 카드가 없습니다!");
        return null;
    }

    IEnumerator BattleRoutine()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log("--- 전투 시작 ---");

        int p1TotalSlots = player1.baseSlots + player1.bonusSlots;
        int p2TotalSlots = player2.baseSlots + player2.bonusSlots;
        currentMaxSlots = Mathf.Max(p1TotalSlots, p2TotalSlots);
        
        
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnStart));
        if (CheckForGameOver()) yield break;
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
            yield return StartCoroutine(ProcessSingleRound(p1Slot, p2Slot, p1CardView, p2CardView, i)); 
            
            p1CardView.DestroyCard();
            p2CardView.DestroyCard();
            yield return new WaitForSeconds(0.5f);
            
            if (CheckForGameOver()) yield break;
        }
        
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnEnd));
        if (CheckForGameOver()) yield break;
        ApplySanityDamage();
        if (CheckForGameOver()) yield break;
        
        player1.ClearAllSlotBuffs();
        player2.ClearAllSlotBuffs();
        if (GameConstants.DEBUG_MODE) Debug.Log("\n--- 모든 페이즈 및 라운드 정상 종료 ---");
        yield return new WaitForSeconds(2f); // 턴 사이에 잠시 대기

        // 1. 이전 턴의 등록된 카드 모두 삭제
        player1.registeredSlots.Clear();
        player2.registeredSlots.Clear();

        // 2. AI에게 다음 턴 행동 준비 명령
        if (player2.agent != null)
        {
            player2.agent.PrepareTurn();
            DetermineAllSlotStates(player2);
            if (uiManager != null) uiManager.UpdateOpponentStatus();
        }

        // 3. 플레이어에게 4장의 카드 드로우 명령
        if (uiManager != null)
        {
            uiManager.DrawNewCards(4);
        }

        // 4. 모든 준비가 끝났으므로, 다시 "전투 끝남"(입력 대기) 상태로 전환
        isBattleOver = true;
    }

    void DetermineAllSlotStates(Player player)
    {
        foreach (var slot in player.registeredSlots) DetermineSlotState(slot);
    }
    
    void DetermineSlotState(RegisteredCardSlot slot)
    {
        if (slot.user == null) { slot.state = SlotState.Awakened; return; }
        slot.state = (slot.user.sanity >= 0) ? SlotState.Awakened : SlotState.Revelation;
        if (slot.state == SlotState.Awakened && slot.user.sanity == 15 && slot.cardSO.HasEncroachmentState) slot.state = SlotState.Encroachment;
        else if (slot.state == SlotState.Revelation && slot.user.sanity == -15 && slot.cardSO.HasCorrosionState) slot.state = SlotState.Corrosion;
        if (GameConstants.DEBUG_MODE) Debug.Log($"{slot.user.characterName}의 카드 '{GetStateData(slot).stateName}' 최종 상태: {slot.state}");
    }

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
            Player winnerPlayer = null, loserPlayer = null;
            CardView winnerView = null, loserView = null;
            int winnerIndex = -1, loserIndex = -1;

            if (p1InitiativeRoll > p2InitiativeRoll) { winnerSlot = slot1; loserSlot = slot2; winnerPlayer = player1; loserPlayer = player2; winnerView = view1; loserView = view2; winnerIndex = roundIndex; loserIndex = roundIndex; }
            else if (p2InitiativeRoll > p1InitiativeRoll) { winnerSlot = slot2; loserSlot = slot1; winnerPlayer = player2; loserPlayer = player1; winnerView = view2; loserView = view1; winnerIndex = roundIndex; loserIndex = roundIndex; }

            if (winnerSlot != null)
            {
                if (GameConstants.DEBUG_MODE) Debug.Log($"[전투] 우위 경쟁 승자: {winnerSlot.user.characterName}");
                if (GameConstants.DEBUG_MODE) Debug.Log($"--- {winnerSlot.user.characterName}의 '{GetStateData(winnerSlot).stateName}' 공격 ---");

                Character attacker = winnerSlot.user;
                Character defender = loserSlot.user;
                int finalDamage = CalculateFinalDamage(winnerSlot, loserSlot, winnerPlayer, winnerIndex);
                defender.TakeDamage(finalDamage);

                attacker.ChangeSanity(2);
                defender.ChangeSanity(-3);

                if (GameConstants.DEBUG_MODE)
                {
                    string attackerStatus = $"공격자: {attacker.characterName} | HP: {attacker.currentHp}/{attacker.GetFinalMaxHealth()} | 정신력: {attacker.sanity} | 장비: {GetEquipmentList(attacker)} | 카드: {GetStateData(winnerSlot).stateName}";
                    string defenderStatus = $"방어자: {defender.characterName} | HP: {defender.currentHp}/{defender.GetFinalMaxHealth()} | 정신력: {defender.sanity} | 장비: {GetEquipmentList(defender)} | 카드: {GetStateData(loserSlot).stateName}";
                    Debug.Log($"[전투 결과]\n{attackerStatus}\n{defenderStatus}");
                }

                if (defender.currentHp <= 0) { HandleCharacterDeath(winnerSlot, loserSlot); }

                winnerView.PlayAttackAnimation(loserView.transform.position, winnerView.transform.position);
                yield return new WaitForSeconds(0.5f);
                loserView.PlayDamageEffect();
                if (CheckForGameOver()) yield break;
            }
            else { if (GameConstants.DEBUG_MODE) Debug.Log("[전투] 우위 경쟁 무승부! 전투가 무효 처리됩니다."); }
        }
        yield return new WaitForSeconds(1f);
        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 전투 후 ---");
        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PostCombat, slot1, slot2));
    }

    private int CalculateFinalDamage(RegisteredCardSlot attackerSlot, RegisteredCardSlot defenderSlot, Player attackerPlayer, int slotIndex)
    {
        Character attacker = attackerSlot.user;
        Character defender = defenderSlot.user;
        CardStateData attackerCardState = GetStateData(attackerSlot);

        float damageReduction = GameConstants.MAX_DAMAGE_REDUCTION_RATE * (defender.GetFinalDefense() / (float)(defender.GetFinalDefense() + GameConstants.DEFENSE_CONSTANT));
        
        DiceRoll diceRoll = DiceRollParser.Parse(attackerCardState.attackDice);
        if (attackerPlayer.slotDiceCountBuffs.ContainsKey(slotIndex)) { diceRoll.numberOfDice += attackerPlayer.slotDiceCountBuffs[slotIndex]; }
        if (attackerPlayer.slotDiceMaxBuffs.ContainsKey(slotIndex)) { diceRoll.sidesOfDice += attackerPlayer.slotDiceMaxBuffs[slotIndex]; }
        int rolledDiceValue = diceRoll.Roll();
        
        float attackDamage = attacker.GetFinalAttackPower() * (GameConstants.ATTACK_CONSTANT + Mathf.Pow(rolledDiceValue / 5.5f, 1.3f));
        bool isCritical = Random.value < attacker.GetFinalCritRate();
        float critMultiplier = isCritical ? attacker.GetFinalCritMultiplier() : 1.0f;
        float resistance = defender.GetFinalResistanceFor(attackerCardState.attackType);
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
                    attackPower = ownerSlot.user.GetFinalAttackPower(), 
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
    
    string GetEquipmentList(Character character)
    {
        if (character == null || character.equippedItems.Count == 0) return "없음";
        return string.Join(", ", character.equippedItems.Values.Select(item => item.equipmentName));
    }

    void ApplySanityDamage()
    {
        foreach (var character in player1.characters.Concat(player2.characters))
        {
            if (character.sanity == -15 && character.currentHp > 0)
            {
                character.TakeDamage(Mathf.FloorToInt(character.GetFinalMaxHealth() * 0.1f));
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
            if (ally.currentHp > 0) { ally.ChangeSanity(-5); }
        }
    }

    public void OnAgentTurnFinished(Player player)
    {
        player.isTurnFinished = true;

        // 양쪽 플레이어가 모두 준비되었는지 확인
        if (player1.isTurnFinished && player2.isTurnFinished)
        {
            // 모두 준비되었으면 전투 코루틴 시작
            StartCoroutine(BattleRoutine());
        }
    }
}