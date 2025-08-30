using System.Collections;
using UnityEngine;

/// <summary>
/// 위클라인 보스의 신경가스 살포 특수공격을 구현합니다.
/// 위클라인을 중심으로 신경가스 영역을 생성하여 위클라인의 이동속도와 접촉 데미지를 증가시킵니다.
/// </summary>
public class WickelineNerveGasAttack : SpecialAttackBase
{
    [Header("Nerve Gas Settings")]
    [SerializeField] private float gasAreaDuration = 8f; // 신경가스 지속 시간
    [SerializeField] private float activationDelay = 1f; // 이펙트 재생 후 영역 활성화까지의 지연 시간
    
    [Header("Buff Settings")]
    [SerializeField] private float speedMultiplier = 1.5f; // 이동속도 증가 배율
    [SerializeField] private float damageMultiplier = 2f; // 접촉 데미지 증가 배율
    
    [Header("Visual Effects")]
    [SerializeField] private float fadeInDuration = 1.5f; // 스프라이트 페이드인 시간
    [SerializeField] private float fadeOutDuration = 1f; // 스프라이트 페이드아웃 시간
    
    [Header("Pool Settings")]
    [PoolTagSelector] public string nerveGasEffectPoolTag = "WickelineNerveGasEffect"; // 신경가스 이펙트 풀 태그
    
    // 현재 활성화된 신경가스 영역
    private NerveGasArea currentGasArea;
    
    void Awake()
    {
        // 신경가스 살포 공격 데이터 설정
        attackData = new SpecialAttackData
        {
            attackName = "Nerve Gas Deploy",
            cooldown = 18f,
            priority = 4,
            minDistanceToPlayer = 0f,
            maxDistanceToPlayer = 12f,
            minHealthPercentage = 0f,
            maxHealthPercentage = 1f,
            canBeInterrupted = false,
            requiresLineOfSight = false
        };
    }
    
    void Update()
    {
        // 신경가스 영역이 활성화되어 있을 때만 버프 상태 업데이트
        if (currentGasArea != null && currentGasArea.IsActive())
        {
            Debug.Log($"[DEBUG] NerveGas: Update - Gas area is active, updating buff");
            UpdateWickelineBuff();
        }
        else if (currentGasArea != null)
        {
            Debug.Log($"[DEBUG] NerveGas: Update - Gas area exists but not active. IsActive(): {currentGasArea.IsActive()}");
        }
    }
    
    /// <summary>
    /// 신경가스 살포 공격 시퀀스를 실행합니다.
    /// </summary>
    protected override IEnumerator ExecuteAttackSequence()
    {
        try
        {
            // 1. 신경가스 이펙트 오브젝트 생성
            GameObject gasEffect = CreateNerveGasEffect();
            if (gasEffect == null)
            {
                Debug.LogError("Nerve Gas: Failed to create gas effect");
                yield break;
            }
            
            currentGasArea = gasEffect.GetComponent<NerveGasArea>();
            if (currentGasArea == null)
            {
                Debug.LogError("Nerve Gas: NerveGasArea component not found");
                yield break;
            }
            
            // 2. 파티클 시스템 이펙트 재생 (물리적 고정)
            StartBossImmobilization();
            yield return StartCoroutine(PlayActivationEffect());
            
            // 파티클 효과 완료 후 보스 이동 허용 및 물리적 고정 해제 (버프 효과를 위해)
            EndBossImmobilization();
            isExecuting = false;
            
            // 3. 신경가스 영역 활성화 (스프라이트 페이드인)
            yield return StartCoroutine(ActivateGasArea());
            
            // 4. 신경가스 지속 시간 동안 대기
            yield return new WaitForSeconds(gasAreaDuration);
            
            // 5. 신경가스 영역 비활성화 (스프라이트 페이드아웃)
            yield return StartCoroutine(DeactivateGasArea());
        }
        finally
        {
            // 6. 정리 작업
            CleanupGasArea();
            // 이미 이동 허용 상태가 아니라면 완전히 종료
            if (isExecuting)
            {
                isExecuting = false;
            }
        }
    }
    
    /// <summary>
    /// 신경가스 이펙트 오브젝트를 풀에서 가져옵니다.
    /// </summary>
    private GameObject CreateNerveGasEffect()
    {
        if (GameManager.instance?.pool == null)
        {
            Debug.LogError("Nerve Gas: PoolManager not available");
            return null;
        }
        
        GameObject gasEffect = GameManager.instance.pool.Get(nerveGasEffectPoolTag);
        if (gasEffect == null)
        {
            Debug.LogError($"Nerve Gas: Failed to get gas effect from pool: {nerveGasEffectPoolTag}");
            return null;
        }
        
        // 위클라인 위치에 배치
        if (ownerBoss != null)
        {
            gasEffect.transform.position = ownerBoss.transform.position;
        }
        gasEffect.transform.rotation = Quaternion.identity;
        
        gasEffect.SetActive(true);
        
        return gasEffect;
    }
    
