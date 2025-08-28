using System.Collections;
using UnityEngine;

/// <summary>
/// 위클라인의 트리플렛 코드 공격에서 사용되는 작살 형태의 투사체입니다.
/// PoolManager와 호환되며, 플레이어와 충돌 시 데미지를 입힙니다.
/// </summary>
public class TripletCodeProjectile : MonoBehaviour
{
    [Header("Projectile Components")]
    [SerializeField] private Rigidbody2D projectileRigidbody; // 투사체 리지드바디
    [SerializeField] private Collider2D projectileCollider; // 투사체 콜라이더
    [SerializeField] private SpriteRenderer projectileSprite; // 투사체 스프라이트
    
    [Header("Trail Effect")]
    [SerializeField] private TrailRenderer trailRenderer; // 투사체 궤적 이펙트
    
    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab; // 충돌 이펙트 프리팹
    [PoolTagSelector] public string hitEffectPoolTag = "ProjectileHitEffect"; // 충돌 이펙트 풀 태그
    
    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer = 1 << 6; // 플레이어 레이어
    
    // 투사체 상태
    private bool isActive = false;
    private float currentDamage;
    private float remainingLifetime;
    private BossBase ownerBoss;
    
    // 움직임 관련
    private Vector2 moveDirection;
    private float moveSpeed;
    
    void Awake()
    {
        // 컴포넌트 자동 할당
        if (projectileRigidbody == null)
            projectileRigidbody = GetComponent<Rigidbody2D>();
        
        if (projectileCollider == null)
            projectileCollider = GetComponent<Collider2D>();
            
        if (projectileSprite == null)
            projectileSprite = GetComponent<SpriteRenderer>();
            
        if (trailRenderer == null)
            trailRenderer = GetComponent<TrailRenderer>();
        
        // 리지드바디 설정
        if (projectileRigidbody != null)
        {
            projectileRigidbody.gravityScale = 0f; // 중력 무시
            projectileRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 연속 충돌 감지
        }
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        // 풀에서 재사용될 때마다 초기화
        isActive = false;
        
        // 트레일 렌더러 완전 초기화
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false; // 먼저 비활성화
            trailRenderer.Clear(); // 기존 트레일 제거
            // 다음 프레임에 다시 활성화 (Clear가 완전히 적용되도록)
            StartCoroutine(EnableTrailNextFrame());
        }
    }
    
    /// <summary>
    /// 다음 프레임에 트레일 렌더러를 활성화합니다.
    /// </summary>
    private System.Collections.IEnumerator EnableTrailNextFrame()
    {
        yield return null; // 한 프레임 대기
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
    }
    
    void OnDisable()
    {
        // 비활성화 시 상태 초기화
        isActive = false;
        
        if (projectileRigidbody != null)
        {
            projectileRigidbody.linearVelocity = Vector2.zero;
        }
        
        // 트레일 렌더러 완전 정리
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            trailRenderer.Clear();
        }
    }
    
    void Update()
    {
        if (!isActive || !GameManager.instance.isLive) return;
        
        // 생존 시간 체크
        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f)
        {
            DestroyProjectile(false); // 시간 초과로 파괴 (충돌 이펙트 없음)
        }
    }
    
    /// <summary>
    /// 투사체를 발사합니다.
    /// </summary>
    /// <param name="direction">발사 방향</param>
    /// <param name="speed">발사 속도</param>
    /// <param name="lifetime">생존 시간</param>
    /// <param name="damage">데미지</param>
    /// <param name="owner">발사한 보스</param>
    public void Launch(Vector2 direction, float speed, float lifetime, float damage, BossBase owner)
    {
        // 투사체 설정
        moveDirection = direction.normalized;
        moveSpeed = speed;
        currentDamage = damage;
        remainingLifetime = lifetime;
        ownerBoss = owner;
        isActive = true;
        
        // 투사체 방향 설정 (스프라이트 회전) - Weapon.cs의 Fire() 메서드와 동일한 방식
        if (moveDirection != Vector2.zero)
        {
            // Unity의 기본 스프라이트가 위쪽(Vector3.up)을 향하고 있다고 가정하고 회전
            transform.rotation = Quaternion.FromToRotation(Vector3.up, moveDirection);
        }
        
        // 물리적 움직임 시작
        if (projectileRigidbody != null)
        {
            projectileRigidbody.linearVelocity = moveDirection * moveSpeed;
        }
        
        // 콜라이더 활성화
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }
        
        Debug.Log($"Triplet Code projectile launched: direction={moveDirection}, speed={speed}, damage={damage}");
    }
    
    /// <summary>
    /// 충돌 감지 (Trigger 방식)
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        // 플레이어와 충돌 체크
        if (IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // 플레이어에게 데미지 적용
                player.TakeDamage(currentDamage);
                
                Debug.Log($"Triplet Code projectile hit player for {currentDamage} damage");
                
                // 충돌 이펙트와 함께 투사체 파괴
                DestroyProjectile(true);
                return;
            }
        }
    }
    
    /// <summary>
    /// 물리적 충돌 감지 (Collision 방식) - 백업용
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;
        
        GameObject other = collision.gameObject;
        
        // 플레이어와 충돌 체크
        if (IsInLayerMask(other.layer, playerLayer))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(currentDamage);
                Debug.Log($"Triplet Code projectile hit player (collision) for {currentDamage} damage");
                DestroyProjectile(true);
                return;
            }
        }
    }
    
    /// <summary>
    /// 레이어 마스크 체크 유틸리티
    /// </summary>
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
    
    /// <summary>
    /// 투사체를 파괴하고 풀로 반환합니다.
    /// </summary>
    /// <param name="showHitEffect">충돌 이펙트 표시 여부</param>
    private void DestroyProjectile(bool showHitEffect)
    {
        if (!isActive) return;
        
        isActive = false;
        
        // 충돌 이펙트 표시
        if (showHitEffect)
        {
            SpawnHitEffect();
        }
        
        // 물리적 움직임 중지
        if (projectileRigidbody != null)
        {
            projectileRigidbody.linearVelocity = Vector2.zero;
        }
        
        // 콜라이더 비활성화
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }
        
        // 풀로 반환
        ReturnToPool();
    }
    
    /// <summary>
    /// 충돌 이펙트를 생성합니다.
    /// </summary>
    private void SpawnHitEffect()
    {
        if (GameManager.instance?.pool == null) return;
        
        GameObject hitEffect = GameManager.instance.pool.Get(hitEffectPoolTag);
        if (hitEffect != null)
        {
            hitEffect.transform.position = transform.position;
            hitEffect.transform.rotation = transform.rotation;
            hitEffect.SetActive(true);
            
            // 파티클 시스템이 있다면 재생
            ParticleSystem particles = hitEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
            }
        }
        
        // 충돌 효과음
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
    }
    
    /// <summary>
    /// 투사체를 풀로 반환합니다.
    /// </summary>
    private void ReturnToPool()
    {
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null && GameManager.instance?.pool != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            // Poolable이 없다면 비활성화
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 투사체의 현재 상태를 반환합니다.
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// 투사체를 강제로 정지시킵니다. (공격 중단 시 사용)
    /// </summary>
    public void ForceStop()
    {
        DestroyProjectile(false);
    }
    
    /// <summary>
    /// 투사체의 데미지를 변경합니다.
    /// </summary>
    public void SetDamage(float newDamage)
    {
        currentDamage = newDamage;
    }
    
    /// <summary>
    /// 투사체의 속도를 변경합니다.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        if (projectileRigidbody != null && isActive)
        {
            projectileRigidbody.linearVelocity = moveDirection * moveSpeed;
        }
    }
}