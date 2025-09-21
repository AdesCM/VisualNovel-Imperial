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

    // 슬롯별 버프 정보를 저장할 Dictionaries
    public Dictionary<int, int> slotDiceCountBuffs = new Dictionary<int, int>();
    
    // ★★★ 누락되었던 이 변수를 추가합니다 ★★★
    public Dictionary<int, int> slotDiceMaxBuffs = new Dictionary<int, int>();

    // 턴 종료 시 모든 슬롯 버프를 초기화
    public void ClearAllSlotBuffs()
    {
        slotDiceCountBuffs.Clear();
        
        // ★★★ 새로운 버프도 함께 초기화하도록 추가합니다 ★★★
        slotDiceMaxBuffs.Clear();
    }
}