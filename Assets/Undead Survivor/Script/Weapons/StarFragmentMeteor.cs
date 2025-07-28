using UnityEngine;
using System.Collections;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Star Fragment 메테오 이펙트를 관리하는 클래스
/// 메테오 낙하 애니메이션과 충돌 처리를 담당합니다.
/// </summary>
public class StarFragmentMeteor : MonoBehaviour
{
    [Header("메테오 낙하 연출")]
    [Tooltip("메테오 낙하 시간")]
    public float meteorFallDuration = 0.5f;
    
    [Tooltip("메테오 낙하 시작 오프셋 (오른쪽 위에서 시작)")]
    public Vector2 meteorStartOffset = new Vector2(10f, 20f);

    [Header("Fade Out Settings")]
    [Tooltip("페이드 아웃이 완료되는 데 걸리는 시간")]
    public float fadeOutDuration = 0.2f;

    private float damage;
    private float area;
    private Vector3 targetPosition;
    private bool hasDealtDamage = false;
    private bool damageEnabled = false;
    private bool isFading = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 메테오 이펙트를 초기화하고 낙하 시퀀스를 시작합니다.
    /// </summary>
    public void Init(float dmg, float attackArea, Vector3 targetPos)
    {
        damage = dmg;
        area = attackArea;
        targetPosition = targetPos;
        hasDealtDamage = false;
        damageEnabled = false;
        isFading = false;

        // 스프라이트 렌더러 초기화
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

    }

    public void StartMeteorSequence()
    {
        StartCoroutine(MeteorSequence());
    }

    public void SetDamageEnabled(bool enabled)
    {
        damageEnabled = enabled;
    }

    /// <summary>
    /// 메테오의 시퀀스: 낙하 → 충돌 → 페이드아웃
    /// </summary>
    private IEnumerator MeteorSequence()
    {
        // 1. 메테오 낙하 애니메이션
        yield return StartCoroutine(MeteorFallAnimation());

        // 2. 충돌 처리
        OnMeteorImpact();

        // 3. 페이드 아웃
        StartFadeOut();
    }

    /// <summary>
    /// 메테오 낙하 애니메이션
    /// </summary>
    private IEnumerator MeteorFallAnimation()
    {
        // 시작 위치 계산 (타겟 위치에서 오프셋)
        Vector3 startPos = targetPosition + new Vector3(meteorStartOffset.x, meteorStartOffset.y, 0f);
        
        // 메테오 초기 설정
        transform.position = startPos;
        transform.localScale = Vector3.one * area;

        // DOTween을 사용한 낙하 애니메이션
        Sequence meteorSequence = DOTween.Sequence();
        
        // 위치 이동 + 회전 애니메이션
        meteorSequence.Append(transform.DOMove(targetPosition, meteorFallDuration)
            .SetEase(Ease.InQuad));

        yield return meteorSequence.WaitForCompletion();
    }

    /// <summary>
    /// 메테오 충돌 처리
    /// </summary>
    private void OnMeteorImpact()
    {
        // 피해 적용 활성화
        damageEnabled = true;
        
        // 충돌 피해 적용
        DealDamage();
    }

    private void DealDamage()
    {
        if (hasDealtDamage || !damageEnabled) return;

        // 충돌 범위 내 모든 적에게 피해
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, area);
        
        foreach (Collider2D target in targets)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.TakeDamage(damage);
            }
        }

        hasDealtDamage = true;
    }


    /// <summary>
    /// 페이드 아웃 애니메이션을 시작합니다.
    /// </summary>
    private void StartFadeOut()
    {
        isFading = true;
        if (spriteRenderer != null)
        {
            // DOTween을 사용하여 알파 값을 0으로 페이드 아웃
            spriteRenderer.DOFade(0f, fadeOutDuration)
                .SetDelay(0.3f) // 임팩트 효과 후 잠깐 대기
                .OnComplete(() => DeactivateEffect());
        }
        else
        {
            // SpriteRenderer가 없으면 즉시 비활성화
            DeactivateEffect();
        }
    }

    /// <summary>
    /// 이펙트를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void DeactivateEffect()
    {
        // Poolable 컴포넌트를 사용하여 풀에 반납합니다.
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 충돌 범위 시각화 (에디터에서만)
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, area);
    }

    private void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
} 