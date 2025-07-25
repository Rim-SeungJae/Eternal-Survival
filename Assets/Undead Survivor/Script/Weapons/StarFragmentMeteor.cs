using UnityEngine;
using System.Collections;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Star Fragment 메테오 이펙트를 관리하는 클래스
/// 충돌 시 피해를 주고 일정 시간 후 사라집니다.
/// </summary>
public class StarFragmentMeteor : MonoBehaviour
{
    private float damage;
    private float area;
    private float duration;
    private bool hasDealtDamage = false;
    private bool damageEnabled = true;

    public void Init(float dmg, float attackArea, float dur)
    {
        damage = dmg;
        area = attackArea;
        duration = dur;
        hasDealtDamage = false;
        damageEnabled = true;

        // 메테오 충돌 시 즉시 피해 적용
        DealDamage();

        // 충돌 효과 (스케일 애니메이션)
        PlayImpactEffect();

        // 일정 시간 후 오브젝트 비활성화
        StartCoroutine(DeactivateAfterDuration());
    }

    public void SetDamageEnabled(bool enabled)
    {
        damageEnabled = enabled;
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

    private void PlayImpactEffect()
    {
        // 충돌 시 펀치감 있는 스케일 효과
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * area, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                // 스케일 효과 완료 후 약간 줄어들면서 안정화
                transform.DOScale(Vector3.one * area * 0.9f, 0.1f)
                    .SetEase(Ease.InQuad);
            });
    }

    private IEnumerator DeactivateAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
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