using UnityEngine;
using TMPro;
using DG.Tweening;

public class CardView : MonoBehaviour
{
    public CardDataSO cardData { get; private set; }

    [SerializeField] private SpriteRenderer cardArtRenderer;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro attackText;
    [SerializeField] private TextMeshPro speedText;
    public Sprite defaultCardArt;

    public void Setup(CardDataSO data, CardStateData currentStateData)
    {
        cardData = data;
        if(currentStateData != null)
        {
            this.name = currentStateData.stateName;
            if (nameText != null) nameText.text = currentStateData.stateName;
            if (attackText != null) attackText.text = currentStateData.attackDice;

            if (cardArtRenderer != null) cardArtRenderer.sprite = currentStateData.cardArt;
        }
   
        if (data != null && speedText != null) speedText.text = data.speed.ToString();
    }

    public void PlayAttackAnimation(Vector3 targetPosition, Vector3 originalPosition) { /* DOTween Sequence */ }
    public void PlayDamageEffect() { transform.DOShakePosition(0.3f, new Vector3(0.2f, 0, 0), 20); }
    public void DestroyCard() { transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject)); }
}