using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 행동, 상태, 피격 처리를 관리하는 클래스입니다.
/// 플레이어를 추적하고, 피해를 받으면 넉백 및 사망 처리를 수행합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("이동 속도")]
    public float speed;
    [Tooltip("현재 체력")]
    public float health;
    [Tooltip("최대 체력")]
    public float maxHealth;
    [Tooltip("플레이어에게 입히는 접촉 데미지")]
    public float contactDamage;
    [Tooltip("드롭 가능한 아이템 목록과 각 아이템의 드롭 확률")]
    public LootItem[] lootTable; // DestructibleData와 동일한 구조체를 사용합니다.

    [Header("References")]
    [Tooltip("적 종류별 애니메이터 컨트롤러 배열")]
    public RuntimeAnimatorController[] animCon;
    [Tooltip("추적할 대상 (플레이어)")]
    public Rigidbody2D target;
    [Tooltip("그림자 오브젝트 Transform")]
    public Transform shadow;

    // 내부 상태 변수
    private bool isLive; // 생존 여부
    private bool isKnockBack; // 넉백 상태 여부

    // 컴포넌트 캐싱
    private Rigidbody2D rigid;
    private Collider2D coll;
    private Animator anim;
    private SpriteRenderer spriter;
    private CapsuleCollider2D capsule;
    private WaitForFixedUpdate wait; // 물리 업데이트 프레임 대기용

    [Header("Knockback Settings")]
    [Tooltip("넉백 시 받는 힘의 크기")]
    public float knockbackForce = 3f;
    [Tooltip("넉백 지속 시간")]
    public float knockbackDuration = 0.1f;

    // 상태 효과(디버프) 관련 변수들
    private float lastNoxiousAftermathDamageTime = -1f; // 마지막으로 '유독성 후유증' 피해를 입은 시간
    
    // 성능 최적화를 위한 변수들
    private float visibilityCheckTimer = 0f;
    private const float VISIBILITY_CHECK_INTERVAL = 0.1f;
    private bool isCurrentlyVisible = true;

    void Awake()
    {
        // 컴포넌트 초기화 및 캐싱
        capsule = GetComponent<CapsuleCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        shadow = transform.Find("Shadow"); // 자식 오브젝트에서 그림자 탐색
        wait = new WaitForFixedUpdate();

        // DayNightController는 GameManager에서 관리하므로, GameManager를 통해 참조를 가져옵니다.
        // 이렇게 하면 FindObjectOfType 호출을 줄일 수 있습니다.
        // 기존 if (dayNightController == null) 블록을 제거하고 직접 할당합니다.
        // dayNightController = FindObjectOfType<DayNightController>(); // 이 줄을 제거합니다.
    }

    void FixedUpdate()
    {
        // 시간 정지 상태일 때는 물리 로직을 실행하지 않습니다.
        if (GameManager.instance.isTimeStopped) 
        {
            rigid.linearVelocity = Vector2.zero; // 혹시 모를 관성을 제거
            return;
        }

        // 게임이 진행 중이 아니거나, 적이 살아있지 않거나, 피격/넉백 중일 때는 이동 로직을 실행하지 않습니다.
        if (!GameManager.instance.isLive || !isLive || isKnockBack || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {
            return;
        }

        // 플레이어 방향으로 이동 벡터 계산
        Vector2 dirVec = target.position - rigid.position;
        // 정규화된 방향 벡터와 속도를 이용하여 다음 위치 계산
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;

        // 물리적으로 이동
        rigid.MovePosition(rigid.position + nextVec);
        // 이동 후 속도를 0으로 설정하여 관성으로 인한 미끄러짐 방지
        rigid.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        // 시간 정지 상태일 때 애니메이션 속도를 0으로, 아닐 때 1로 설정합니다.
        if (anim != null) anim.speed = GameManager.instance.isTimeStopped && health>0 ? 0 : 1;

        // 성능 최적화: 거리 계산을 일정 간격마다 수행
        visibilityCheckTimer += Time.deltaTime;
        
        if (visibilityCheckTimer >= VISIBILITY_CHECK_INTERVAL)
        {
            visibilityCheckTimer = 0f;
            
            // 밤에는 플레이어의 빛 범위 안에 있을 때만 보이도록 처리
            if (GameManager.instance.IsNight())
            {
                Vector3 playerPos = GameManager.instance.player.transform.position;
                // sqrMagnitude 사용으로 제곱근 연산 제거
                float sqrDist = (playerPos - transform.position).sqrMagnitude;
                float lightRadiusHalf = GameManager.instance.dayNightController.CurrentLightRadius / 2.0f;
                bool isVisible = sqrDist < lightRadiusHalf * lightRadiusHalf;
                
                if (isCurrentlyVisible != isVisible)
                {
                    isCurrentlyVisible = isVisible;
                    spriter.enabled = isVisible;
                    if (shadow != null)
                        shadow.gameObject.SetActive(isVisible);
                }
            }
            else // 낮에는 항상 보이도록 처리
            {
                if (!isCurrentlyVisible)
                {
                    isCurrentlyVisible = true;
                    spriter.enabled = true;
                    if (shadow != null)
                        shadow.gameObject.SetActive(true);
                }
            }
        }
    }

    void LateUpdate()
    {
        // 모든 업데이트가 끝난 후, 플레이어 위치에 따라 스프라이트 방향을 결정합니다.
        if (!GameManager.instance.isLive || !isLive) return;
        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        // 오브젝트 풀에서 재사용될 때마다 상태를 초기화합니다.
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        isKnockBack = false;
        coll.enabled = true;
        rigid.simulated = true; // 물리 시뮬레이션 활성화
        spriter.sortingOrder = 0; // 기본 렌더링 순서
        anim.SetBool("Dead", false);
        health = maxHealth;
        
        // 가시성 상태 초기화
        visibilityCheckTimer = 0f;
        isCurrentlyVisible = true;
        
        // GameManager에 자신을 등록합니다.
        GameManager.instance.RegisterEnemy(this);
    }

    /// <summary>
    /// Spawner가 전달하는 데이터로 적의 능력치를 초기화합니다.
    /// </summary>
    /// <param name="data">적 생성 데이터</param>
    public void Init(SpawnDataSO data)
    {
        if (shadow != null)
        {
            shadow.localPosition = data.shadowOffset;
            shadow.localScale = data.shadowSize;
        }
        capsule.size = data.colliderSize;
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
        contactDamage = data.contactDamage;
        lootTable = data.lootTable;
    }

    /// <summary>
    /// 외부로부터 피해를 받아 체력을 감소시키고 관련 효과를 처리합니다.
    /// </summary>
    /// <param name="damage">받은 피해량</param>
    public void TakeDamage(float damage)
    {
        if (!isLive) return; // 이미 죽었다면 아무것도 하지 않음

        health -= damage;

        // 시간 정지 중이 아닐 때만 넉백 효과를 적용합니다.
        if (!GameManager.instance.isTimeStopped) StartCoroutine(KnockBack());

        if (health > 0)
        {
            // 체력이 남아있으면 피격 애니메이션 재생
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            // 사망 처리
            isLive = false;
            coll.enabled = false; // 다른 오브젝트와 더 이상 충돌하지 않도록
            rigid.simulated = false; // 물리 시뮬레이션 비활성화
            spriter.sortingOrder = 1; // 다른 오브젝트 뒤에 그려지도록
            
            anim.SetBool("Dead", true);

            // 게임 매니저에 처치 및 경험치 획득 알림
            GameManager.instance.kill++;
            DropLoot();

            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    /// <summary>
    /// 지속 피해 효과(장판 등)로부터 피해를 받습니다. 피해 간격(cooldown)을 체크합니다.
    /// </summary>
    /// <param name="damage">받은 피해량</param>
    /// <param name="cooldown">연속 피해 방지를 위한 최소 간격</param>
    public void TakeDamage(float damage, float cooldown)
    {
        // 마지막 피해 시간으로부터 cooldown이 지나지 않았다면 피해를 받지 않습니다.
        if (Time.time < lastNoxiousAftermathDamageTime + cooldown) return;

        lastNoxiousAftermathDamageTime = Time.time; // 마지막 피해 시간 갱신
        TakeDamage(damage); // 일반 피해 처리 로직 호출
    }

    /// <summary>
    /// 피격 시 넉백 효과를 처리하는 코루틴입니다.
    /// </summary>
    IEnumerator KnockBack()
    {
        isKnockBack = true;
        yield return wait; // 다음 물리 프레임까지 대기
        // 플레이어 반대 방향으로 힘을 가함
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration); // 짧은 시간 동안 넉백 상태 유지
        isKnockBack = false;
    }

    /// <summary>
    /// 사망 애니메이션이 끝난 후 호출되어 오브젝트를 풀에 반환합니다. (Animation Event에서 호출)
    /// </summary>
    public void Dead()
    {
        // GameManager에서 자신을 등록 해제합니다.
        GameManager.instance.UnregisterEnemy(this);
        
        // Poolable 컴포넌트에서 자신의 태그를 가져와 PoolManager에 반환합니다.
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            // Poolable이 없다면, 예전 방식대로 비활성화만 합니다.
            gameObject.SetActive(false);
        }
    }

    private void DropLoot()
    {
        if (lootTable.Length == 0)
        {
            return;
        }

        // 드롭 테이블의 각 아이템에 대해 드롭 확률을 계산합니다.
        foreach (var loot in lootTable)
        {
            // Random.value는 0과 1 사이의 무작위 실수를 반환합니다.
            if (Random.value <= loot.dropChance)
            {
                // 풀 매니저에서 태그를 사용하여 아이템을 가져옵니다.
                GameObject item = GameManager.instance.pool.Get(loot.itemTag);
                if (item != null)
                {
                    item.transform.position = transform.position;
                    item.SetActive(true); // 풀에서 가져온 아이템을 활성화합니다.
                }
            }
        }
    }

    /// <summary>
    /// 시간 정지 효과를 활성화하거나 비활성화합니다.
    /// </summary>
    public void SetTimeStopEffect(bool active)
    {
        if (active)
        {
            // 밝은 하늘색 외곽선과 색조를 적용합니다.
            spriter.color = new Color(0.7f, 0.9f, 1f, 1f); // 몸체 색상도 살짝 변경
        }
        else
        { 
            // 원래 색상으로 복원합니다.
            spriter.color = Color.white;
        }
    }
}