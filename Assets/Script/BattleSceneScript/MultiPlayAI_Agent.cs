using UnityEngine;
using System.Linq;

public class MultiPlayAI_Agent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private GameManager gameManager;

    public void Setup(Player player, GameManager gm)
    {
        controlledPlayer = player;
        gameManager = gm;
        // AI도 자신의 덱을 구성하고 셔플해야 함
        // ... (덱 구성 로직)
        controlledPlayer.ShuffleDeck();
    }

    public void PrepareTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}(멀티플레이 AI)가 행동을 결정 중입니다...");
        
        // --- AI 로직 시작 ---
        // 예시: 덱에서 4장을 뽑고, 그 중 속도가 가장 빠른 2장을 등록
        controlledPlayer.DrawCards(4);
        var sortedHand = controlledPlayer.hand.OrderByDescending(card => card.speed).ToList();
        
        for(int i = 0; i < 2 && i < sortedHand.Count; i++)
        {
            controlledPlayer.registeredSlots.Add(new RegisteredCardSlot 
            {
                cardSO = sortedHand[i],
                user = controlledPlayer.characters[0] // 임시로 첫번째 캐릭터 사용
            });
        }
        // --- AI 로직 끝 ---
        
        // 행동이 끝났음을 GameManager에 알림
        gameManager.OnAgentTurnFinished(controlledPlayer);
    }
}