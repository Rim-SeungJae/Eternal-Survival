using System.Collections;
using UnityEngine;

/// <summary>
/// 위클라인 보스의 트리플렛 코드 특수공격을 구현합니다.
/// 플레이어 방향을 기준으로 3개 방향(0도, +30도, -30도)으로 작살 투사체를 발사합니다.
/// </summary>
public class WickelineTripletCodeAttack : SpecialAttackBase
{
    [Header("Triplet Code Settings")]
    [SerializeField] private float trajectoryDisplayDuration = 1f; // 궤적 표시 시간
    [SerializeField] private float projectileSpeed = 15f; // 투사체 속도
    [SerializeField] private float projectileLifetime = 3f; // 투사체 생존 시간
    [SerializeField] private float damage = 30f; // 투사체 데미지
    [SerializeField] private float angleOffset = 30f; // 좌우 투사체 각도 오프셋
    
    [Header("Pool Settings")]
    [PoolTagSelector] public string trajectoryIndicatorPoolTag = "TrajectoryIndicator"; // 궤적 표시 풀 태그
    [PoolTagSelector] public string projectilePoolTag = "TripletCodeProjectile"; // 투사체 풀 태그
    
    [Header("Visual Settings")]
    [SerializeField] private float trajectoryLineWidth = 0.1f; // 궤적 표시 선 굵기
    [SerializeField] private float trajectoryLength = 10f; // 궤적 표시 길이
    
    // 현재 활성화된 궤적 표시들
    private TrajectoryIndicator[] currentTrajectoryIndicators = new TrajectoryIndicator[3];
    
    void Awake()
    {
        // 트리플렛 코드 공격 데이터 설정
        attackData = new SpecialAttackData
        {
            attackName = "Triplet Code",
            cooldown = 6f,
            priority = 5,
            minDistanceToPlayer = 0f,
            maxDistanceToPlayer = 20f,
            minHealthPercentage = 0f,
            maxHealthPercentage = 1f,
            canBeInterrupted = false,
            requiresLineOfSight = true
        };
    }
    
    /// <summary>
    /// 트리플렛 코드 공격 시퀀스를 실행합니다.
    /// </summary>
    protected override IEnumerator ExecuteAttackSequence()
    {
        try
        {
            // 1. 플레이어 방향 계산
            Vector2 playerDirection = GetDirectionToPlayer();
            if (playerDirection == Vector2.zero)
            {
                Debug.LogWarning("Triplet Code: Player direction not found");
                yield break;
            }
            
            // 2. 3개 방향 계산 (중앙, 좌측 +30도, 우측 -30도)
            Vector2[] attackDirections = CalculateAttackDirections(playerDirection);
            
            // 3. 궤적 표시 시작
            yield return StartCoroutine(DisplayTrajectories(attackDirections));
            
            // 4. 투사체 발사
            LaunchProjectiles(attackDirections);
            
            // 5. 투사체들이 완료될 때까지 대기
            yield return new WaitForSeconds(0.5f);
        }
        finally
        {
            // 궤적 표시 정리
            CleanupTrajectoryIndicators();
            OnAttackComplete();
        }
    }
    
