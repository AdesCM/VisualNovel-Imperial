using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Inspector에서 연결
    public Player player1;
    public Player player2;
    public CardDataSO dummyCard; // '발악' 카드로 사용할 ScriptableObject 에셋
    public GameObject cardPrefab; 
    public Transform p1BattlePos, p2BattlePos;

    // 내부 변수
    private Dictionary<string, CardDataSO> cardDatabase;
    private Queue<CardEffectExecution> effectQueue = new Queue<CardEffectExecution>();
    private int currentMaxSlots;
    
    // 구조체 정의
    private struct CardEffectExecution { public CardEffect effect; public Player owner; public Player opponent; }
    private struct EffectToSort { public CardEffectExecution execution; public int speed; public int attackPower; public System.Guid randomId; }

    void Awake()
    {
        LoadAllCardsFromAssets(); 
    }

    void Start()
    {
        // 테스트용 카드 등록 (실제 게임에서는 다른 로직으로 대체)
        if (cardDatabase.ContainsKey("c001")) player1.registeredCards.Add(cardDatabase["c001"]);
        if (cardDatabase.ContainsKey("c004")) player2.registeredCards.Add(cardDatabase["c004"]);
        
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

    IEnumerator BattleRoutine()
    {
        Debug.Log("--- 전투 시작 ---");

        int p1TotalSlots = player1.baseSlots + player1.bonusSlots;
        int p2TotalSlots = player2.baseSlots + player2.bonusSlots;
        currentMaxSlots = Mathf.Max(p1TotalSlots, p2TotalSlots);
        player1.bonusSlots = 0;
        player2.bonusSlots = 0;

        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnStart));
        if (CheckForGameOver()) yield break;
        
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.CardReveal));
        if (CheckForGameOver()) yield break;
        
        for (int i = 0; i < currentMaxSlots; i++)
        {
            Debug.Log($"\n<<<<< 라운드 {i + 1} 시작 >>>>>");
            bool p1HasCard = i < player1.registeredCards.Count;
            bool p2HasCard = i < player2.registeredCards.Count;

            // 한쪽이라도 카드가 없으면 dummyCard('발악' 카드)로 대체
            CardDataSO p1CardData = p1HasCard ? player1.registeredCards[i] : dummyCard;
            CardDataSO p2CardData = p2HasCard ? player2.registeredCards[i] : dummyCard;

            GameObject p1CardObject = Instantiate(cardPrefab, p1BattlePos.position, Quaternion.identity);
            CardView p1CardView = p1CardObject.GetComponent<CardView>();
            p1CardView.Setup(p1CardData);
            
            GameObject p2CardObject = Instantiate(cardPrefab, p2BattlePos.position, Quaternion.identity);
            CardView p2CardView = p2CardObject.GetComponent<CardView>();
            p2CardView.Setup(p2CardData);

            yield return new WaitForSeconds(1f);

            // ★★★ 수정된 핵심 로직 ★★★
            // 이제 어떤 상황이든(한쪽이 더미 카드여도) 항상 ProcessSingleRound를 실행하여 정상적인 전투 페이즈를 진행합니다.
            yield return StartCoroutine(ProcessSingleRound(p1CardData, p2CardData, p1CardView, p2CardView)); 
            
            p1CardView.DestroyCard();
            p2CardView.DestroyCard();
            yield return new WaitForSeconds(0.5f);
            
            if (CheckForGameOver()) yield break;
        }
        
        yield return StartCoroutine(ProcessGlobalPhase(GamePhase.TurnEnd));
        if (CheckForGameOver()) yield break;
        
        Debug.Log("\n--- 모든 페이즈 및 라운드 정상 종료 ---");
    }

    IEnumerator ProcessSingleRound(CardDataSO card1, CardDataSO card2, CardView view1, CardView view2)
    {
        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PreCombat, card1, card2));
        if (CheckForGameOver()) yield break;
        
        int finalP1Attack = card1.attackPower + CalculateCombatBonus(player1, player2, card1, card2);
        int finalP2Attack = card2.attackPower + CalculateCombatBonus(player2, player1, card2, card1);
        
        Debug.Log("--- 전투 단계 ---");
        Debug.Log($"{player1.playerName}: {card1.cardName}({finalP1Attack}) vs {player2.playerName}: {card2.cardName}({finalP2Attack})");

        if(finalP1Attack > finalP2Attack)
        {
            Debug.Log($"라운드 승자: {player1.playerName}");
            view1.PlayAttackAnimation(view2.transform.position, view1.transform.position);
            yield return new WaitForSeconds(0.5f);
            view2.PlayDamageEffect();
        }
        else if (finalP2Attack > finalP1Attack)
        {
            Debug.Log($"라운드 승자: {player2.playerName}");
            view2.PlayAttackAnimation(view1.transform.position, view2.transform.position);
            yield return new WaitForSeconds(0.5f);
            view1.PlayDamageEffect();
        }
        else
        {
            Debug.Log("라운드 무승부");
        }
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(ProcessRoundPhase(GamePhase.PostCombat, card1, card2));
    }
    
    // ★★★ 삭제된 부분 ★★★
    // ProcessDirectAttack 함수는 더 이상 필요 없으므로 제거되었습니다.

    IEnumerator ProcessRoundPhase(GamePhase phase, CardDataSO card1, CardDataSO card2)
    {
        var effectsToProcess = new List<EffectToSort>();
        CollectEffectsFromCard(effectsToProcess, phase, player1, player2, card1);
        CollectEffectsFromCard(effectsToProcess, phase, player2, player1, card2);
        SortEffects(effectsToProcess);
        foreach (var sortedEffect in effectsToProcess) { effectQueue.Enqueue(sortedEffect.execution); }
        yield return StartCoroutine(ProcessEffectQueue());
    }
    
    IEnumerator ProcessGlobalPhase(GamePhase phase)
    {
        var effectsToProcess = new List<EffectToSort>();
        foreach (var card in player1.registeredCards) if(card != null) CollectEffectsFromCard(effectsToProcess, phase, player1, player2, card);
        foreach (var card in player2.registeredCards) if(card != null) CollectEffectsFromCard(effectsToProcess, phase, player2, player1, card);
        SortEffects(effectsToProcess);
        foreach (var sortedEffect in effectsToProcess) { effectQueue.Enqueue(sortedEffect.execution); }
        yield return StartCoroutine(ProcessEffectQueue());
    }

    void CollectEffectsFromCard(List<EffectToSort> list, GamePhase phase, Player owner, Player opponent, CardDataSO currentCard)
    {
        foreach (var effectData in currentCard.effects)
        {
            CardEffect tempEffect = EffectFactory.CreateEffect(effectData.effectId, effectData.parameters);
            if (tempEffect != null && tempEffect.TriggerPhase == phase)
            {
                list.Add(new EffectToSort
                {
                    execution = new CardEffectExecution { effect = tempEffect, owner = owner, opponent = opponent },
                    speed = currentCard.speed,
                    attackPower = currentCard.attackPower,
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

    private int CalculateCombatBonus(Player owner, Player opponent, CardDataSO ownerCard, CardDataSO opponentCard)
    {
        int totalBonus = 0;
        foreach (var effectData in ownerCard.effects)
        {
            CardEffect tempEffect = EffectFactory.CreateEffect(effectData.effectId, effectData.parameters);
            if (tempEffect != null)
            {
                totalBonus += tempEffect.GetCombatBonus(owner, opponent, ownerCard, opponentCard);
            }
        }
        return totalBonus;
    }

    IEnumerator ProcessEffectQueue()
    {
        while (effectQueue.Count > 0)
        {
            CardEffectExecution exec = effectQueue.Dequeue();
            exec.effect.Execute(exec.owner, exec.opponent);
            if (CheckForGameOver()) yield break;
            yield return new WaitForSeconds(1f);
        }
    }

    bool CheckForGameOver()
    {
        if (player1.health <= 0 || player2.health <= 0)
        {
            Debug.Log("게임 종료!");
            Time.timeScale = 0;
            return true;
        }
        return false;
    }

    // ★★★ 삭제된 부분 ★★★
    // ParseEffectsFromString 함수는 CSV 방식에서만 필요하므로 제거되었습니다.
}