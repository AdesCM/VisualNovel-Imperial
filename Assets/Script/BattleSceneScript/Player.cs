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
}