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
    
    protected override void Awake()
    {
        base.Awake();
        healthBar = GetComponentInChildren<BossHealthBar>();
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
        
        // 특별한 드롭 처리 (기존 DropLoot 대신)
        DropBossLoot();
        
        // 체력바 숨김
        if (healthBar != null)
        {
            healthBar.Hide();
        }
        
        // 보스 처치 알림
        ShowBossDefeatedNotification();
    }
    
    /// <summary>
    /// 보스 전용 드롭 시스템 (일반 적보다 높은 확률)
    /// </summary>
    protected virtual void DropBossLoot()
    {
        if (bossData.lootTable.Length == 0) return;
        
        foreach (var loot in bossData.lootTable)
        {
            // 보스는 더 높은 확률로 아이템을 드롭 (1.5배)
            float adjustedChance = Mathf.Min(loot.dropChance * 1.5f, 1f);
            
            if (Random.value <= adjustedChance)
            {
                GameObject item = GameManager.instance.pool.Get(loot.itemTag);
                if (item != null)
                {
                    item.transform.position = transform.position;
                    item.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// 보스 등장 알림을 표시합니다.
    /// </summary>
    protected virtual void ShowBossAppearanceNotification()
    {
        if (BossNotificationUI.Instance != null)
        {
            BossNotificationUI.Instance.ShowBossAppearance(bossData.bossName);
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
            BossNotificationUI.Instance.ShowBossDefeated(bossData.bossName);
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
}