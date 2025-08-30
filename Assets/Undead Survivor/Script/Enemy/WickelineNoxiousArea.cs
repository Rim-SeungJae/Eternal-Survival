using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 위클라인의 Noxious Aftermath에서 생성되는 독성 장판 이펙트입니다.
/// 플레이어의 NoxiousAftermathEffect와 유사하지만, 플레이어에게 데미지를 입히는 몬스터 버전입니다.
/// 일정 시간 후 사라지며, 범위 내 플레이어에게 지속 데미지를 줍니다.
/// </summary>
public class WickelineNoxiousArea : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("피해 간격 (초)")]
    public float damageCooldown = 0.5f; // 피해 간격
    
    [Header("Visual Settings")]
    [Tooltip("애니메이션이 재생되어야 할 목표 시간")]
    public float targetAnimationDuration = 0.5f;
    
    [Tooltip("페이드 아웃이 완료되는 데 걸리는 시간")]
    public float fadeOutDuration = 0.2f;
    
    // 태그 기반 플레이어 감지로 변경 (LayerMask 제거)
    
    // 장판 속성
    private float damage; // 독장판의 피해량
    private float duration; // 독장판의 지속 시간
    private float timer; // 지속 시간 타이머
    private BossBase ownerBoss; // 장판을 생성한 보스
    
    // 컴포넌트 참조
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D areaCollider;
    
    // 상태 관리
    private bool isFading = false;
    private float animationPlayTime;
    private float lastDamageTime = 0f;
    
    void Awake()
    {
        // 컴포넌트 자동 할당
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        areaCollider = GetComponent<Collider2D>();
        
        // Collider가 Trigger로 설정되어 있는지 확인
        if (areaCollider != null && !areaCollider.isTrigger)
        {
            Debug.LogWarning($"WickelineNoxiousArea: Collider on {gameObject.name} should be set as Trigger!");
        }
    }

    void OnEnable()
    {
        // 풀에서 재사용될 때마다 초기화
        timer = 0f;
        isFading = false;
        lastDamageTime = 0f;

        // 스프라이트 렌더러 초기화
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        // 애니메이터 초기화
        if (animator != null)
        {
            animator.Play(0, 0, 0f);
            animationPlayTime = animator.GetCurrentAnimatorStateInfo(0).length;
            
            // 애니메이션 속도 조정
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
        
        // 콜라이더 활성화
        if (areaCollider != null)
        {
            areaCollider.enabled = true;
        }
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;
        
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
    /// 독성 장판을 초기화합니다.
    /// </summary>
    /// <param name="dmg">데미지</param>
    /// <param name="dur">지속 시간</param>
    /// <param name="owner">장판을 생성한 보스</param>
    public void Init(float dmg, float dur, BossBase owner)
    {
        damage = dmg;
        duration = dur;
        ownerBoss = owner;
        
        Debug.Log($"WickelineNoxiousArea initialized: damage={damage}, duration={duration}");
    }

    /// <summary>
    /// 페이드 아웃 애니메이션을 시작합니다.
    /// </summary>
    private void StartFadeOut()
    {
        isFading = true;
        
        // 콜라이더 비활성화 (더 이상 데미지를 주지 않음)
        if (areaCollider != null)
        {
            areaCollider.enabled = false;
        }
        
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
        if (poolable != null && GameManager.instance?.pool != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 플레이어가 독성 장판 범위에 들어왔을 때 호출됩니다.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isFading || !GameManager.instance.isLive) return;
        
        if (IsPlayerCollider(other))
        {
            // 즉시 첫 데미지 적용
            DealDamageToPlayer(other);
            lastDamageTime = Time.time;
        }
    }

    /// <summary>
    /// 플레이어가 독성 장판 범위에 머물러 있을 때 호출됩니다.
    /// </summary>
    void OnTriggerStay2D(Collider2D other)
    {
        if (isFading || !GameManager.instance.isLive) return;
        
        if (IsPlayerCollider(other))
        {
            // 쿨다운 체크 후 데미지 적용
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                DealDamageToPlayer(other);
                lastDamageTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 플레이어가 독성 장판 범위에서 나갔을 때 호출됩니다.
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        // 현재 특별한 처리 없음 (필요시 추후 구현)
    }

    /// <summary>
    /// 콜라이더가 플레이어인지 확인합니다.
    /// </summary>
    private bool IsPlayerCollider(Collider2D collider)
    {
        return collider.CompareTag(GameTags.PLAYER);
    }

    /// <summary>
    /// 플레이어에게 데미지를 입힙니다.
    /// </summary>
    private void DealDamageToPlayer(Collider2D playerCollider)
    {
        Player player = GameManager.instance.player;
        if (player != null)
        {
            // 플레이어에게 데미지 적용 (Player.TakeDamage 사용)
            player.TakeDamage(damage);
        
        }
        else
        {
            // GameManager를 통한 직접 체력 감소 (Player 컴포넌트가 없는 경우)
            if (GameManager.instance != null)
            {
                GameManager.instance.health -= damage;
            }
        }
    }

    /// <summary>
    /// 장판의 데미지를 런타임에 변경합니다.
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// 장판의 지속 시간을 런타임에 변경합니다.
    /// </summary>
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    /// <summary>
    /// 장판이 아직 활성 상태인지 반환합니다.
    /// </summary>
    public bool IsActive()
    {
        return !isFading && gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 장판을 강제로 제거합니다.
    /// </summary>
    public void ForceDestroy()
    {
        if (!isFading)
        {
            StartFadeOut();
        }
    }
    
    /// <summary>
    /// 에디터에서 장판 범위를 시각화합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 장판 범위 표시
        if (areaCollider != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.3f);
            
            if (areaCollider is CircleCollider2D circleCollider)
            {
                Gizmos.DrawSphere(transform.position, circleCollider.radius * transform.localScale.x);
            }
            else if (areaCollider is BoxCollider2D boxCollider)
            {
                Vector3 size = boxCollider.size;
                size.x *= transform.localScale.x;
                size.y *= transform.localScale.y;
                Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, size);
            }
        }
        
        // 데미지 정보 표시
        if (Application.isPlaying && IsActive())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up, Vector3.one * 0.3f);
        }
    }
}