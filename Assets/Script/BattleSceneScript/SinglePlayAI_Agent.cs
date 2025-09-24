using UnityEngine;
using System.Collections.Generic;

public class SinglePlayAI_Agent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private GameManager gameManager;
    [SerializeField] private List<CardDataSO> skillPool; // Inspector에서 몬스터가 쓸 스킬 목록을 등록

    public void Setup(Player player, GameManager gm)
    {
        controlledPlayer = player;
        gameManager = gm;
    }

    public void PrepareTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}(싱글플레이 AI)가 행동을 결정 중입니다...");
        
        // 기존 카드 등록 내용 비우기 (매 턴 새로 등록)
        controlledPlayer.registeredSlots.Clear();

        // 예시: 기술 목록에서 무작위로 2개의 기술을 골라 등록
        for (int i = 0; i < 4; i++)
        {
            CardDataSO chosenSkill = skillPool[Random.Range(0, skillPool.Count)];
            controlledPlayer.registeredSlots.Add(new RegisteredCardSlot
            {
                cardSO = chosenSkill,
                user = controlledPlayer.characters[0]
            });
        }
        
        // ★★★ 삭제된 부분 ★★★
        // gameManager.OnAgentTurnFinished(controlledPlayer); // -> 이 줄을 삭제합니다.
    }

    
}