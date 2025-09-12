using UnityEngine;

[ExecuteAlways]
public class FitCardIllustration : MonoBehaviour
{
    public Transform cardFrame; // Quad (부모 프레임)
    public Vector2 padding = new Vector2(0.1f, 0.1f); 

    void Start()
    {
        FitToFrame();
    }

    void FitToFrame()
    {
        if (cardFrame == null) cardFrame = transform.parent;
        if (cardFrame == null) return;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Quad의 실제 크기
        Vector3 quadSize = cardFrame.localScale;

        // 스프라이트의 원본 크기 (bounds 대신 sprite.rect 사용 권장)
        Vector2 spriteSize = sr.sprite.bounds.size;

        // Quad 내부에 맞게 스케일 계산
        float scaleX = (quadSize.x - padding.x) / spriteSize.x;
        float scaleY = (quadSize.y - padding.y) / spriteSize.y;
        float finalScale = Mathf.Min(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1);

        // 중앙 정렬
        transform.localPosition = Vector3.zero;
    }
}