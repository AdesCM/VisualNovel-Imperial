using UnityEngine;
using System.Linq;

public class MultiPlayAI_Agent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private GameManager gameManager;
    private AIPatternSO currentPattern; 
    public void Setup(Player player, GameManager gm, AIPatternSO pattern)
    {
        controlledPlayer = player;
        gameManager = gm;
        currentPattern = pattern;
        // AI도 자신의 덱을 구성하고 셔플해야 함
        // ... (덱 구성 로직)
        controlledPlayer.ShuffleDeck();
    }

    public void PrepareTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}(멀티플레이 AI)가 행동을 결정 중입니다...");
        
        // ★★★ 변수 이름 수정: registeredSlots -> registeredCards ★★★
        controlledPlayer.registeredCards.Clear();

        // 예시: 덱에서 4장을 뽑고, 그 중 속도가 가장 빠른 2장을 등록
        controlledPlayer.DrawCards(4);
        // ★★★ 접근 방식 수정: card.speed -> card.CardSO.speed ★★★
        var sortedHand = controlledPlayer.hand.OrderByDescending(card => card.CardSO.speed).ToList();
        
        for(int i = 0; i < 2 && i < sortedHand.Count; i++)
        {
            // ★★★ 타입 변경: RegisteredCardSlot -> RuntimeCard ★★★
            controlledPlayer.registeredCards.Add(sortedHand[i]);
        }
        // --- AI 로직 끝 ---
        
        // 행동이 끝났음을 GameManager에 알림
        //gameManager.OnAgentTurnFinished(controlledPlayer);
    }
}