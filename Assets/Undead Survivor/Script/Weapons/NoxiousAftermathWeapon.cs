using UnityEngine;

/// <summary>
/// 플레이어가 지나간 자리에 연속적인 독성 장판을 생성하는 '유독성 발자국' 무기 로직을 처리합니다.
/// 거리 기반으로 장판을 생성하여 자연스러운 길을 만듭니다.
/// </summary>
public class NoxiousAftermathWeapon : WeaponBase
{
    [Header("독성 장판 설정")]
    [Tooltip("장판 생성 간격 (거리 기준)")]
    public float spawnDistance = 0.8f;
    
    [Tooltip("장판 겹침 정도 (0~1, 높을수록 더 촘촘)")]
    public float overlapFactor = 0.3f;
    
    [Tooltip("최소 이동 속도 (이보다 느리면 장판 생성 안함)")]
    public float minMoveSpeed = 0.1f;

    private Vector3 lastPosition;
    private float accumulatedDistance;
    private Vector3 lastMoveDirection;

    public override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        lastPosition = player.transform.position;
        lastMoveDirection = Vector3.zero;
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        Vector3 currentPosition = player.transform.position;
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
                
                SpawnPuddle(spawnPos, lastMoveDirection, moveDistance);
                accumulatedDistance -= spawnDistance * (1f - overlapFactor);
            }
        }
        
        lastPosition = currentPosition;
    }

    /// <summary>
    /// 독성 장판을 생성하고 초기화합니다.
    /// </summary>
    /// <param name="spawnPosition">장판이 생성될 위치</param>
    /// <param name="moveDirection">플레이어 이동 방향</param>
    /// <param name="moveSpeed">플레이어 이동 거리</param>
    private void SpawnPuddle(Vector3 spawnPosition, Vector3 moveDirection, float moveSpeed)
    {
        WeaponData weaponData = itemData as WeaponData;

        GameObject puddle = GameManager.instance.pool.Get(weaponData.projectileTag);
        if (puddle == null)
        {
            Debug.LogWarning($"PoolManager에서 태그 '{weaponData.projectileTag}'에 해당하는 독장판 프리팹을 가져오지 못했습니다. PoolManager 설정을 확인하세요.");
            return;
        }

        puddle.transform.position = spawnPosition;
        puddle.transform.rotation = Quaternion.identity;
        puddle.transform.localScale = Vector3.one * attackArea.Value;

        NoxiousAftermathEffect puddleLogic = puddle.GetComponent<NoxiousAftermathEffect>();
        if (puddleLogic != null)
        {
            puddleLogic.Init(damage.Value, duration.Value);
        }
        
        puddle.SetActive(true);

        // 효과음은 너무 자주 재생되지 않도록 확률적으로 재생
        if (UnityEngine.Random.value < 0.1f) // 10% 확률
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Melee);
        }
    }
}
