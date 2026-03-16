using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float moveUpDistance = 1.5f; // Quãng đường chữ sẽ bay lên
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.OutQuad; // Hiệu ứng chuyển động (có thể thử Ease.OutBack để chữ nảy lên)

    [SerializeField, Range(-1f, 0f)] private float xMin = -0.5f;
    [SerializeField, Range(0f, 1f)] private float xMax = 0.5f;

    public void SetData(string value, Color color)
    {
        textMesh.text = value;
        textMesh.color = color;

        // Random nhẹ tọa độ X để các số máu không bị đè xếp lớp lên nhau nếu đánh liên tục
        float randomX = UnityEngine.Random.Range(xMin, xMax);
        transform.position += new Vector3(randomX, 0, 0);

        transform.localScale = startScale * Vector3.one;

        // Khởi tạo animation với DOTween cho Transform thường
        var moveTween = transform.DOMoveY(transform.position.y + moveUpDistance, duration);
        var fadeTween = textMesh.DOFade(0f, duration);
        var scaleTween = transform.DOScale(0.5f, duration); // Thu nhỏ lại một chút khi biến mất

        // Gom các hiệu ứng lại chạy cùng lúc
        var sequence = DOTween.Sequence();
        sequence
            .Append(moveTween)
            .Join(fadeTween)
            .Join(scaleTween)
            .SetEase(ease)
            .OnComplete(SelfDestroy) // Tự hủy object sau khi chạy xong animation để không nặng máy
            .Play();
    }

    private void SelfDestroy()
    {
        Destroy(this.gameObject);
    }
}
