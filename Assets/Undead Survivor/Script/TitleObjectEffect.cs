using UnityEngine;
using DG.Tweening;

public class TitleObjectEffect : MonoBehaviour
{
    public float startZ = -1000f;
    public float targetZ = 0f;
    public float dropDuration = 0.4f;
    public Ease easing;

    public AudioClip stampSfx;
    public GameObject dustPrefab;

    private void Awake()
    // 타이틀 오브젝트의 초기 위치를 설정합니다.
    {
        Vector3 pos = transform.localPosition;
        pos.z = startZ;
        transform.localPosition = pos;
    }

    public Sequence PlayEffect()
    // 오브젝트가 떨어지는 효과를 재생합니다.
    {
        Vector3 start = transform.localPosition;
        start.z = startZ;
        transform.localPosition = start;

        Vector3 end = new Vector3(start.x, start.y, targetZ);
        Vector3 originalScale = transform.localScale;
        Vector3 squashedScale = new Vector3(originalScale.x * 1.1f, originalScale.y * 0.7f, originalScale.z);

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOLocalMoveZ(targetZ, dropDuration).SetEase(easing))
           .AppendCallback(() =>
           {
               if (dustPrefab)
                   Instantiate(dustPrefab, transform.position, Quaternion.identity);
               if (stampSfx)
                   AudioSource.PlayClipAtPoint(stampSfx, Camera.main.transform.position);
           });

        return seq;
    }
}