    /// <summary>
    /// 파티클 시스템을 사용한 활성화 이펙트를 재생합니다.
    /// </summary>
    private IEnumerator PlayActivationEffect()
    {
        if (currentGasArea == null) yield break;
        
        Debug.Log("Nerve Gas: Playing activation particle effect");
        
        // 파티클 시스템 재생
        currentGasArea.PlayActivationParticles();
        
        // 효과음 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Range); // 범위 공격 효과음
        }
        
        // 파티클 재생 시간 동안 대기
        yield return new WaitForSeconds(activationDelay);
    }
    
    /// <summary>
    /// 신경가스 영역을 활성화합니다 (스프라이트 페이드인).
    /// </summary>
    private IEnumerator ActivateGasArea()
    {
        if (currentGasArea == null) yield break;
        
        Debug.Log($"[DEBUG] NerveGas: Activating gas area with fade in");
        
        // 스프라이트 페이드인 시작
        currentGasArea.StartFadeIn(fadeInDuration);
        
        // 페이드인 완료까지 대기
        yield return new WaitForSeconds(fadeInDuration);
        
        Debug.Log($"[DEBUG] NerveGas: Gas area fully activated. IsActive(): {currentGasArea.IsActive()}");
    }
    
    /// <summary>
    /// 신경가스 영역을 비활성화합니다 (스프라이트 페이드아웃).
    /// </summary>
    private IEnumerator DeactivateGasArea()
    {
        if (currentGasArea == null) yield break;
        
        Debug.Log("Nerve Gas: Deactivating gas area with fade out");
        
        // 스프라이트 페이드아웃 시작
        currentGasArea.StartFadeOut(fadeOutDuration);
        
        // 페이드아웃 완료까지 대기
        yield return new WaitForSeconds(fadeOutDuration);
        
        Debug.Log("Nerve Gas: Gas area fully deactivated");
    }
    
    /// <summary>
    /// 위클라인이 신경가스 영역 안에 있는지 확인합니다.
    /// </summary>
    private bool IsWickelineInGasArea()
    {
        if (currentGasArea == null)
        {
            Debug.Log($"[DEBUG] NerveGas: IsWickelineInGasArea - currentGasArea is null");
            return false;
        }
        
        if (ownerBoss == null)
        {
            Debug.Log($"[DEBUG] NerveGas: IsWickelineInGasArea - ownerBoss is null");
            return false;
        }
        
        if (!currentGasArea.IsActive())
        {
            Debug.Log($"[DEBUG] NerveGas: IsWickelineInGasArea - Gas area is not active");
            return false;
        }
        
        // 신경가스 영역의 중심은 위클라인이 공격을 시작했던 위치
        Vector3 gasAreaCenter = currentGasArea.transform.position;
        Vector3 wickelinePosition = ownerBoss.transform.position;
        
        // RangeIndicator 기반의 실제 범위 사용
        float actualRange = currentGasArea.GetActualGasAreaRange();
        float distanceToCenter = Vector3.Distance(wickelinePosition, gasAreaCenter);
        
        bool inRange = distanceToCenter <= actualRange;
        
        Debug.Log($"[DEBUG] NerveGas: IsWickelineInGasArea - Center: {gasAreaCenter}, Wickeline: {wickelinePosition}, Distance: {distanceToCenter:F2}, Range: {actualRange:F2}, InRange: {inRange}");
        
        return inRange;
    }
    
    /// <summary>
    /// 위클라인의 버프 상태를 업데이트합니다 (위치 기반).
    /// </summary>
    private void UpdateWickelineBuff()
    {
        if (ownerBoss == null)
        {
            Debug.Log($"[DEBUG] NerveGas: UpdateWickelineBuff - ownerBoss is null!");
            return;
        }
        
        bool shouldHaveBuff = IsWickelineInGasArea();
        bool currentlyHasBuff = HasBuff();
        
        Debug.Log($"[DEBUG] NerveGas: UpdateWickelineBuff - shouldHaveBuff: {shouldHaveBuff}, currentlyHasBuff: {currentlyHasBuff}");
        
        if (shouldHaveBuff && !currentlyHasBuff)
        {
            Debug.Log($"[DEBUG] NerveGas: Applying buff to Wickeline");
            // 버프 적용
            ApplyBuff(true);
        }
        else if (!shouldHaveBuff && currentlyHasBuff)
        {
            Debug.Log($"[DEBUG] NerveGas: Removing buff from Wickeline");
            // 버프 제거
            ApplyBuff(false);
        }
        else
        {
            Debug.Log($"[DEBUG] NerveGas: No buff state change needed");
        }
    }
    
    /// <summary>
    /// 위클라인에게 실제로 버프를 적용하거나 제거합니다.
    /// </summary>
    private void ApplyBuff(bool apply)
    {
        if (ownerBoss == null)
        {
            Debug.Log($"[DEBUG] NerveGas: ApplyBuff - ownerBoss is null!");
            return;
        }
        
        if (apply)
        {
            // 버프 적용 전 상태 로깅
            Debug.Log($"[DEBUG] NerveGas: Before buff - Speed: {ownerBoss.speed}, ContactDamage: {ownerBoss.contactDamage}");
            
            // 버프 적용
            ownerBoss.speed *= speedMultiplier;
            ownerBoss.contactDamage *= damageMultiplier;
            
            Debug.Log($"[DEBUG] NerveGas: Applied buff to Wickeline - Speed: {ownerBoss.speed} (x{speedMultiplier}), ContactDamage: {ownerBoss.contactDamage} (x{damageMultiplier})");
        }
        else
        {
            // 버프 제거 전 상태 로깅
            Debug.Log($"[DEBUG] NerveGas: Before buff removal - Speed: {ownerBoss.speed}, ContactDamage: {ownerBoss.contactDamage}");
            
            // 버프 제거
            ownerBoss.speed /= speedMultiplier;
            ownerBoss.contactDamage /= damageMultiplier;
            
            Debug.Log($"[DEBUG] NerveGas: Removed buff from Wickeline - Speed: {ownerBoss.speed}, ContactDamage: {ownerBoss.contactDamage}");
        }
    }
    
    /// <summary>
    /// 위클라인이 현재 버프를 받고 있는지 확인합니다.
    /// </summary>
    private bool HasBuff()
    {
        if (ownerBoss == null) return false;
        
        // 버프가 적용되어 있다면 현재 속도가 기본 속도 * 배율과 유사해야 함
        // 기본 속도는 bossData에서 가져올 수 있음
        if (ownerBoss.bossData != null)
        {
            float expectedBuffedSpeed = ownerBoss.bossData.speed * speedMultiplier;
            bool hasBuff = Mathf.Abs(ownerBoss.speed - expectedBuffedSpeed) < 0.01f;
            
            Debug.Log($"[DEBUG] NerveGas: HasBuff - Current speed: {ownerBoss.speed}, Base speed: {ownerBoss.bossData.speed}, Expected buffed: {expectedBuffedSpeed}, HasBuff: {hasBuff}");
            
            return hasBuff;
        }
        
        Debug.Log($"[DEBUG] NerveGas: HasBuff - bossData is null, cannot determine buff state");
        return false;
    }
    
    /// <summary>
    /// 신경가스 영역을 정리합니다.
    /// </summary>
    private void CleanupGasArea()
    {
        if (currentGasArea != null)
        {
            currentGasArea.ForceCleanup();
            currentGasArea = null;
        }
    }
    
    /// <summary>
    /// 플레이어가 신경가스 영역에 있는지 확인합니다.
    /// </summary>
    public bool IsPlayerInGasArea()
    {
        if (currentGasArea == null || ownerBoss == null) return false;
        
        if (GameManager.instance?.player == null) return false;
        
        // RangeIndicator 기반의 실제 범위 사용
        float actualRange = currentGasArea.GetActualGasAreaRange();
        
        float distanceToPlayer = Vector3.Distance(
            ownerBoss.transform.position,
            GameManager.instance.player.transform.position
        );
        
        return distanceToPlayer <= actualRange && currentGasArea.IsActive();
    }
    
    /// <summary>
    /// 현재 신경가스 영역이 활성화되어 있는지 확인합니다.
    /// </summary>
    public bool IsGasAreaActive()
    {
        return currentGasArea != null && currentGasArea.IsActive();
    }
    
    /// <summary>
    /// 공격이 중단될 때 호출됩니다.
    /// </summary>
    public override void InterruptAttack()
    {
        // 물리적 고정 해제 (중단 시 안전장치)
        EndBossImmobilization();
        
        // 버프가 적용되어 있다면 제거
        if (HasBuff())
        {
            ApplyBuff(false);
        }
        
        // 신경가스 영역 정리
        CleanupGasArea();
        
        base.InterruptAttack();
    }
    
    /// <summary>
    /// 공격 완료 시 정리 작업을 수행합니다.
    /// </summary>
    protected override void OnAttackComplete()
    {
        // 물리적 고정 해제 (완료 시 안전장치)
        EndBossImmobilization();
        
        // 버프가 적용되어 있다면 제거
        if (HasBuff())
        {
            ApplyBuff(false);
        }
        
        CleanupGasArea();
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
            Gizmos.DrawWireSphere(bossPos, attackData?.maxDistanceToPlayer ?? 12f);
            
            // 신경가스 영역 표시 (실제 범위는 프리팹의 RangeIndicator 기반)
            if (currentGasArea != null)
            {
                float actualRange = currentGasArea.GetActualGasAreaRange();
                Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);
                Gizmos.DrawSphere(bossPos, actualRange);
                
                // 신경가스 영역 테두리
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(bossPos, actualRange);
            }
            
            // 활성화 상태 표시
            if (Application.isPlaying && IsGasAreaActive())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(bossPos + Vector3.up * 3f, Vector3.one);
            }
        }
    }
}