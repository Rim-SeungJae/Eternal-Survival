using UnityEngine;
using System.Collections.Generic; // HashSet을 사용하기 위해 추가
using DG.Tweening; // DOTween 네임스페이스 추가

/// <summary>
/// Quake 이펙트 자체의 동작(피해 적용, 자동 비활성화 등)을 관리합니다.
/// </summary>
public class QuakeEffect : MonoBehaviour
{
    private float damage;
    private float duration; // 총 지속 시간 (애니메이션 + 페이드 아웃)
    private float timer; // 현재 경과 시간
    private QuakeWeapon parentWeapon; // QuakeWeapon 참조
    private Transform playerTransform; // 플레이어 Transform 참조
    private HashSet<Enemy> hitEnemies; // 이미 피해를 준 적들을 추적하기 위한 HashSet
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러 참조
    private Animator animator; // 애니메이터 참조
    private bool isFading = false; // 페이드 아웃 중인지 여부
    private float animationPlayTime; // 애니메이션이 재생되어야 할 목표 시간 (duration - fadeOutDuration)

    [Header("Animation & Fade Out Settings")]
    [Tooltip("페이드 아웃이 완료되는 데 걸리는 시간")]
    public float fadeOutDuration = 0.2f; // 페이드 아웃 지속 시간

    void Awake()
    {
        hitEnemies = new HashSet<Enemy>(); // HashSet 초기화
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱
        animator = GetComponent<Animator>(); // Animator 캐싱
    }

    void OnEnable()
    {
        timer = 0f;
        playerTransform = GameManager.instance.player.transform;
        hitEnemies.Clear(); // 오브젝트 풀에서 재사용될 때마다 이전에 맞춘 적 목록을 초기화합니다.
        isFading = false; // 활성화 시 페이드 아웃 상태 초기화

        // 스프라이트가 다시 보이도록 알파 값을 1로 설정
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        // 애니메이션 재생 및 길이 가져오기
        if (animator != null)
        {
            // Animator Controller의 기본 State를 재생합니다.
            animator.Play(0, 0, 0f); 
            // 현재 재생 중인 State의 길이를 가져옵니다.
            animationPlayTime = animator.GetCurrentAnimatorStateInfo(0).length;
        }
        else
        {
            animationPlayTime = 0f; // 애니메이터가 없으면 0으로 설정
        }

        // 애니메이션이 재생되어야 할 목표 시간 계산 (총 지속 시간 - 페이드 아웃 시간)
        // 만약 animationPlayTime이 0보다 크다면, duration에서 fadeOutDuration을 뺀 값으로 애니메이션 재생 시간을 조절합니다.
        // 이렇게 하면 animationPlayTime이 애니메이션 클립의 실제 길이가 아닌, 목표 재생 시간이 됩니다.
        if (animationPlayTime > 0)
        {
            float targetAnimationDuration = duration - fadeOutDuration;
            if (targetAnimationDuration > 0)
            {
                animator.speed = animationPlayTime / targetAnimationDuration;
                animationPlayTime = targetAnimationDuration; // 실제 애니메이션 재생 시간은 조절된 시간
            }
            else
            {
                animator.speed = 1f; // 유효하지 않은 경우 기본 속도
                animationPlayTime = 0f; // 즉시 페이드 아웃 시작
            }
        }
        else
        {
            animator.speed = 1f; // 애니메이션이 없으면 기본 속도
            // 애니메이션 재생 시간이 없거나 음수이면 즉시 페이드 아웃 시작
            if (duration > 0 && (duration - fadeOutDuration) <= 0)
            {
                StartFadeOut();
            }
        }
    }

    void Update()
    {
        // 플레이어를 따라다니도록 위치 업데이트
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }

        // 페이드 아웃 중이 아닐 때만 타이머 증가
        if (!isFading)
        {
            timer += Time.deltaTime;

            // 애니메이션 재생 목표 시간에 도달하면 페이드 아웃 시작
            if (timer >= animationPlayTime)
            {
                StartFadeOut();
            }
        }
    }

    /// <summary>
    /// 이펙트의 데미지와 지속시간, 그리고 부모 QuakeWeapon을 설정합니다.
    /// </summary>
    public void Init(float dmg, float dur, QuakeWeapon weapon)
    {
        damage = dmg;
        duration = dur; // 이펙트의 총 지속 시간 (애니메이션 + 페이드 아웃)
        parentWeapon = weapon;
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
                .OnComplete(() => DeactivateEffect()); // 페이드 아웃 완료 후 비활성화
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

        // Quake 이펙트가 종료되었음을 QuakeWeapon에 알립니다.
        if (parentWeapon != null)
        {
            parentWeapon.OnQuakeEffectFinished();
        }
    }

    // 이펙트 범위에 들어온 적에게 피해를 줍니다。
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameTags.ENEMY))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !hitEnemies.Contains(enemy)) // 이미 맞춘 적이 아니라면
            {
                enemy.TakeDamage(damage);
                hitEnemies.Add(enemy); // 맞춘 적 목록에 추가
            }
        }
    }
}
