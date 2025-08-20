using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Omega 보스의 VF Pulse 특수공격을 구현합니다.
/// 플레이어 위치에 범위 표시 후 폭발하는 공격입니다.
/// </summary>
public class OmegaVFPulseAttack : SpecialAttackBase
{
    [Header("VF Pulse Settings")]
    [PoolTagSelector] public string vfPulseEffectPoolTag = "VFPulseEffect"; // VF Pulse 이펙트 풀 태그
    [SerializeField] private float warningDuration = 2f; // 경고 표시 시간
    [SerializeField] private float explosionDuration = 0.5f; // 폭발 지속 시간
    [SerializeField] private float attackRadius = 4f; // 공격 반경
    [SerializeField] private LayerMask playerLayer = 1 << 6; // 플레이어 레이어
    [SerializeField] private float damage = 50f; // 공격 데미지
    
    [Header("Visual Effects")]
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 펄스 애니메이션 커브
    
    private VFPulseEffect currentPulseEffect;
    
    void Awake()
    {
        // VF Pulse 공격 데이터 설정
        attackData = new SpecialAttackData
        {
            attackName = "VF Pulse",
            cooldown = 8f,
            priority = 4, // Arc Blade보다 낮은 우선순위
            minDistanceToPlayer = 0f,
            maxDistanceToPlayer = 20f,
            minHealthPercentage = 0f,
            maxHealthPercentage = 1f,
            canBeInterrupted = false,
            requiresLineOfSight = false
        };
    }
    
    /// <summary>
    /// VF Pulse 공격 시퀀스를 실행합니다.
    /// </summary>
    protected override IEnumerator ExecuteAttackSequence()
    {
        try
        {
            // 보스 고정은 하지 않음 (원거리 공격)
            
            // 1. 플레이어 현재 위치 저장
            Vector3 targetPosition = GetPlayerPosition();
            if (targetPosition == Vector3.zero)
            {
                Debug.LogWarning("VF Pulse: Player position not found");
                yield break;
            }
            
            // 2. VF Pulse 이펙트 생성
            GameObject effectObject = CreateVFPulseEffect(targetPosition);
            if (effectObject == null)
            {
                Debug.LogError("VF Pulse: Failed to create effect object");
                yield break;
            }
            
            currentPulseEffect = effectObject.GetComponent<VFPulseEffect>();
            if (currentPulseEffect == null)
            {
                Debug.LogError("VF Pulse: VFPulseEffect component not found");
                Destroy(effectObject);
                yield break;
            }
            
            // 3. 경고 단계 시작
            currentPulseEffect.StartWarningPhase(warningDuration, attackRadius, pulseCurve);
            
            // 4. 경고 시간 대기
            yield return new WaitForSeconds(warningDuration);
            
            // 5. 폭발 단계 시작
            currentPulseEffect.StartExplosionPhase(explosionDuration);
            
            // 6. 데미지 판정 실행 (실제 범위 사용)
            float actualRange = currentPulseEffect.GetActualAttackRange();
            ExecuteDamageCheck(targetPosition, actualRange);
            
            // 7. 폭발 애니메이션 대기
            yield return new WaitForSeconds(explosionDuration);
            
            // 8. 이펙트 정리
            if (currentPulseEffect != null)
            {
                currentPulseEffect.ReturnToPool();
            }
        }
        finally
        {
            currentPulseEffect = null;
            OnAttackComplete();
        }
    }
    
    /// <summary>
    /// 플레이어의 현재 위치를 반환합니다.
    /// </summary>
    private Vector3 GetPlayerPosition()
    {
        if (GameManager.instance?.player != null)
        {
            return GameManager.instance.player.transform.position;
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// VF Pulse 이펙트 오브젝트를 풀에서 가져옵니다.
    /// </summary>
    private GameObject CreateVFPulseEffect(Vector3 position)
    {
        if (GameManager.instance?.pool == null)
        {
            Debug.LogError("VF Pulse: PoolManager is not available");
            return null;
        }
        
        // 풀에서 이펙트 오브젝트 가져오기
        GameObject effectObject = GameManager.instance.pool.Get(vfPulseEffectPoolTag);
        if (effectObject == null)
        {
            Debug.LogError($"VF Pulse: Failed to get effect object from pool: {vfPulseEffectPoolTag}");
            return null;
        }
        
        // 오브젝트 활성화 (코루틴 시작 전에 필수)
        effectObject.SetActive(true);
        
        // 위치 설정
        effectObject.transform.position = position;
        effectObject.transform.rotation = Quaternion.identity;
        
        return effectObject;
    }
    
    /// <summary>
    /// 폭발 지점에서 데미지 판정을 실행합니다.
    /// </summary>
    private void ExecuteDamageCheck(Vector3 explosionCenter, float actualAttackRadius)
    {
        if (GameManager.instance?.player == null) return;
        
        // 플레이어와 폭발 중심 간의 거리 계산
        float distanceToPlayer = Vector3.Distance(explosionCenter, GameManager.instance.player.transform.position);
        
        // 실제 공격 범위 내에 플레이어가 있는지 확인
        if (distanceToPlayer <= actualAttackRadius)
        {
            // 플레이어에게 피해 적용
            Player player = GameManager.instance.player.GetComponent<Player>();
            if (player != null)
            {
                // 보스 데이터에서 데미지 값 사용 (설정되어 있다면)
                float actualDamage = ownerBoss?.bossData?.specialAttackDamage ?? damage;
                player.TakeDamage(actualDamage);
                
                Debug.Log($"VF Pulse hit player for {actualDamage} damage at distance {distanceToPlayer:F1} (radius: {actualAttackRadius:F1})");
            }
        }
        else
        {
            Debug.Log($"VF Pulse missed - Player distance: {distanceToPlayer:F1}, Attack radius: {actualAttackRadius:F1}");
        }
    }
    
    /// <summary>
    /// 공격이 중단될 때 호출됩니다.
    /// </summary>
    public override void InterruptAttack()
    {
        if (currentPulseEffect != null)
        {
            currentPulseEffect.ReturnToPool();
            currentPulseEffect = null;
        }
        
        base.InterruptAttack();
    }
    
    /// <summary>
    /// 공격 완료 시 정리 작업을 수행합니다.
    /// </summary>
    protected override void OnAttackComplete()
    {
        currentPulseEffect = null;
        base.OnAttackComplete();
    }
    
    /// <summary>
    /// 에디터에서 공격 범위를 시각화합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (ownerBoss != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ownerBoss.transform.position, attackRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ownerBoss.transform.position, attackData?.maxDistanceToPlayer ?? 20f);
            
            // RangeIndicator 시각화 (사용 가능한 경우)
            VFPulseEffect effectComponent = GetComponent<VFPulseEffect>();
            if (effectComponent != null && effectComponent.rangeIndicator != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 indicatorWorldPos = transform.TransformPoint(effectComponent.rangeIndicator.localPosition);
                Gizmos.DrawWireSphere(indicatorWorldPos, 0.2f);
                Gizmos.DrawLine(transform.position, indicatorWorldPos);
            }
        }
    }
}