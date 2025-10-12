using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 처리를 위해 필수
using UnityEngine.UI;
using TMPro;

public class HandCardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RuntimeCard runtimeCard { get; private set; }
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Image cardArtImage; // 카드 일러스트를 표시할 Image
    
    private BattleUIManager uiManager;
    //private Transform originalParent;
    private CanvasGroup canvasGroup;

    public void Setup(RuntimeCard card, BattleUIManager manager)
    {
        this.uiManager = manager;
        this.runtimeCard = card; 
        if (GetComponent<CanvasGroup>() == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        else
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (runtimeCard == null) return;
        
        // GameManager의 헬퍼 함수를 통해 현재 상태 데이터를 가져옴
        CardStateData currentStateData = uiManager.GetGameManager().GetStateData(runtimeCard);
        
        if (cardArtImage != null)
        {
            cardArtImage.sprite = (currentStateData?.cardArt != null) ? currentStateData.cardArt : uiManager.defaultCardArt;
        }
        if (nameText != null)
        {
            nameText.text = currentStateData?.stateName ?? "???";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        uiManager.RegisterCardFromHand(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //originalParent = transform.parent;
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
        //transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
    }
}