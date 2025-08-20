using System.Collections;
using UnityEngine;

/// <summary>
/// Alpha 보스의 Arc Blade 특수공격을 구현합니다.
/// </summary>
public class AlphaArcBladeAttack : SpecialAttackBase
{
    [Header("Arc Blade Settings")]
    [PoolTagSelector] public string chargeEffectPoolTag = "AlphaSpecialAttack";
    public float chargeDuration = 3f;
    public float swirlWaitDuration = 0.1f;
    public LayerMask playerLayer = 1 << 6;
    
    private AlphaChargeEffect currentChargeEffect;
    
    void Awake()
    {
        
        // Arc Blade 공격 데이터 설정
        if (attackData == null)
        {
            attackData = new SpecialAttackData
            {
                attackName = "Arc Blade",
                cooldown = 8f,
                priority = 5, // 높은 우선순위 설정
                minDistanceToPlayer = 0f,
                maxDistanceToPlayer = 15f,
                minHealthPercentage = 0f,
                maxHealthPercentage = 1f,
                canBeInterrupted = false,
                requiresLineOfSight = false
            };
        }
    }
    
    /// <summary>
    /// Arc Blade 공격 시퀀스를 실행합니다.
    /// </summary>
    protected override IEnumerator ExecuteAttackSequence()
    {
        try
        {
            // 1. 보스 완전 고정 (모든 움직임과 외부 힘 차단)
            StartBossImmobilization();
            
            // 2. 플레이어 방향 계산
            Vector2 playerDirection = GetDirectionToPlayer();
            
            // 3. Arc Blade 이펙트 생성 및 시작
            GameObject effectObject = CreateArcBladeEffect(playerDirection);
            if (effectObject != null)
            {
                currentChargeEffect = effectObject.GetComponent<AlphaChargeEffect>();
                if (currentChargeEffect != null)
                {
                    currentChargeEffect.StartArcBladeCharging(chargeDuration, playerDirection);
                }
            }
            
            // 4. 차징 시간 대기
            yield return new WaitForSeconds(chargeDuration);
            
            // 5. Swirl 효과 대기 (공격 실행)
            yield return new WaitForSeconds(swirlWaitDuration);
            
            // 6. 실제 공격 판정 실행
            if (currentChargeEffect != null)
            {
                ExecuteArcBladeAttack(currentChargeEffect.GetActualAttackRange(), playerDirection);
            }
            
            // 7. 추가 대기 (swirl 애니메이션 완료까지)
            yield return new WaitForSeconds(0.5f); // swirl 애니메이션 시간
        }
        finally
        {
            // 8. 보스 이동 복구
            EndBossImmobilization();
            
            // 9. 공격 완료 처리
            OnAttackComplete();
        }
    }
    
    /// <summary>
    /// Arc Blade 이펙트 오브젝트를 생성합니다.
    /// </summary>
    private GameObject CreateArcBladeEffect(Vector2 direction)
    {
        if (GameManager.instance?.pool == null || ownerBoss == null)
        {
            Debug.LogWarning("GameManager.pool or ownerBoss is null");
            return null;
        }
        
        // 풀에서 이펙트 오브젝트 가져오기
        GameObject effectObject = GameManager.instance.pool.Get(chargeEffectPoolTag);
        if (effectObject == null)
        {
            Debug.LogWarning($"Failed to get effect object from pool: {chargeEffectPoolTag}");
            return null;
        }
        
        // 위치 설정 (보스 위치)
        effectObject.transform.position = ownerBoss.transform.position;
        
        return effectObject;
    }
    
    /// <summary>
    /// Arc Blade 공격 판정을 실행합니다.
    /// </summary>
    private void ExecuteArcBladeAttack(float attackRange, Vector2 direction)
    {
        if (GameManager.instance?.player == null || ownerBoss == null) return;
        
        // 플레이어와의 거리 확인
        float distanceToPlayer = Vector2.Distance(ownerBoss.transform.position, GameManager.instance.player.transform.position);
        
        if (distanceToPlayer <= attackRange)
        {
            // 방향 확인 (반원 범위 내인지)
            Vector2 toPlayer = (GameManager.instance.player.transform.position - ownerBoss.transform.position).normalized;
            float angle = Vector2.Angle(direction, toPlayer);
            
            // 반원 범위 (90도) 내에 있는지 확인
            if (angle <= 90f)
            {
                // 플레이어에게 피해 적용
                Player player = GameManager.instance.player.GetComponent<Player>();
                if (player != null)
                {
                    // 보스 데이터에서 공격력 가져오기
                    float damage = ownerBoss.bossData != null ? ownerBoss.bossData.specialAttackDamage : ownerBoss.contactDamage * 2f;
                    player.TakeDamage(damage);
                    
                    Debug.Log($"Arc Blade hit player for {damage} damage");
                }
            }
        }
        
        Debug.Log($"Arc Blade executed - Range: {attackRange}, Distance to player: {distanceToPlayer}");
    }
    
    /// <summary>
    /// 공격이 중단될 때 호출됩니다.
    /// </summary>
    public override void InterruptAttack()
    {
        // Arc Blade는 중단 불가능하지만, 강제로 중단되는 경우 처리
        if (currentChargeEffect != null)
        {
            currentChargeEffect.StopArcBladeCharging();
            currentChargeEffect = null;
        }
        
        base.InterruptAttack();
    }
    
    /// <summary>
    /// 공격 완료 시 정리 작업을 수행합니다.
    /// </summary>
    protected override void OnAttackComplete()
    {
        currentChargeEffect = null;
        base.OnAttackComplete();
    }
}