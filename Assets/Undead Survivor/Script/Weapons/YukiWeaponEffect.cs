using UnityEngine;
using System.Collections;

/// <summary>
/// 유키 무기의 이펙트를 관리하는 클래스입니다.
/// Yuki Weapon 애니메이션을 재생하고 풀에 반환합니다.
/// </summary>
public class YukiWeaponEffect : MonoBehaviour
{
    private float damage;
    private float area;
    private float duration;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void Init(float dmg, float attackArea, float dur)
    {
        damage = dmg;
        area = attackArea;
        duration = dur;

        // 이펙트 시퀀스 시작
        StartCoroutine(EffectSequence());
    }

    private IEnumerator EffectSequence()
    {
        // 1. 애니메이션 재생
        if (animator != null)
        {
            animator.Play(0, 0, 0f);
        }

        // 2. 애니메이션 길이만큼 대기
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }
        else
        {
            // 애니메이터가 없으면 기본 지속시간 사용
            yield return new WaitForSeconds(duration);
        }

        // 3. 풀에 반환
        DeactivateEffect();
    }

    private void DeactivateEffect()
    {
        // 풀에 반환 필수
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
} 