using UnityEngine;
using DG.Tweening;

/// <summary>
/// 부활 시 나타나는 시각적 이펙트의 애니메이션을 관리합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ReviveEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Animation Settings")]
    [Tooltip("전체 애니메이션 지속 시간")]
    public float duration = 1.5f;
    [Tooltip("Y축으로 상승할 거리")]
    public float riseHeight = 1.0f;
    [Tooltip("Y축으로 회전할 총 각도 (360의 배수)")]
    public float rotationDegrees = 1080f; // 3바퀴
    [Tooltip("페이드 아웃이 시작되기 전까지의 시간 비율 (0.0 ~ 1.0)")]
    [Range(0, 1)]
    public float fadeStartTimeRatio = 0.7f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // 이펙트가 재사용될 때마다 초기 상태를 설정하고 애니메이션을 시작합니다.
        StartEffect();
    }

    private void StartEffect()
    {
        // 초기 상태 설정 (투명도, 위치 등)
        spriteRenderer.color = new Color(1, 1, 1, 1); // 완전 불투명
        Vector3 startPosition = transform.position;

        // DOTween 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // 1. Y축 회전과 상승을 동시에 진행
        sequence.Join(transform.DOLocalRotate(new Vector3(0, rotationDegrees, 0), duration, RotateMode.FastBeyond360)
                               .SetEase(Ease.Linear)); // 등속 회전
        sequence.Join(transform.DOMoveY(startPosition.y + riseHeight, duration)
                               .SetEase(Ease.OutQuad)); // 위로 갈수록 느려지게 상승

        // 2. 전체 지속 시간의 특정 지점부터 페이드 아웃 시작
        sequence.Insert(duration * fadeStartTimeRatio, spriteRenderer.DOFade(0, duration * (1 - fadeStartTimeRatio)));

        // 3. 시퀀스가 모두 끝나면 오브젝트를 풀에 반환
        sequence.OnComplete(() =>
        {
            Poolable poolable = GetComponent<Poolable>();
            if (poolable != null)
            {
                GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        });
    }
}
