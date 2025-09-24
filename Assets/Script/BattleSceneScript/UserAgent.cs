using UnityEngine;

// 사람 플레이어의 입력을 기다리는 행위 주체(Agent)
public class UserAgent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private BattleUIManager uiManager;

    public void Setup(Player player, GameManager gameManager)
    {
        controlledPlayer = player;
        // UI 매니저를 찾아서 연결합니다.
        // Inspector에서 직접 연결하는 것이 더 안정적일 수 있습니다.
        uiManager = FindObjectOfType<BattleUIManager>();
    }

    public void PrepareTurn()
    {
        // UserAgent는 UI를 통해 사용자가 '턴 종료' 버튼을 누르기를 기다립니다.
        // 따라서 이 함수는 특별한 행동을 하지 않습니다.
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}가 행동을 준비 중입니다... (사용자 입력 대기)");
    }
}