using UnityEngine;

/// <summary>
/// 위클라인이 이동하는 동안 지나간 자리에 연속적인 독성 장판을 생성하는 패시브 시스템입니다.
/// 플레이어의 NoxiousAftermathWeapon과 유사하지만, 플레이어에게 데미지를 입히는 몬스터 버전입니다.
/// </summary>
public class WickelineNoxiousAftermath : MonoBehaviour
{
    [Header("독성 장판 설정")]
    [Tooltip("장판 생성 간격 (거리 기준)")]
    public float spawnDistance = 1.2f;
    
    [Tooltip("장판 겹침 정도 (0~1, 높을수록 더 촘촘)")]
    public float overlapFactor = 0.4f;
    
    [Tooltip("최소 이동 속도 (이보다 느리면 장판 생성 안함)")]
    public float minMoveSpeed = 0.2f;
    
    [Header("장판 속성")]
    [Tooltip("독성 장판 데미지")]
    public float puddleDamage = 5f;
    
    [Tooltip("독성 장판 지속 시간")]
    public float puddleDuration = 8f;
    
    [Tooltip("독성 장판 크기")]
    public float puddleSize = 2f;
    
    [Header("Pool Settings")]
    [PoolTagSelector] public string puddlePoolTag = "WickelineNoxiousPuddle";
    
    [Header("활성화 설정")]
    [Tooltip("Noxious Aftermath 활성화 여부")]
    public bool isEnabled = true;

    // 추적 변수들
    private Vector3 lastPosition;
    private float accumulatedDistance;
    private Vector3 lastMoveDirection;
    private BossBase ownerBoss;

    void Awake()
    {
        ownerBoss = GetComponent<BossBase>();
        if (ownerBoss == null)
        {
            Debug.LogError("WickelineNoxiousAftermath: BossBase component not found!");
        }
    }

    void Start()
    {
        lastPosition = transform.position;
        lastMoveDirection = Vector3.zero;
        accumulatedDistance = 0f;
    }

    void Update()
    {
        if (!GameManager.instance.isLive || !isEnabled) return;
        
        // 특수공격 중에는 비활성화
        if (ownerBoss != null && ownerBoss.IsPerformingSpecialAttack()) return;
        
        ProcessMovement();
    }
    
    
    /// <summary>
    /// 이동을 처리하고 필요시 독성 장판을 생성합니다.
    /// </summary>
    private void ProcessMovement()
    {
        Vector3 currentPosition = transform.position;
        Vector3 moveVector = currentPosition - lastPosition;
        float moveDistance = moveVector.magnitude;
        
        // 최소 이동 거리 체크
        if (moveDistance > minMoveSpeed * Time.deltaTime)
        {
            accumulatedDistance += moveDistance;
            lastMoveDirection = moveVector.normalized;
            
            // 일정 거리마다 장판 생성
            while (accumulatedDistance >= spawnDistance)
            {
                // 연속적인 배치를 위해 약간 겹치도록 생성
                Vector3 spawnPos = Vector3.Lerp(lastPosition, currentPosition, 
                    1f - (accumulatedDistance / moveDistance));
                
                SpawnNoxiousPuddle(spawnPos, lastMoveDirection);
                accumulatedDistance -= spawnDistance * (1f - overlapFactor);
            }
        }
        
        lastPosition = currentPosition;
    }

    /// <summary>
    /// 독성 장판을 생성하고 초기화합니다.
    /// </summary>
    /// <param name="spawnPosition">장판이 생성될 위치</param>
    /// <param name="moveDirection">위클라인 이동 방향</param>
    private void SpawnNoxiousPuddle(Vector3 spawnPosition, Vector3 moveDirection)
    {
        if (GameManager.instance?.pool == null)
        {
            Debug.LogError("WickelineNoxiousAftermath: PoolManager not available");
            return;
        }

        GameObject puddle = GameManager.instance.pool.Get(puddlePoolTag);
        if (puddle == null)
        {
            Debug.LogWarning($"WickelineNoxiousAftermath: Failed to get puddle from pool '{puddlePoolTag}'");
            return;
        }

        // 장판 위치 및 설정
        puddle.transform.position = spawnPosition;
        puddle.transform.rotation = Quaternion.identity;
        puddle.transform.localScale = Vector3.one * puddleSize;

        // 장판 로직 초기화
        WickelineNoxiousArea puddleLogic = puddle.GetComponent<WickelineNoxiousArea>();
        if (puddleLogic != null)
        {
            puddleLogic.Init(puddleDamage, puddleDuration, ownerBoss);
        }
        else
        {
            Debug.LogWarning("WickelineNoxiousAftermath: WickelineNoxiousArea component not found on puddle");
        }
        
        puddle.SetActive(true);
        
    }
    
    /// <summary>
    /// Noxious Aftermath를 활성화/비활성화합니다.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }
    
    /// <summary>
    /// 장판 속성을 런타임에 변경합니다.
    /// </summary>
    public void UpdatePuddleProperties(float damage, float duration, float size)
    {
        puddleDamage = damage;
        puddleDuration = duration;
        puddleSize = size;
    }
    
    /// <summary>
    /// 현재 활성화 상태를 반환합니다.
    /// </summary>
    public bool IsCurrentlyActive()
    {
        return isEnabled && GameManager.instance.isLive && 
               (ownerBoss == null || !ownerBoss.IsPerformingSpecialAttack());
    }
    
    /// <summary>
    /// 강제로 상태를 리셋합니다.
    /// </summary>
    public void ResetState()
    {
        lastPosition = transform.position;
        accumulatedDistance = 0f;
        lastMoveDirection = Vector3.zero;
    }
    
    /// <summary>
    /// 에디터에서 시각화를 위한 기즈모를 그립니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!isEnabled) return;
        
        // 장판 생성 거리 표시
        Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnDistance);
        
        // 장판 크기 미리보기
        Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.4f);
        Gizmos.DrawSphere(transform.position, puddleSize * 0.5f);
        
        // 활성화 상태 표시
        if (Application.isPlaying && IsCurrentlyActive())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
    }
}