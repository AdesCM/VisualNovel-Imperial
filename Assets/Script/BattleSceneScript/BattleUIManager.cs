using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class BattleUIManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("UI Prefabs & Parents")]
    [SerializeField] private GameObject handCardPrefab;
    [SerializeField] private GameObject opponentCardBackPrefab;
    [SerializeField] private GameObject registeredSlotPrefab;
    [SerializeField] private Transform handPanel;
    [SerializeField] private Transform registeredSlotsPanel;
    [SerializeField] private Transform opponentStatusPanel;

    [Header("UI Resources")]
    public Sprite defaultCardArt;

    public Sprite awakenedBackSprite; // 각성 및 잠식 뒷면
    public Sprite revelationBackSprite; // 자각 및 침식 뒷면

    private List<HandCardView> handCardObjects = new List<HandCardView>();
    private List<RegisteredSlotView> registeredSlotViews = new List<RegisteredSlotView>();
    private int registrationCounter = 0;

    void Start()
    {

    }

    public void InitializePlayerUI(Player player, int maxSlots)
    {
        InitializeRegisteredSlots(maxSlots);
        
        // ★★★ masterDeck 구성 ★★★
        if (player.masterDeck.Count == 0)
        {
            // 예시: c001 카드 10장, c002 카드 10장을 원본 덱으로 구성
            for (int i = 0; i < 10; i++) player.masterDeck.Add(gameManager.GetCardData("c001"));
            for (int i = 0; i < 10; i++) player.masterDeck.Add(gameManager.GetCardData("c002"));
        }

        // ★★★ 플레이용 덱을 원본 덱으로 초기화 ★★★
        player.deck.Clear();
        player.deck.AddRange(player.masterDeck);
        
        // 덱 섞고 초기 8장 드로우
        player.ShuffleDeck();
        DrawNewCards(4);
    }

    // 슬롯 UI를 동적으로 생성하고 초기화하는 함수 (GameManager가 호출)
    public void InitializeRegisteredSlots(int maxSlots)
    {
        foreach (Transform child in registeredSlotsPanel)
        {
            Destroy(child.gameObject);
        }
        registeredSlotViews.Clear();

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(registeredSlotPrefab, registeredSlotsPanel);
            RegisteredSlotView slotView = slotObj.GetComponent<RegisteredSlotView>();
            slotView.Setup(i, this);
            registeredSlotViews.Add(slotView);
        }
    }

    void InitialDraw()
    {
        List<CardDataSO> drawnCards = player1.DrawCards(8);
        foreach (var cardData in drawnCards)
        {
            CreateHandCardObject(cardData);
        }
    }
    
    void CreateHandCardObject(CardDataSO cardData)
    {
        GameObject cardObj = Instantiate(handCardPrefab, handPanel);
        HandCardView cardView = cardObj.GetComponent<HandCardView>();
        cardView.Setup(cardData, this);
        handCardObjects.Add(cardView);

        // 간단한 드로우 애니메이션
        cardObj.transform.localScale = Vector3.zero;
        cardObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        UpdateHandLayout();
    }

    void UpdateHandLayout()
    {
        // 핸드 카드 UI 정렬 로직 (GridLayoutGroup 사용을 권장)
    }

    // 클릭으로 카드 등록
    public void RegisterCardFromHand(HandCardView cardView)
    {
        if (player1.registeredSlots.Count >= (player1.baseSlots + player1.bonusSlots))
        {
            Debug.Log("더 이상 카드를 등록할 수 없습니다.");
            return;
        }

        player1.hand.Remove(cardView.cardSO);
        player1.registeredSlots.Add(new RegisteredCardSlot 
        { 
            cardSO = cardView.cardSO, 
            user = player1.characters[0], // 시전자 지정 (향후 캐릭터 선택 UI 필요)
            registrationOrder = registrationCounter++
        });

        handCardObjects.Remove(cardView);
        Destroy(cardView.gameObject);
        
        SortAndRefreshUI();
    }
    
    // 드래그 앤 드롭으로 카드 등록 (향후 확장)
    public void RegisterCardFromHand(HandCardView cardView, int slotIndex)
    {
        // 이 부분은 드래그로 순서 교체 등 복잡한 로직이 필요
        // 현재는 클릭과 동일하게, 가장 왼쪽 빈 자리에 등록하도록 처리
        RegisterCardFromHand(cardView);
    }
    
    public void UnregisterCard(int slotIndex)
    {
        if (slotIndex < player1.registeredSlots.Count)
        {
            RegisteredCardSlot slotToRemove = player1.registeredSlots[slotIndex];
            player1.registeredSlots.RemoveAt(slotIndex);
            
            player1.hand.Add(slotToRemove.cardSO);
            CreateHandCardObject(slotToRemove.cardSO);
            
            SortAndRefreshUI();
        }
    }

    void SortAndRefreshUI()
    {
        // 속도 > 등록 순서로 정렬
        player1.registeredSlots = player1.registeredSlots
                                  .OrderByDescending(s => s.cardSO.speed)
                                  .ThenBy(s => s.registrationOrder)
                                  .ToList();
        
        // 등록 슬롯 UI 갱신
        for (int i = 0; i < registeredSlotViews.Count; i++)
        {
            if (i < player1.registeredSlots.Count)
            {
                registeredSlotViews[i].UpdateVisual(player1.registeredSlots[i].cardSO);
            }
            else
            {
                registeredSlotViews[i].UpdateVisual(null);
            }
        }
    }

    public void UpdateOpponentStatus()
    {
    // 기존 상태 아이콘들 모두 제거
        foreach (Transform child in opponentStatusPanel) Destroy(child.gameObject);
    
    // 상대방의 등록 카드 상태에 따라 아이콘 생성
        foreach (var slot in player2.registeredSlots)
        {
            GameObject backObj = Instantiate(opponentCardBackPrefab, opponentStatusPanel);
            Image backImage = backObj.GetComponent<Image>();
        
         // ★★★ 색상 변경 대신, 스프라이트(일러스트)를 직접 교체합니다 ★★★
            if (slot.state == SlotState.Awakened || slot.state == SlotState.Encroachment)
            {
                backImage.sprite = awakenedBackSprite; // 하얀색 뒷면 일러스트로 변경
            }
            else
            {
                backImage.sprite = revelationBackSprite; // 검은색 뒷면 일러스트로 변경
            }
        }
    }

    public void DrawNewCards(int amount)
    {
        List<CardDataSO> drawnCards = player1.DrawCards(amount);
        foreach (var cardData in drawnCards)
        {
            CreateHandCardObject(cardData);
        }
        if (GameConstants.DEBUG_MODE) Debug.Log($"{player1.playerName}가 {amount}장의 카드를 뽑습니다.");
    }
    

    public void OnTurnEndButtonPressed()
    {
        gameManager.StartCombat();
    }

    
}