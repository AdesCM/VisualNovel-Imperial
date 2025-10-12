public class RuntimeCard
{
    public CardDataSO CardSO;      // 이 카드의 원본 설계도
    public Character Caster;       // 이 카드의 시전자
    public SlotState State;        // 이 카드의 현재 상태 (자각/잠식 등)
    public int registrationOrder;  // 속도 정렬 시 동 순위 처리를 위한 등록 순서
}