using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보스 몬스터의 기본 클래스입니다. Enemy를 상속받아 기본 기능을 유지하면서 보스만의 특별한 기능을 추가합니다.
/// </summary>
public abstract class BossBase : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] public BossDataSO bossData;
    [SerializeField] protected BossHealthBar healthBar;
    
    [Header("Special Attack System")]
    [SerializeField] protected List<SpecialAttackBase> specialAttacks = new List<SpecialAttackBase>();
    [SerializeField] protected float globalSpecialAttackTimer = 0f;
    [SerializeField] protected float minTimeBetweenAttacks = 1f; // 공격 간 최소 간격
    
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
        
        // 특수공격 시스템 초기화
        InitializeSpecialAttacks();
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!GameManager.instance.isLive || isDead) return;
        
        globalSpecialAttackTimer += Time.deltaTime;
        
        // 최소 간격이 지났고, 현재 특수공격을 수행중이지 않을 때만 새 공격 시도
        if (globalSpecialAttackTimer >= minTimeBetweenAttacks && !IsPerformingSpecialAttack())
        {
            TryExecuteSpecialAttack();
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
    /// 사용 가능한 특수공격을 선택하고 실행합니다.
    /// </summary>
    protected virtual void TryExecuteSpecialAttack()
    {
        // 실행 가능한 공격들을 필터링
        var availableAttacks = specialAttacks.Where(attack => attack != null && attack.CanExecute()).ToList();
        
        if (availableAttacks.Count == 0) return;
        
        // 우선순위 기반 선택
        SpecialAttackBase selectedAttack = SelectAttackByPriority(availableAttacks);
        
        if (selectedAttack != null)
        {
            globalSpecialAttackTimer = 0f; // 타이머 리셋
            selectedAttack.Execute();
        }
    }
    
    /// <summary>
    /// 우선순위를 기반으로 특수공격을 선택합니다.
    /// 가장 높은 우선순위를 가진 공격을 선택합니다.
    /// </summary>
    protected virtual SpecialAttackBase SelectAttackByPriority(List<SpecialAttackBase> availableAttacks)
    {
        if (availableAttacks.Count == 0) return null;
        if (availableAttacks.Count == 1) return availableAttacks[0];
        
        // 우선순위로 정렬 (높은 우선순위가 먼저)
        var sortedAttacks = availableAttacks.OrderByDescending(attack => attack.AttackData.priority).ToList();
        
        // 가장 높은 우선순위 값 확인
        int highestPriority = sortedAttacks[0].AttackData.priority;
        
        // 같은 우선순위를 가진 공격들 필터링
        var highestPriorityAttacks = sortedAttacks.Where(attack => attack.AttackData.priority == highestPriority).ToList();
        
        // 같은 우선순위가 여러 개인 경우 랜덤 선택
        if (highestPriorityAttacks.Count > 1)
        {
            int randomIndex = Random.Range(0, highestPriorityAttacks.Count);
            return highestPriorityAttacks[randomIndex];
        }
        
        return highestPriorityAttacks[0];
    }
    
    /// <summary>
    /// 레거시 지원: 하위 클래스에서 오버라이드 가능한 특수공격 메서드
    /// 새 시스템을 사용하지 않는 기존 보스들을 위한 호환성 제공
    /// </summary>
    protected virtual void PerformSpecialAttack()
    {
        // 기본적으로 새 시스템 사용
        TryExecuteSpecialAttack();
    }
    
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
        // 진행 중인 모든 특수공격 중단
        InterruptAllSpecialAttacks();
        
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
    public float GetDistanceToPlayer()
    {
        if (GameManager.instance?.player == null) return float.MaxValue;
        
        return Vector2.Distance(transform.position, GameManager.instance.player.transform.position);
    }
    
    /// <summary>
    /// 플레이어 방향 벡터를 반환합니다.
    /// </summary>
    public Vector2 GetDirectionToPlayer()
    {
        if (GameManager.instance?.player == null) return Vector2.zero;
        
        Vector2 direction = GameManager.instance.player.transform.position - transform.position;
        return direction.normalized;
    }
    
    /// <summary>
    /// 특수 공격 중 보스를 완전히 고정시킵니다.
    /// Rigidbody constraints를 사용하여 위치를 고정하고 모든 외부 힘을 차단합니다.
    /// </summary>
    public virtual void StartSpecialAttackImmobilization()
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
    public virtual void EndSpecialAttackImmobilization()
    {
        if (rigid == null) return;
        
        isPerformingSpecialAttack = false;
        
        // 원래 제약 조건으로 복구
        rigid.constraints = originalConstraints;
        
        Debug.Log($"{gameObject.name}: Special attack immobilization ended");
    }
    
    /// <summary>
    /// 특수 공격 상태 확인 (기존 시스템 + 새 시스템)
    /// </summary>
    public bool IsPerformingSpecialAttack()
    {
        // 기존 시스템 확인
        if (isPerformingSpecialAttack) return true;
        
        // 새 시스템 확인
        return specialAttacks.Any(attack => attack != null && attack.IsExecuting);
    }
    
    /// <summary>
    /// 특수공격 시스템을 초기화합니다.
    /// </summary>
    protected virtual void InitializeSpecialAttacks()
    {
        // 자식 컴포넌트에서 SpecialAttackBase 찾기
        var attacks = GetComponentsInChildren<SpecialAttackBase>();
        
        specialAttacks.Clear();
        foreach (var attack in attacks)
        {
            attack.Initialize(this);
            specialAttacks.Add(attack);
        }
        
        Debug.Log($"{gameObject.name}: Initialized {specialAttacks.Count} special attacks");
    }
    
    /// <summary>
    /// 특수공격을 추가합니다.
    /// </summary>
    public virtual void AddSpecialAttack(SpecialAttackBase attack)
    {
        if (attack != null && !specialAttacks.Contains(attack))
        {
            attack.Initialize(this);
            specialAttacks.Add(attack);
        }
    }
    
    /// <summary>
    /// 특수공격을 제거합니다.
    /// </summary>
    public virtual void RemoveSpecialAttack(SpecialAttackBase attack)
    {
        if (attack != null)
        {
            specialAttacks.Remove(attack);
        }
    }
    
    /// <summary>
    /// 모든 특수공격을 중단합니다.
    /// </summary>
    protected virtual void InterruptAllSpecialAttacks()
    {
        foreach (var attack in specialAttacks)
        {
            if (attack != null && attack.IsExecuting)
            {
                attack.InterruptAttack();
            }
        }
    }
}