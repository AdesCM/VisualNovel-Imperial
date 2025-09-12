using System.Collections.Generic;

public abstract class CardEffect
{
    // 효과가 행동을 발동할 단계를 지정
    public abstract GamePhase TriggerPhase { get; }

    // 데이터를 받아 초기화하는 메소드
    public abstract void Initialize(Dictionary<string, object> parameters);

    // 실제 행동 로직
    public abstract void Execute(Player owner, Player opponent);

    // 이번 전투의 공격력 보너스를 반환하는 조회(Query) 함수
    public virtual int GetCombatBonus(Player owner, Player opponent, CardDataSO ownerCard, CardDataSO opponentCard)
    {
        return 0;
    }
}