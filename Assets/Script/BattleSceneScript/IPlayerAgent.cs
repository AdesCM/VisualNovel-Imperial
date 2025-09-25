// 모든 플레이어 조종자(사람, AI 등)가 따라야 할 규칙
public interface IPlayerAgent
{
    // 이 Agent가 조종할 Player 데이터를 설정
    void Setup(Player player, GameManager gameManager, AIPatternSO pattern);

    // 턴 시작 시, 행동을 준비하라는 명령을 받음
    void PrepareTurn();
}