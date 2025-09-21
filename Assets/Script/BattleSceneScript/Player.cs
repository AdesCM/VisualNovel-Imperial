using UnityEngine;
using System.Collections.Generic;

public class RegisteredCardSlot
{
    public CardDataSO cardSO;
    public Character user;
    public SlotState state;
}

public class Player : MonoBehaviour
{
    public string playerName;
    public List<Character> characters = new List<Character>();
    public List<RegisteredCardSlot> registeredSlots = new List<RegisteredCardSlot>();
    public int baseSlots = 4;
    public int bonusSlots = 0;

    // ★★★ 슬롯별 버프 정보를 저장할 Dictionary 추가 ★★★
    // Key: 슬롯 인덱스(0~), Value: 보너스 주사위 굴림 횟수
    public Dictionary<int, int> slotDiceCountBuffs = new Dictionary<int, int>();

    // ★★★ 턴 종료 시 모든 슬롯 버프를 초기화하는 메소드 추가 ★★★
    public void ClearAllSlotBuffs()
    {
        slotDiceCountBuffs.Clear();
    }
}