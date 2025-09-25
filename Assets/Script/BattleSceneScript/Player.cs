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
    public List<CardDataSO> masterDeck = new List<CardDataSO>(); // 원본 덱 (절대 변하지 않음)
    public List<CardDataSO> deck = new List<CardDataSO>();         // 플레이용 덱 (카드를 뽑으면 줄어듦)
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
            if (deck.Count == 0) // 플레이용 덱이 비었는지 확인
            {
                if (masterDeck.Count == 0)
                {
                    if (GameConstants.DEBUG_MODE) Debug.Log("원본 덱이 비어있어 더 이상 뽑을 수 없습니다!");
                    break; 
                }
                
                // 원본 덱을 복사하여 플레이용 덱을 새로 채움
                deck.AddRange(masterDeck);
                ShuffleDeck();
                if (GameConstants.DEBUG_MODE) Debug.Log("덱이 소진되어 원본 덱으로 새로 채우고 섞습니다.");
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

    // 플레이어를 조종할 Agent
    [HideInInspector] public IPlayerAgent agent;
    //일단 주섳리
    //public bool isTurnFinished = false;

    void Awake()
    {
        // 시작할 때 자신의 게임 오브젝트에 붙어있는 Agent 컴포넌트를 자동으로 찾음
        agent = GetComponent<IPlayerAgent>();
    }
}