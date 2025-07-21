using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// '유독성 발자국' 무기에서 생성되는 독장판 이펙트의 로직을 관리합니다.
/// 일정 시간 후 사라지며, 범위 내 적에게 피해를 줍니다. 중첩 피해를 방지합니다.
/// </summary>
public class NoxiousAftermathEffect : MonoBehaviour
{

    public float damageCooldown = 0.5f; // 피해 간격
    private float damage;       // 독장판의 피해량
    private float duration;     // 독장판의 지속 시간
    private float timer;        // 지속 시간 타이머
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러 참조
    private Animator animator; // 애니메이터 참조
    private bool isFading = false; // 페이드 아웃 중인지 여부
    private float animationPlayTime; // 애니메이션이 재생되어야 할 목표 시간

    [Header("Animation & Fade Out Settings")]
    [Tooltip("애니메이션이 재생되어야 할 목표 시간")]
    public float targetAnimationDuration = 0.5f; // 애니메이션이 재생되어야 할 목표 시간
    [Tooltip("페이드 아웃이 완료되는 데 걸리는 시간")]
    public float fadeOutDuration = 0.2f; // 페이드 아웃 지속 시간

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 참조 할당
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        timer = 0f;
        isFading = false;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

        }

        if (animator != null)
        {
            animator.Play(0, 0, 0f);
            animationPlayTime = animator.GetCurrentAnimatorStateInfo(0).length;
        }
        else
        {
            animationPlayTime = 0f;
        }

        if (animationPlayTime > 0)
        {
            if (targetAnimationDuration > 0)
            {
                animator.speed = animationPlayTime / targetAnimationDuration;
                animationPlayTime = targetAnimationDuration;
            }
            else
            {
                animator.speed = 1f;
                animationPlayTime = 0f;
            }
        }
        else
        {
            animator.speed = 1f;
            if (duration > 0 && (duration - fadeOutDuration) <= 0)
            {
                StartFadeOut();
            }
        }
    }

    void Update()
    {
        if (!isFading)
        {
            timer += Time.deltaTime;
            if (timer >= duration - fadeOutDuration)
            {
                StartFadeOut();
            }
        }
    }

    /// <summary>
    /// 독장판을 초기화합니다.
    /// </summary>
    public void Init(float dmg, float dur)
    {
        damage = dmg;
        duration = dur;
    }

    /// <summary>
    /// 페이드 아웃 애니메이션을 시작합니다.
    /// </summary>
    private void StartFadeOut()
    {
        isFading = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0f, fadeOutDuration)
                .OnComplete(() => DeactivateEffect());
        }
        else
        {
            DeactivateEffect();
        }
    }

    /// <summary>
    /// 이펙트를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void DeactivateEffect()
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
    }

    // 이펙트 범위에 들어온 적에게 피해를 줍니다。
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, damageCooldown);
            }
        }
    }
}
