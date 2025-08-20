using System.Collections;
using UnityEngine;

/// <summary>
/// 특수공격 데이터를 정의하는 구조체입니다.
/// Priority 시스템을 사용하여 높은 우선순위 공격이 먼저 실행됩니다.
/// </summary>
[System.Serializable]
public class SpecialAttackData
{
    [Header("Attack Info")]
    public string attackName = "Special Attack";
    public float cooldown = 5f;
    public int priority = 1; // 우선순위 (높을수록 먼저 실행됨)
    
    [Header("Execution Conditions")]
    public float minDistanceToPlayer = 0f; // 최소 거리
    public float maxDistanceToPlayer = float.MaxValue; // 최대 거리
    public float minHealthPercentage = 0f; // 최소 체력 비율 (0~1)
    public float maxHealthPercentage = 1f; // 최대 체력 비율 (0~1)
    
    [Header("Settings")]
    public bool canBeInterrupted = false; // 공격 중 중단 가능 여부
    public bool requiresLineOfSight = false; // 시야 확보 필요 여부
    
    /// <summary>
    /// 현재 상황에서 이 공격을 사용할 수 있는지 확인합니다.
    /// </summary>
    public bool CanExecute(BossBase boss)
    {
        if (boss == null) return false;
        
        // 거리 조건 확인
        float distanceToPlayer = boss.GetDistanceToPlayer();
        if (distanceToPlayer < minDistanceToPlayer || distanceToPlayer > maxDistanceToPlayer)
            return false;
        
        // 체력 조건 확인
        float healthPercentage = boss.health / boss.maxHealth;
        if (healthPercentage < minHealthPercentage || healthPercentage > maxHealthPercentage)
            return false;
        
        // TODO: 시야 확보 조건 구현 (필요시)
        
        return true;
    }
}

/// <summary>
/// 보스의 특수공격을 정의하는 추상 클래스입니다.
/// </summary>
public abstract class SpecialAttackBase : MonoBehaviour
{
    [SerializeField] protected SpecialAttackData attackData;
    [SerializeField] protected bool isExecuting = false;
    
    protected BossBase ownerBoss;
    protected float lastExecutionTime = -999f;
    
    public SpecialAttackData AttackData => attackData;
    public bool IsExecuting => isExecuting;
    public bool IsOnCooldown => Time.time - lastExecutionTime < attackData.cooldown;
    
    /// <summary>
    /// 특수공격을 초기화합니다.
    /// </summary>
    public virtual void Initialize(BossBase boss)
    {
        ownerBoss = boss;
    }
    
    /// <summary>
    /// 이 특수공격을 실행할 수 있는지 확인합니다.
    /// </summary>
    public virtual bool CanExecute()
    {
        if (ownerBoss == null || isExecuting || IsOnCooldown)
            return false;
        
        return attackData.CanExecute(ownerBoss);
    }
    
    /// <summary>
    /// 특수공격을 실행합니다.
    /// </summary>
    public void Execute()
    {
        if (!CanExecute()) return;
        
        isExecuting = true;
        lastExecutionTime = Time.time;
        
        StartCoroutine(ExecuteAttackSequence());
    }
    
    /// <summary>
    /// 특수공격 시퀀스를 실행합니다. 하위 클래스에서 구현해야 합니다.
    /// </summary>
    protected abstract IEnumerator ExecuteAttackSequence();
    
    /// <summary>
    /// 특수공격을 중단합니다.
    /// </summary>
    public virtual void InterruptAttack()
    {
        if (!isExecuting || !attackData.canBeInterrupted) return;
        
        StopAllCoroutines();
        OnAttackComplete();
    }
    
    /// <summary>
    /// 공격 완료 시 호출됩니다.
    /// </summary>
    protected virtual void OnAttackComplete()
    {
        isExecuting = false;
    }
    
    /// <summary>
    /// 플레이어와의 거리를 반환합니다.
    /// </summary>
    protected float GetDistanceToPlayer()
    {
        return ownerBoss?.GetDistanceToPlayer() ?? float.MaxValue;
    }
    
    /// <summary>
    /// 플레이어 방향을 반환합니다.
    /// </summary>
    protected Vector2 GetDirectionToPlayer()
    {
        return ownerBoss?.GetDirectionToPlayer() ?? Vector2.zero;
    }
    
    /// <summary>
    /// 보스 고정을 시작합니다.
    /// </summary>
    protected void StartBossImmobilization()
    {
        ownerBoss?.StartSpecialAttackImmobilization();
    }
    
    /// <summary>
    /// 보스 고정을 해제합니다.
    /// </summary>
    protected void EndBossImmobilization()
    {
        ownerBoss?.EndSpecialAttackImmobilization();
    }
}