    /// <summary>
    /// 플레이어 방향을 기준으로 3개의 공격 방향을 계산합니다.
    /// </summary>
    private Vector2[] CalculateAttackDirections(Vector2 baseDirection)
    {
        Vector2[] directions = new Vector2[3];
        
        // 기본 방향 (플레이어 방향)
        directions[0] = baseDirection.normalized;
        
        // 좌측 방향 (+30도)
        float leftAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) + (angleOffset * Mathf.Deg2Rad);
        directions[1] = new Vector2(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle)).normalized;
        
        // 우측 방향 (-30도)
        float rightAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) - (angleOffset * Mathf.Deg2Rad);
        directions[2] = new Vector2(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle)).normalized;
        
        return directions;
    }
    
    /// <summary>
    /// 각 방향에 대한 궤적을 표시합니다.
    /// </summary>
    private IEnumerator DisplayTrajectories(Vector2[] directions)
    {
        if (GameManager.instance?.pool == null)
        {
            Debug.LogError("Triplet Code: PoolManager not available");
            yield break;
        }
        
        // 각 방향에 대해 궤적 표시 생성
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject indicatorObj = GameManager.instance.pool.Get(trajectoryIndicatorPoolTag);
            if (indicatorObj != null)
            {
                indicatorObj.SetActive(true);
                TrajectoryIndicator indicator = indicatorObj.GetComponent<TrajectoryIndicator>();
                
                if (indicator != null)
                {
                    // 궤적 설정
                    Vector3 startPos = ownerBoss.transform.position;
                    Vector3 endPos = startPos + (Vector3)(directions[i] * trajectoryLength);
                    
                    indicator.SetupTrajectory(startPos, endPos,  trajectoryLineWidth);
                    indicator.StartDisplay(trajectoryDisplayDuration);
                    
                    currentTrajectoryIndicators[i] = indicator;
                }
                else
                {
                    Debug.LogWarning($"TrajectoryIndicator component not found on pooled object: {trajectoryIndicatorPoolTag}");
                }
            }
        }
        
        // 궤적 표시 시간만큼 대기
        yield return new WaitForSeconds(trajectoryDisplayDuration);
    }
    
    /// <summary>
    /// 3개 방향으로 투사체를 발사합니다.
    /// </summary>
    private void LaunchProjectiles(Vector2[] directions)
    {
        if (GameManager.instance?.pool == null)
        {
            Debug.LogError("Triplet Code: PoolManager not available for projectiles");
            return;
        }
        
        Vector3 launchPosition = ownerBoss.transform.position;
        
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject projectileObj = GameManager.instance.pool.Get(projectilePoolTag);
            if (projectileObj != null)
            {
                projectileObj.SetActive(true);
                projectileObj.transform.position = launchPosition;
                
                TripletCodeProjectile projectile = projectileObj.GetComponent<TripletCodeProjectile>();
                if (projectile != null)
                {
                    // 투사체 설정 및 발사
                    projectile.Launch(directions[i], projectileSpeed, projectileLifetime, damage, ownerBoss);
                }
                else
                {
                    Debug.LogWarning($"TripletCodeProjectile component not found on pooled object: {projectilePoolTag}");
                }
            }
            else
            {
                Debug.LogError($"Failed to get projectile from pool: {projectilePoolTag}");
            }
        }
        
        // 발사 효과음
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
        }
    }
    
    /// <summary>
    /// 활성화된 궤적 표시들을 정리합니다.
    /// </summary>
    private void CleanupTrajectoryIndicators()
    {
        for (int i = 0; i < currentTrajectoryIndicators.Length; i++)
        {
            if (currentTrajectoryIndicators[i] != null)
            {
                currentTrajectoryIndicators[i].ForceCleanup();
                currentTrajectoryIndicators[i] = null;
            }
        }
    }
    
    /// <summary>
    /// 공격이 중단될 때 호출됩니다.
    /// </summary>
    public override void InterruptAttack()
    {
        CleanupTrajectoryIndicators();
        base.InterruptAttack();
    }
    
    /// <summary>
    /// 공격 완료 시 정리 작업을 수행합니다.
    /// </summary>
    protected override void OnAttackComplete()
    {
        CleanupTrajectoryIndicators();
        base.OnAttackComplete();
    }
    
    /// <summary>
    /// 에디터에서 공격 범위를 시각화합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (ownerBoss != null)
        {
            Vector3 bossPos = ownerBoss.transform.position;
            
            // 최대 공격 거리 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(bossPos, attackData?.maxDistanceToPlayer ?? 20f);
            
            // 궤적 길이 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossPos, trajectoryLength);
            
            // 플레이어가 있으면 실제 공격 방향 미리보기
            if (GameManager.instance?.player != null)
            {
                Vector2 playerDir = GetDirectionToPlayer();
                if (playerDir != Vector2.zero)
                {
                    Vector2[] dirs = CalculateAttackDirections(playerDir);
                    Gizmos.color = Color.cyan;
                    
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        Vector3 endPos = bossPos + (Vector3)(dirs[i] * trajectoryLength);
                        Gizmos.DrawLine(bossPos, endPos);
                    }
                }
            }
        }
    }
}