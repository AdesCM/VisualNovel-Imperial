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
        
        // ★★★ 덱 구성 로직 삭제 ★★★
        // 이제 덱 구성은 GameManager가 PlayerDataManager를 통해 처리합니다.

        // 플레이용 덱(deck)을 GameManager가 만들어준 원본 덱(masterDeck)으로 초기화
        player.deck.Clear();
        player.deck.AddRange(player.masterDeck);



        //player.deck.Clear(); 혹시 몰라 일단 주석처리
        //player.deck.AddRange(player.masterDeck);
        player.ShuffleDeck();
        
        // ★★★ 초기 드로우 (첫 턴: 8장, 이후: 4장) ★★★ 여기선 4장
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

    
    void CreateHandCardObject(RuntimeCard runtimeCard)
    {
        GameObject cardObj = Instantiate(handCardPrefab, handPanel);
        HandCardView cardView = cardObj.GetComponent<HandCardView>();
              
        cardView.Setup(runtimeCard, this);
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
        if (player1.registeredCards.Count >= (player1.baseSlots + player1.bonusSlots))
        {
            if (GameConstants.DEBUG_MODE) Debug.Log("더 이상 카드를 등록할 수 없습니다.");
            return;
        }

        RuntimeCard cardToRegister = cardView.runtimeCard;
        cardToRegister.registrationOrder = registrationCounter++;

        player1.hand.Remove(cardToRegister);
        player1.registeredCards.Add(cardToRegister);

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
        if (slotIndex < player1.registeredCards.Count)
        {
            RuntimeCard cardToUnregister = player1.registeredCards[slotIndex];
            player1.registeredCards.RemoveAt(slotIndex);
            
            player1.hand.Add(cardToUnregister);
            CreateHandCardObject(cardToUnregister);
            
            SortAndRefreshUI();
        }
    }

    void SortAndRefreshUI()
    {
        // 속도 > 등록 순서로 정렬
        player1.registeredCards = player1.registeredCards
                                  .OrderByDescending(card => card.CardSO.speed)
                                  .ThenBy(card => card.registrationOrder)
                                  .ToList();
        
        // 등록 슬롯 UI 갱신
        for (int i = 0; i < registeredSlotViews.Count; i++)
        {
            if (i < player1.registeredCards.Count)
            {
                RuntimeCard card = player1.registeredCards[i];
                registeredSlotViews[i].UpdateVisual(card.CardSO, gameManager.GetStateData(card));
            }
            else
            {
                registeredSlotViews[i].UpdateVisual(null, null);
            }
        }
    }

    public void UpdateOpponentStatus()
    {
    // 기존 상태 아이콘들 모두 제거
        foreach (Transform child in opponentStatusPanel) Destroy(child.gameObject);
    
    // 상대방의 등록 카드 상태에 따라 아이콘 생성
        foreach (var slot in player2.registeredCards)
        {
            GameObject backObj = Instantiate(opponentCardBackPrefab, opponentStatusPanel);
            Image backImage = backObj.GetComponent<Image>();
        
         // ★★★ 색상 변경 대신, 스프라이트(일러스트)를 직접 교체합니다 ★★★
            if (slot.State == SlotState.Awakened || slot.State == SlotState.Encroachment)
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
        // 1. Player에게 카드를 뽑아달라고 요청
        List<RuntimeCard> drawnCards = player1.DrawCards(amount);
        
        // 2. 뽑은 카드들을 Player의 핸드 리스트에 추가
        player1.hand.AddRange(drawnCards);

        // 3. 핸드 리스트에 추가된 카드들을 기반으로 UI 오브젝트 생성
        foreach (var runtimeCard in drawnCards)
        {
            CreateHandCardObject(runtimeCard);
        }
        
        if (GameConstants.DEBUG_MODE) Debug.Log($"{player1.playerName}가 {drawnCards.Count}장의 카드를 뽑습니다. 현재 핸드: {player1.hand.Count}장");
    }
    

    public void OnTurnEndButtonPressed()
    {
        gameManager.StartCombat();
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }
}