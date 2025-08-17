using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 몬스터의 기본 클래스입니다. Enemy를 상속받아 기본 기능을 유지하면서 보스만의 특별한 기능을 추가합니다.
/// </summary>
public abstract class BossBase : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] protected BossDataSO bossData;
    [SerializeField] protected BossHealthBar healthBar;
    
    protected float specialAttackTimer;
    protected bool isDead = false;
    
    // Special attack immobilization system
    protected bool isPerformingSpecialAttack = false;
    private RigidbodyConstraints2D originalConstraints;
    
    protected override void Awake()
    {
        base.Awake();
        healthBar = GetComponentInChildren<BossHealthBar>();
        
        // Store original rigidbody constraints
        if (rigid != null)
        {
            originalConstraints = rigid.constraints;
        }
    }
    
    protected virtual void Start()
    {
        if (bossData != null)
        {
            InitializeBoss(bossData);
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!GameManager.instance.isLive || isDead) return;
        
        specialAttackTimer += Time.deltaTime;
        if (specialAttackTimer >= bossData.specialAttackCooldown)
        {
            specialAttackTimer = 0f;
            PerformSpecialAttack();
        }
    }
    
    /// <summary>
    /// 보스 데이터로 초기화합니다.
    /// </summary>
    public virtual void InitializeBoss(BossDataSO data)
    {
        bossData = data;
        
        // Enemy의 기본 스탯 설정
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
        contactDamage = data.contactDamage;
        lootTable = data.lootTable;
        
        // 체력바 초기화
        if (healthBar != null)
        {
            healthBar.InitializeHealthBar(data.bossName, maxHealth);
        }
        
        // 보스 등장 알림
        ShowBossAppearanceNotification();
    }
    
    /// <summary>
    /// 보스의 특수 공격을 수행합니다. 하위 클래스에서 구현해야 합니다.
    /// </summary>
    protected abstract void PerformSpecialAttack();
    
    /// <summary>
    /// 피해를 받을 때 체력바를 업데이트합니다.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        
        base.TakeDamage(damage);
        
        // 체력바 업데이트
        if (healthBar != null)
        {
            healthBar.UpdateHealth(health, maxHealth);
        }
        
        // 보스가 죽었을 때 특별한 처리
        if (health <= 0 && !isDead)
        {
            isDead = true;
            OnBossDeath();
        }
    }
    
    /// <summary>
    /// 보스 사망 시 특별한 처리를 수행합니다.
    /// </summary>
    protected virtual void OnBossDeath()
    {
        // 경험치 보상
        GameManager.instance.GetExp(bossData.expReward);
        
        // 체력바 숨김
        if (healthBar != null)
        {
            healthBar.Hide();
        }
        
        // 보스 처치 알림
        ShowBossDefeatedNotification();
    }
    
    /// <summary>
    /// 보스 등장 알림을 표시합니다.
    /// </summary>
    protected virtual void ShowBossAppearanceNotification()
    {
        if (BossNotificationUI.Instance != null)
        {
            BossNotificationUI.Instance.ShowBossAppearance(bossData.bossName, bossData.bossIcon);
        }
        else
        {
            Debug.Log($"보스 '{bossData.bossName}' 등장!");
        }
    }
    
    /// <summary>
    /// 보스 처치 알림을 표시합니다.
    /// </summary>
    protected virtual void ShowBossDefeatedNotification()
    {
        if (BossNotificationUI.Instance != null)
        {
            BossNotificationUI.Instance.ShowBossDefeated(bossData.bossName, bossData.bossIcon);
        }
        else
        {
            Debug.Log($"보스 '{bossData.bossName}' 처치!");
        }
    }
    
    /// <summary>
    /// 플레이어와의 거리를 반환합니다.
    /// </summary>
    protected float GetDistanceToPlayer()
    {
        if (GameManager.instance?.player == null) return float.MaxValue;
        
        return Vector2.Distance(transform.position, GameManager.instance.player.transform.position);
    }
    
    /// <summary>
    /// 플레이어 방향 벡터를 반환합니다.
    /// </summary>
    protected Vector2 GetDirectionToPlayer()
    {
        if (GameManager.instance?.player == null) return Vector2.zero;
        
        Vector2 direction = GameManager.instance.player.transform.position - transform.position;
        return direction.normalized;
    }
    
    /// <summary>
    /// 특수 공격 중 보스를 완전히 고정시킵니다.
    /// Rigidbody constraints를 사용하여 위치를 고정하고 모든 외부 힘을 차단합니다.
    /// </summary>
    protected virtual void StartSpecialAttackImmobilization()
    {
        if (rigid == null) return;
        
        isPerformingSpecialAttack = true;
        
        // 현재 속도를 0으로 설정
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        
        // 위치와 회전을 고정 (모든 축)
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        
        Debug.Log($"{gameObject.name}: Special attack immobilization started");
    }
    
    /// <summary>
    /// 특수 공격이 끝난 후 보스의 이동을 복구합니다.
    /// </summary>
    protected virtual void EndSpecialAttackImmobilization()
    {
        if (rigid == null) return;
        
        isPerformingSpecialAttack = false;
        
        // 원래 제약 조건으로 복구
        rigid.constraints = originalConstraints;
        
        Debug.Log($"{gameObject.name}: Special attack immobilization ended");
    }
    
    /// <summary>
    /// 특수 공격 상태 확인
    /// </summary>
    public bool IsPerformingSpecialAttack()
    {
        return isPerformingSpecialAttack;
    }
}