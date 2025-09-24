using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 처리를 위해 필수
using UnityEngine.UI;

public class HandCardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardDataSO cardSO { get; private set; }

    [SerializeField] private Image cardArtImage; // 카드 일러스트를 표시할 Image
    
    private BattleUIManager uiManager;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    // ★★★ 에러의 원인 2: 이 함수가 없었습니다. ★★★
    public void Setup(CardDataSO data, BattleUIManager manager)
{
    cardSO = data;
    uiManager = manager;
    // ... (CanvasGroup 관련 코드는 동일)

    if (cardArtImage != null)
    {
        // ★★★ 대체 이미지 로직 추가 ★★★
        if (data != null && data.cardArt != null)
        {
            // 데이터와 아트가 모두 있으면 -> 원래 일러스트 사용
            cardArtImage.sprite = data.cardArt;
        }
        else
        {
            // 데이터가 없거나, 데이터는 있지만 아트가 비어있으면 -> 대체 이미지 사용
            cardArtImage.sprite = uiManager.defaultCardArt;
        }
    }

}

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 시 카드 등록
        uiManager.RegisterCardFromHand(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(uiManager.transform); // 드래그 동안 최상위 캔버스 자식으로 이동
        canvasGroup.blocksRaycasts = false; // 드래그 중 다른 UI 감지를 위해
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭되지 않았다면 원래 위치로 돌려놓는 로직이 필요하지만,
        // 현재는 RegisteredSlotView에서 드롭을 처리하므로 간단하게 구현합니다.
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
    }
}