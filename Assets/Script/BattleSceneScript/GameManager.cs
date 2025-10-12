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
    private StageData currentStageData;
    private int currentWaveIndex = 0;

    private struct CardEffectExecution { public CardEffect effect; public Player ownerPlayer; public Player opponentPlayer; public Character ownerUser; public Character opponentUser; }
    private struct EffectToSort { public CardEffectExecution execution; public int speed; public int attackPower; public System.Guid randomId; public RuntimeCard ownerCard; }

    private int currentTurn = 0; // 현재 턴 번호 기록

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
        //InitialSetup();
        yield return StartCoroutine(InitialSetup());

        // 2. 게임이 끝날 때까지 무한 반복
        while (true)
        {
            // 3. 전투가 끝날 때까지 대기
            yield return new WaitUntil(() => isBattleOver);

            // 4. 웨이브가 클리어되었는지 확인
            bool isEnemyTeamWiped = !player2.characters.Any(c => c.currentHp > 0);
            if (isEnemyTeamWiped)
            {
                currentWaveIndex++;
                if (currentWaveIndex < currentStageData.Waves.Count)
                {
                    // 다음 웨이브 시작
                    yield return StartCoroutine(StartWave(currentWaveIndex));
                }
                else
                {
                    // 스테이지 클리어
                    StageClear();
                    yield break; // GameLoop 종료
                }
            }

            // 4. 전투가 끝나면, 다음 턴을 준비
            yield return new WaitForSeconds(2f); // 턴 사이에 잠시 대기
            StartNewTurn();
        }
    }
    IEnumerator InitialSetup()
    {
        currentStageData = StageManager.Instance.GetCurrentStageData();
        if (currentStageData == null)
        {
            Debug.LogError("현재 스테이지 정보를 찾을 수 없습니다!");
            yield break;
        }

        // --- 1. Player 1 (User) 캐릭터 생성 및 설정 ---
        player1.agent?.Setup(player1, this, null);
        player1.characters.Clear();
        // (향후 이 부분도 PlayerDataManager에서 어떤 캐릭터를 선택했는지 받아와야 함)
        CharacterSO knightSO = characterDatabase["briram_spear"];
        Character knight = new Character(knightSO);
        EquipmentSO longsword = equipmentDatabase["longsword_01"];
        knight.EquipItem(longsword);
        player1.characters.Add(knight);
        
        // --- 2. Player 2 (AI) 설정 ---
        player2.characters.Clear();
        foreach (var enemyId in currentStageData.Waves[0])
        {
            CharacterSO enemySO = characterDatabase[enemyId];
            player2.characters.Add(new Character(enemySO));
        }
        AIPatternSO aiPattern = StageManager.Instance.GetAIPattern(currentStageData.AIPatternID);
        player2.agent?.Setup(player2, this, aiPattern);
        
        // ★★★ 3. PlayerDataManager를 확인하여 덱 구성 ★★★
        player1.masterDeck.Clear();
        // '여행 가방'이 있고, 그 안에 덱 정보가 들어있다면
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.PlayerDeck.Count > 0)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log("PlayerDataManager로부터 덱을 구성합니다.");
            foreach (var deckInfo in PlayerDataManager.Instance.PlayerDeck)
            {
                CardDataSO cardSO = GetCardData(deckInfo.CardID);
                Character caster = player1.characters.FirstOrDefault(c => c.blueprint.characterId == deckInfo.CasterID);
                if (cardSO != null && caster != null)
                {
                    player1.masterDeck.Add(new RuntimeCard { CardSO = cardSO, Caster = caster });
                }
            }
        }
        else // '여행 가방'이 없거나 비어있다면 (디버깅용 대체 로직)
        {
            if (GameConstants.DEBUG_MODE) Debug.LogWarning("PlayerDataManager 덱 정보 없음. 캐릭터 고유 스킬로 기본 덱을 구성합니다.");
            foreach (var character in player1.characters)
            {
                foreach (var skillSO in character.GetAvailableSkills())
                {
                    player1.masterDeck.Add(new RuntimeCard { CardSO = skillSO, Caster = character });
                }
            }
        }
        
        // ★★★ 4. UI 초기화 명령 (BattleUIManager는 이제 덱 구성을 하지 않음) ★★★
        if (uiManager != null)
        {
            int initialMaxSlots = player1.baseSlots + player1.bonusSlots;
            uiManager.InitializePlayerUI(player1, initialMaxSlots);
        }
        
        // --- 5. 첫 턴 준비 시작 ---
        StartNewTurn();
        yield return null;
    }

     /* 20251012 덱 로드 로직 변경으로 인한 주석처리
     IEnumerator InitialSetup()
    {
        currentStageData = StageManager.Instance.GetCurrentStageData();
        if (currentStageData == null)
        {
            Debug.LogError("현재 스테이지 정보를 찾을 수 없습니다!");
            yield break;
        }



        // --- Player 1 (User) 설정 ---
        player1.agent?.Setup(player1, this, null);
        CharacterSO knightSO = characterDatabase["briram_spear"];
        Character knight = new Character(knightSO);
        EquipmentSO longsword = equipmentDatabase["longsword_01"];
        knight.EquipItem(longsword);
        player1.characters.Add(knight);
        
        // --- Player 2 (AI) 설정 ---
        player2.characters.Clear();
        foreach (var enemyId in currentStageData.Waves[0])
        {
            CharacterSO enemySO = characterDatabase[enemyId];
            player2.characters.Add(new Character(enemySO));
        }
        AIPatternSO aiPattern = StageManager.Instance.GetAIPattern(currentStageData.AIPatternID);
        player2.agent?.Setup(player2, this, aiPattern); // 이제 Skill Pool이 정상적으로 구성됩니다.


        player1.masterDeck.Clear();
        // '여행 가방'이 있고, 그 안에 덱 정보가 들어있다면
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.PlayerDeckCardIDs.Count > 0)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log("PlayerDataManager로부터 덱을 구성합니다.");
            foreach (var cardId in PlayerDataManager.Instance.PlayerDeckCardIDs)
            {
                CardDataSO cardSO = GetCardData(cardId);
                if (cardSO != null)
                {
                    // 덱의 모든 카드는 플레이어의 첫 번째 캐릭터가 시전한다고 가정
                    player1.masterDeck.Add(new RuntimeCard { CardSO = cardSO, Caster = player1.characters[0] });
                }
            }
        }
        else // '여행 가방'이 없거나 비어있다면 (디버깅용 대체 로직)
        {
            if (GameConstants.DEBUG_MODE) Debug.LogWarning("PlayerDataManager 덱 정보 없음. 캐릭터 고유 스킬로 기본 덱을 구성합니다.");
            foreach (var skillSO in knight.GetAvailableSkills())
            {
                player1.masterDeck.Add(new RuntimeCard { CardSO = skillSO, Caster = knight });
            }
        }



        if (uiManager != null)
        {
            int initialMaxSlots = player1.baseSlots + player1.bonusSlots;
            uiManager.InitializePlayerUI(player1, initialMaxSlots);
        }
        
        // --- 첫 턴 준비 ---
        StartNewTurn();
    }
    */

    void StartNewTurn()
    {
        currentTurn++;
        if (GameConstants.DEBUG_MODE) Debug.Log("========== 새로운 턴 시작 ==========");

        // 1. 이전 턴의 등록된 카드 모두 삭제
        player1.registeredCards.Clear();
        player2.registeredCards.Clear();

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
            int maxSlots = player1.baseSlots + player1.bonusSlots;
            uiManager.InitializeRegisteredSlots(maxSlots);
            uiManager.DrawNewCards(4);
        }
    }

    IEnumerator StartWave(int waveIndex)
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"<<<<< WAVE {waveIndex + 1} 시작 >>>>>");
        player2.characters.Clear();
        foreach (var enemyId in currentStageData.Waves[waveIndex])
        {
            CharacterSO enemySO = characterDatabase[enemyId];
            player2.characters.Add(new Character(enemySO));
        }
        // AI Agent는 이미 존재하므로 Setup을 다시 호출할 필요는 없지만, 스킬 풀 갱신 등 필요 시 호출 가능
        (player2.agent as SinglePlayAI_Agent)?.BuildDynamicSkillPool(); // 예시: 스킬 풀 재구성
        
        // 다음 턴 준비 (AI 행동 결정 및 UI 업데이트)
        StartNewTurn();
        yield return null;
    }

    void StageClear()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log("🎉🎉🎉 스테이지 클리어! 🎉🎉🎉");
    }
    

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
        if (isBattleOver) return;

        if (GameConstants.DEBUG_MODE) Debug.Log("--- 턴 종료 버튼 입력: 전투 시작 ---");
        DetermineAllSlotStates(player1);
        //isBattleOver = false; // "전투 시작" 신호
        StartCoroutine(BattleRoutine());
    }
    
    public CardDataSO GetCardData(string cardId)
    {
        if (cardDatabase.ContainsKey(cardId)) { return cardDatabase[cardId]; }
        Debug.LogError($"CardDatabase에 ID가 '{cardId}'인 카드가 없습니다!");
        return null;
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }

    IEnumerator BattleRoutine()
    {
        isBattleOver = false;
        if (GameConstants.DEBUG_MODE) Debug.Log("--- 전투 시작 ---");

        int p1TotalSlots = player1.baseSlots + player1.bonusSlots;
        int p2TotalSlots = player2.baseSlots + player2.bonusSlots;
        currentMaxSlots = Mathf.Max(p1TotalSlots, p2TotalSlots);
        
        
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnStart));
        if (CheckForGameOver()) { isBattleOver = true; yield break; }
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.CardReveal));
        if (CheckForGameOver()) { isBattleOver = true; yield break; }

        for (int i = 0; i < currentMaxSlots; i++)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log($"\n<<<<< 라운드 {i + 1} 시작 >>>>>");
            
            bool p1HasCard = i < player1.registeredCards.Count;
            bool p2HasCard = i < player2.registeredCards.Count;
            RuntimeCard p1Card = p1HasCard ? player1.registeredCards[i] : new RuntimeCard { CardSO = dummyCard, Caster = null };
            RuntimeCard p2Card = p2HasCard ? player2.registeredCards[i] : new RuntimeCard { CardSO = dummyCard, Caster = null };
            
            GameObject p1CardObject = Instantiate(cardPrefab, p1BattlePos.position, Quaternion.identity);
            CardView p1CardView = p1CardObject.GetComponent<CardView>();
            p1CardView.Setup(p1Card.CardSO, GetStateData(p1Card));
            
            GameObject p2CardObject = Instantiate(cardPrefab, p2BattlePos.position, Quaternion.identity);
            CardView p2CardView = p2CardObject.GetComponent<CardView>();
            p2CardView.Setup(p2Card.CardSO, GetStateData(p2Card));

            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(ProcessSingleRound(p1Card, p2Card, p1CardView, p2CardView, i)); 
            
            p1CardView.DestroyCard();
            p2CardView.DestroyCard();
            yield return new WaitForSeconds(0.5f);
            
            if (CheckForGameOver()) { isBattleOver = true; yield break; }
        }
        
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnEnd));
        if (CheckForGameOver()) { isBattleOver = true; yield break; }
        ApplySanityDamage();
        if (CheckForGameOver()) { isBattleOver = true; yield break; }
        
        player1.ClearAllSlotBuffs();
        player2.ClearAllSlotBuffs();
        if (GameConstants.DEBUG_MODE) Debug.Log("\n--- 모든 페이즈 및 라운드 정상 종료 ---");
        yield return new WaitForSeconds(2f); // 턴 사이에 잠시 대기


        // 4. 모든 준비가 끝났으므로, 다시 "전투 끝남"(입력 대기) 상태로 전환
        isBattleOver = true;
    }

    void DetermineAllSlotStates(Player player)
    {
        foreach (var card in player.registeredCards) DetermineSlotState(card);
    }
    
    void DetermineSlotState(RuntimeCard card)
    {
        if (card.Caster == null) { card.State = SlotState.Awakened; return; }
        card.State = (card.Caster.sanity >= 0) ? SlotState.Awakened : SlotState.Revelation;
        if (card.State == SlotState.Awakened && card.Caster.sanity == 15 && card.CardSO.HasEncroachmentState) card.State = SlotState.Encroachment;
        else if (card.State == SlotState.Revelation && card.Caster.sanity == -15 && card.CardSO.HasCorrosionState) card.State = SlotState.Corrosion;
        if (GameConstants.DEBUG_MODE) Debug.Log($"{card.Caster.characterName}의 카드 '{GetStateData(card).stateName}' 최종 상태: {card.State}");
    }

    IEnumerator ProcessSingleRound(RuntimeCard card1, RuntimeCard card2, CardView view1, CardView view2, int roundIndex)
    {
        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 전투 전 ---");
        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PreCombat, card1, card2));
        if (CheckForGameOver()) yield break;

        RuntimeCard winnerCard = null, loserCard = null;
        Player winnerPlayer = null;
        CardView winnerView = null, loserView = null;
        int winnerIndex = -1;

        // ★★★ 수정된 부분: 우위 경쟁 및 일방 공격 로직 통합 ★★★
        if (card1.Caster != null && card2.Caster != null)
        {
            // 1. 양쪽 모두 카드를 낸 경우: 기존의 우위 경쟁 진행
            if (GameConstants.DEBUG_MODE) Debug.Log("--- [전투] 우위 경쟁 ---");
            int p1InitiativeRoll = DiceRollParser.Parse(GetStateData(card1).attackDice).Roll();
            int p2InitiativeRoll = DiceRollParser.Parse(GetStateData(card2).attackDice).Roll();
            if (GameConstants.DEBUG_MODE) Debug.Log($"{card1.Caster.characterName} 주사위: {p1InitiativeRoll}  vs  {card2.Caster.characterName} 주사위: {p2InitiativeRoll}");

            if (p1InitiativeRoll > p2InitiativeRoll) { winnerCard = card1; loserCard = card2; winnerPlayer = player1; winnerView = view1; loserView = view2; winnerIndex = roundIndex; }
            else if (p2InitiativeRoll > p1InitiativeRoll) { winnerCard = card2; loserCard = card1; winnerPlayer = player2; winnerView = view2; loserView = view1; winnerIndex = roundIndex; }
            else { if (GameConstants.DEBUG_MODE) Debug.Log("[전투] 우위 경쟁 무승부! 전투가 무효 처리됩니다."); }
        }
        else if (card1.Caster != null && card2.Caster == null)
        {
            // 2. 플레이어1만 카드를 낸 경우: 일방 공격
            if (GameConstants.DEBUG_MODE) Debug.Log($"--- [전투] {player1.playerName}의 일방 공격 ---");
            winnerCard = card1;
            winnerPlayer = player1;
            winnerView = view1;
            winnerIndex = roundIndex;
            // 패자는 특정 캐릭터가 아니므로, 랜덤한 생존 적을 대상으로 지정
            var aliveEnemies = player2.characters.Where(c => c.currentHp > 0).ToList();
            if (aliveEnemies.Count > 0)
            {
                Character randomTarget = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
                loserCard = new RuntimeCard { Caster = randomTarget }; // 피해를 받을 대상만 임시로 지정
                loserView = view2; // 시각 효과를 위해 View는 그대로 사용
            }
        }
        else if (card1.Caster == null && card2.Caster != null)
        {
            // 3. 플레이어2만 카드를 낸 경우: 일방 공격
            if (GameConstants.DEBUG_MODE) Debug.Log($"--- [전투] {player2.playerName}의 일방 공격 ---");
            winnerCard = card2;
            winnerPlayer = player2;
            winnerView = view2;
            winnerIndex = roundIndex;
            var aliveEnemies = player1.characters.Where(c => c.currentHp > 0).ToList();
            if (aliveEnemies.Count > 0)
            {
                Character randomTarget = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
                loserCard = new RuntimeCard { Caster = randomTarget };
                loserView = view1;
            }
        }

        // ★★★ 공통 피해 처리 로직 ★★★
        if (winnerCard != null && loserCard != null && loserCard.Caster != null)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log($"[전투] 우위 경쟁 승자: {winnerCard.Caster.characterName}");
            if (GameConstants.DEBUG_MODE) Debug.Log($"--- {winnerCard.Caster.characterName}의 '{GetStateData(winnerCard).stateName}' 공격 ---");

            Character attacker = winnerCard.Caster;
            Character defender = loserCard.Caster;
            int finalDamage = CalculateFinalDamage(winnerCard, loserCard, winnerPlayer, winnerIndex);
            defender.TakeDamage(finalDamage);
            attacker.ChangeSanity(2);
            defender.ChangeSanity(-3);

            if (GameConstants.DEBUG_MODE)
            {
                string attackerStatus = $"공격자: {attacker.characterName} | HP: {attacker.currentHp}/{attacker.GetFinalMaxHealth()} | 정신력: {attacker.sanity} | 카드: {GetStateData(winnerCard).stateName}";
                string defenderStatus = $"방어자: {defender.characterName} | HP: {defender.currentHp}/{defender.GetFinalMaxHealth()} | 정신력: {defender.sanity}";
                Debug.Log($"[전투 결과]\n{attackerStatus}\n{defenderStatus}");
            }

            if (defender.currentHp <= 0) { HandleCharacterDeath(winnerCard, loserCard); }

            winnerView.PlayAttackAnimation(loserView.transform.position, winnerView.transform.position);
            yield return new WaitForSeconds(0.5f);
            if (loserView != null) loserView.PlayDamageEffect();
            if (CheckForGameOver()) yield break;
        }

        yield return new WaitForSeconds(1f);
        if (GameConstants.DEBUG_MODE) Debug.Log("--- [페이즈] 전투 후 ---");
        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PostCombat, card1, card2));
    }
