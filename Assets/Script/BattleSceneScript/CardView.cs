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

    public void Setup(CardDataSO data, CardStateData currentStateData)
    {
        cardData = data;
        this.name = currentStateData.stateName;
        
        if (cardArtRenderer != null) cardArtRenderer.sprite = data.cardArt;
        if (nameText != null) nameText.text = currentStateData.stateName;
        if (attackText != null) attackText.text = currentStateData.attackDice;
        if (speedText != null) speedText.text = data.speed.ToString();
    }

    public void MoveToPosition(Vector3 targetPosition, float duration) { /* ... */ }
    public void PlayAttackAnimation(Vector3 targetPosition, Vector3 originalPosition) { /* ... */ }
    public void PlayDamageEffect() { /* ... */ }
    public void DestroyCard() { /* ... */ }
}