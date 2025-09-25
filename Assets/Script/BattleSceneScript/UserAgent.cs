using UnityEngine;

public class UserAgent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private BattleUIManager uiManager;

    // ★★★ AIPatternSO pattern 파라미터를 추가합니다 ★★★
    public void Setup(Player player, GameManager gameManager, AIPatternSO pattern)
    {
        controlledPlayer = player;
        uiManager = FindObjectOfType<BattleUIManager>();
        // UserAgent는 AI 패턴을 사용하지 않으므로, pattern 파라미터는 무시합니다.
    }

    public void PrepareTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}가 행동을 준비 중입니다... (사용자 입력 대기)");
    }
}