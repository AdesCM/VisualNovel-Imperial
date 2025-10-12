using UnityEngine;
using System.Collections.Generic;

// 특정 턴에 수행할 행동 하나를 정의하는 클래스
[System.Serializable]
public class ScheduledAction
{
    public int turnNumber; // 이 행동을 실행할 턴 번호
    public CardDataSO cardToUse; // 사용할 카드의 SO
    public int targetSlotIndex;
    // 카드를 등록할 슬롯 인덱스 (1번 슬롯 -> 0)
    // public int casterCharacterIndex; // 여러 캐릭터 중 누가 시전할지 (0 = 첫번째 캐릭터)
}

[CreateAssetMenu(fileName = "New AI Pattern", menuName = "Card Game/AI Pattern")]

public class AIPatternSO : ScriptableObject
{
    public string patternId; // 이 패턴의 고유 ID
    public List<ScheduledAction> scheduledActions;
}