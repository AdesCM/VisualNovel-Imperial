using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RegisteredSlotView : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public int slotIndex { get; private set; }
    private BattleUIManager uiManager;
    [SerializeField] private Image cardArtImage; // 중앙에 표시될 카드 일러스트 이미지

    public void Setup(int index, BattleUIManager manager)
    {
        slotIndex = index;
        uiManager = manager;
        UpdateVisual(null, null); // 초기에는 비어있음
    }

    public void OnDrop(PointerEventData eventData)
    {
        HandCardView draggedCard = eventData.pointerDrag.GetComponent<HandCardView>();
        if (draggedCard != null)
        {
            uiManager.RegisterCardFromHand(draggedCard, slotIndex);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 시 카드 등록 해제
        uiManager.UnregisterCard(slotIndex);
    }
    
    public void UpdateVisual(CardDataSO cardSO, CardStateData currentStateData)
    {
        if (cardSO != null) // 슬롯에 카드가 등록된 경우
        {
        // ★★★ 대체 이미지 로직 추가 ★★★
            if (cardSO != null && currentStateData != null)
            {
                // ★★★ currentStateData에서 cardArt를 가져옴 ★★★
                cardArtImage.sprite = (currentStateData.cardArt != null) ? currentStateData.cardArt : uiManager.defaultCardArt;
                cardArtImage.color = Color.white;
            }
            else // 슬롯이 비어있는 경우
            {
                cardArtImage.sprite = null;
                cardArtImage.color = Color.clear;
            }
        }
        else // 슬롯이 비어있는 경우
        {
            cardArtImage.sprite = null;
            cardArtImage.color = Color.clear;
        }
    }
}