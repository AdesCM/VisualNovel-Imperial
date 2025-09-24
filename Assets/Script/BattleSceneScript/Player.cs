using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RegisteredCardSlot
{
    public CardDataSO cardSO;
    public Character user;
    public SlotState state;
    public int registrationOrder; // 속도 정렬 시 동 순위 처리를 위한 등록 순서
}

public class Player : MonoBehaviour
{
    public string playerName;
    public List<Character> characters = new List<Character>();

    // ★★★ 덱, 핸드, 버린 카드 목록 추가 ★★★
    public List<CardDataSO> deck = new List<CardDataSO>();
    public List<CardDataSO> hand = new List<CardDataSO>();
    public List<CardDataSO> discardPile = new List<CardDataSO>();

    public List<RegisteredCardSlot> registeredSlots = new List<RegisteredCardSlot>();
    public int baseSlots = 4;
    public int bonusSlots = 0;

    public Dictionary<int, int> slotDiceCountBuffs = new Dictionary<int, int>();
    public Dictionary<int, int> slotDiceMaxBuffs = new Dictionary<int, int>();

    // ★★★ 덱 관리 로직 추가 ★★★
    public void ShuffleDeck()
    {
        var random = new System.Random();
        deck = deck.OrderBy(x => random.Next()).ToList();
    }

    public List<CardDataSO> DrawCards(int amount)
    {
        List<CardDataSO> drawnCards = new List<CardDataSO>();
        for (int i = 0; i < amount; i++)
        {
            if (deck.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    if (GameConstants.DEBUG_MODE) Debug.Log("덱과 버린 카드 더미가 모두 비었습니다!");
                    break; // 더 이상 뽑을 카드가 없음
                }
                // 덱이 비면 버린 카드 더미를 섞어서 다시 덱으로 만듦
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
                if (GameConstants.DEBUG_MODE) Debug.Log("덱을 재구성하고 섞습니다.");
            }
            
            CardDataSO cardToDraw = deck[0];
            deck.RemoveAt(0);
            hand.Add(cardToDraw);
            drawnCards.Add(cardToDraw);
        }
        return drawnCards;
    }
    
    public void ClearAllSlotBuffs()
    {
        slotDiceCountBuffs.Clear();
        slotDiceMaxBuffs.Clear();
    }

    // ★★★ 이 플레이어를 조종할 Agent ★★★
    [HideInInspector] public IPlayerAgent agent;
    public bool isTurnFinished = false;

    void Awake()
    {
        // 시작할 때 자신의 게임 오브젝트에 붙어있는 Agent 컴포넌트를 자동으로 찾음
        agent = GetComponent<IPlayerAgent>();
    }
}