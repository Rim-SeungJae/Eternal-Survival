using System.Collections;
using UnityEngine;

/// <summary>
/// Omega 보스 몬스터입니다. BossBase를 상속받아 다중 특수공격을 구현합니다.
/// 원형 미사일 공격과 텔레포트 공격 두 종류의 특수공격을 보유합니다.
/// </summary>
public class OmegaBoss : BossBase
{
    [Header("Omega Boss Settings")]
    [SerializeField] private bool enableDebugLogs = true; // 디버그 로그 활성화
    
    /// <summary>
    /// Omega 보스 초기화 시 특수공격 시스템을 설정합니다.
    /// </summary>
    protected override void InitializeSpecialAttacks()
    {
        base.InitializeSpecialAttacks();
        
        if (enableDebugLogs)
        {
            Debug.Log($"Omega Boss initialized with {specialAttacks.Count} special attacks");
            
            // 각 특수공격의 정보 출력
            for (int i = 0; i < specialAttacks.Count; i++)
            {
                if (specialAttacks[i] != null)
                {
                    var attackData = specialAttacks[i].AttackData;
                    Debug.Log($"Special Attack {i + 1}: {attackData.attackName} (Priority: {attackData.priority}, Cooldown: {attackData.cooldown}s)");
                }
            }
        }
    }
    
    /// <summary>
    /// Omega 보스의 특수 행동을 업데이트합니다.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        
        // 추가적인 Omega 보스 전용 로직이 필요한 경우 여기에 구현
        // 예: 페이즈 변경, 특별한 조건부 공격 패턴 등
    }
    
    /// <summary>
    /// Omega 보스만의 고유한 특수공격 선택 로직을 구현합니다.
    /// 체력에 따라 공격 패턴을 변경할 수 있습니다.
    /// </summary>
    protected override SpecialAttackBase SelectAttackByPriority(System.Collections.Generic.List<SpecialAttackBase> availableAttacks)
    {
        if (availableAttacks.Count == 0) return null;
        
        // 체력 비율에 따른 공격 패턴 조정 (선택적)
        float healthPercentage = health / maxHealth;
        
        if (healthPercentage < 0.3f)
        {
            // 체력이 30% 이하일 때는 더 공격적인 패턴 우선 선택
            var aggressiveAttacks = availableAttacks.FindAll(attack => 
                attack.AttackData.attackName.Contains("텔레포트") || 
                attack.AttackData.attackName.Contains("Teleport"));
            
            if (aggressiveAttacks.Count > 0)
            {
                return base.SelectAttackByPriority(aggressiveAttacks);
            }
        }
        
        // 기본 우선순위 기반 선택
        return base.SelectAttackByPriority(availableAttacks);
    }
    
    /// <summary>
    /// Omega 보스 사망 시 특별한 처리를 추가합니다.
    /// </summary>
    protected override void OnBossDeath()
    {
        if (enableDebugLogs)
        {
            Debug.Log("Omega Boss defeated!");
        }
        
        // 기본 보스 사망 처리
        base.OnBossDeath();
        
        // Omega 보스만의 추가 처리 (필요시)
        // 예: 특별한 드롭 아이템, 특수 효과 등
    }
}