//if (GameConstants.DEBUG_MODE) Debug.Log($"[전투] 우위 경쟁 승자: {winnerCard.Caster.characterName}");
    private int CalculateFinalDamage(RuntimeCard attackerCard, RuntimeCard defenderCard, Player attackerPlayer, int slotIndex)
    {
        Character attacker = attackerCard.Caster;
        Character defender = defenderCard.Caster;
        CardStateData attackerCardState = GetStateData(attackerCard);

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

    IEnumerator ProcessRoundPhase(GamePhase phase, RuntimeCard card1, RuntimeCard card2)
    {
        var effectsToProcess = new List<EffectToSort>();
        CollectEffectsFromCard(effectsToProcess, phase, player1, player2, card1, card2);
        CollectEffectsFromCard(effectsToProcess, phase, player2, player1, card2, card1);
        SortEffects(effectsToProcess);
        foreach (var sortedEffect in effectsToProcess) { effectQueue.Enqueue(sortedEffect.execution); }
        yield return StartCoroutine(ProcessEffectQueue());
    }

    IEnumerator ProcessGlobalPhase(GamePhase phase)
    {
        var effectsToProcess = new List<EffectToSort>();
        foreach (var card in player1.registeredCards) if (card != null) CollectEffectsFromCard(effectsToProcess, phase, player1, player2, card, null);
        foreach (var card in player2.registeredCards) if (card != null) CollectEffectsFromCard(effectsToProcess, phase, player2, player1, card, null);
        SortEffects(effectsToProcess);
        foreach (var sortedEffect in effectsToProcess) { effectQueue.Enqueue(sortedEffect.execution); }
        yield return StartCoroutine(ProcessEffectQueue());
    }

    void CollectEffectsFromCard(List<EffectToSort> list, GamePhase phase, Player owner, Player opponent, RuntimeCard ownerCard, RuntimeCard opponentCard)
    {
        CardStateData currentStateData = GetStateData(ownerCard);
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
                    execution = new CardEffectExecution { effect = tempEffect, ownerPlayer = owner, opponentPlayer = opponent, ownerUser = ownerCard.Caster, opponentUser = opponentCard?.Caster },
                    speed = ownerCard.CardSO.speed,
                    attackPower = ownerCard.Caster.GetFinalAttackPower(), 
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
    
    public CardStateData GetStateData(RuntimeCard card)
    {
        if (card == null || card.CardSO == null) return null;
        switch (card.State)
        {
            case SlotState.Encroachment: return card.CardSO.encroachmentState;
            case SlotState.Corrosion: return card.CardSO.corrosionState;
            case SlotState.Revelation: return card.CardSO.revelationState;
            default: return card.CardSO.awakenedState;
        }
    }

    public CardStateData GetStateData(SlotState state, CardDataSO cardSO)
    {
        if (cardSO == null) return null;
        switch (state)
        {
            case SlotState.Encroachment: return cardSO.encroachmentState;
            case SlotState.Corrosion: return cardSO.corrosionState;
            case SlotState.Revelation: return cardSO.revelationState;
            default: return cardSO.awakenedState;
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
    
    void HandleCharacterDeath(RuntimeCard killerCard, RuntimeCard deadCard)
    {
        Character killer = killerCard.Caster;
        Character deadCharacter = deadCard.Caster;
        if (GameConstants.DEBUG_MODE) Debug.Log($"{deadCharacter.characterName}이(가) 처치되었습니다!");
        killer.ChangeSanity(3);
        Player deadCharacterTeam = player1.characters.Contains(deadCharacter) ? player1 : player2;
        foreach (var ally in deadCharacterTeam.characters)
        {
            if (ally.currentHp > 0) { ally.ChangeSanity(-5); }
        }
        if (!player2.characters.Any(c => c.currentHp > 0))
        {
            isBattleOver = true;
        }
    }
     //일단 주석처리. 근데 이건 멀티플레이에서나 필요한거 아닌가? 나중에 멀티플레이 배틀씬 만들고 생각해보자. 이 Scene에서는 사용하지 않을 것지만 기억하는 용도로 남겨둠
     /*
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
    */
    

    
}