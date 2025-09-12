using UnityEngine;
using TMPro; // TextMeshPro 사용 시
using DG.Tweening; // DOTween 사용을 위해 추가

public class CardView : MonoBehaviour
{
    public CardDataSO cardData { get; private set; } // 이 카드의 원본 데이터

    [SerializeField] private SpriteRenderer cardArtRenderer;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro attackText;
    [SerializeField] private TextMeshPro speedText;

    // 데이터를 기반으로 카드의 외형을 설정
    public void Setup(CardDataSO data)
    {
        cardData = data;
        
        // SO의 데이터로 게임 오브젝트의 컴포넌트들을 채웁니다.
        this.name = data.cardName; // 게임 오브젝트 이름 변경
        if (cardArtRenderer != null) cardArtRenderer.sprite = data.cardArt;
        if (nameText != null) nameText.text = data.cardName;
        if (attackText != null) attackText.text = data.attackPower.ToString();
        if (speedText != null) speedText.text = data.speed.ToString();
    }

    // --- DOTween을 사용한 애니메이션 연출 메소드들 ---

    public void MoveToPosition(Vector3 targetPosition, float duration)
    {
        transform.DOMove(targetPosition, duration).SetEase(Ease.OutCubic);
    }

    public void PlayAttackAnimation(Vector3 targetPosition, Vector3 originalPosition)
    {
        // 공격 모션을 연출하는 DOTween 시퀀스
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(targetPosition, 0.3f).SetEase(Ease.OutSine));
        sequence.AppendInterval(0.1f); // 잠시 멈춤
        sequence.Append(transform.DOMove(originalPosition, 0.5f).SetEase(Ease.InSine));
    }
    
    public void PlayDamageEffect()
    {
        // 데미지를 입었을 때 흔들리는 효과
        transform.DOShakePosition(0.3f, new Vector3(0.2f, 0, 0), 20);
    }

    public void DestroyCard()
    {
        // 카드가 파괴될 때 작아지면서 사라지는 효과
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                 .OnComplete(() => Destroy(gameObject));
    }
}