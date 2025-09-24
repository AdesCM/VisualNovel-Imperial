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
        UpdateVisual(null); // 초기에는 비어있음
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
    
    public void UpdateVisual(CardDataSO cardSO)
{
    if (cardSO != null) // 슬롯에 카드가 등록된 경우
    {
        // ★★★ 대체 이미지 로직 추가 ★★★
        if (cardSO.cardArt != null)
        {
            // 카드 아트가 있으면 -> 해당 아트 표시
            cardArtImage.sprite = cardSO.cardArt;
            cardArtImage.color = Color.white;
        }
        else
        {
            // 카드 아트는 없으면 -> 대체 이미지 표시
            cardArtImage.sprite = uiManager.defaultCardArt;
            cardArtImage.color = new Color(0.7f, 0.7f, 0.7f, 1f); // 약간 어둡게
        }
    }
    else // 슬롯이 비어있는 경우
    {
        cardArtImage.sprite = null;
        cardArtImage.color = Color.clear;
    }
}
}