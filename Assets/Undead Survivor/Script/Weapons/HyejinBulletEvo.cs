using UnityEngine;
using System.Collections;

/// <summary>
/// 진화된 Hyejin Weapon의 투사체를 관리하는 클래스입니다.
/// 기본 Bullet 클래스를 상속받아 기본 투사체 로직을 재사용하고,
/// Three Calamities 스택 시스템을 추가합니다.
/// </summary>
public class HyejinBulletEvo : Bullet
{
    private HyejinWeaponEvo weaponRef;
    
    [Header("# Three Calamities")]
    [Tooltip("Three Calamities 스택이 발동되는 횟수")]
    public int calamitiesStackThreshold = 3;
    
    [Tooltip("스택 지속 시간")]
    public float stackDuration = 5f;

    [Tooltip("삼매 스택 효과 태그")]
    [PoolTagSelector]
    public string calamitiesStackEffectTag;

    /// <summary>
    /// 진화 무기 참조를 설정합니다.
    /// </summary>
    /// <param name="weapon">진화된 Hyejin 무기</param>
    public void SetEvolutionWeapon(HyejinWeaponEvo weapon)
    {
        weaponRef = weapon;
    }

    /// <summary>
    /// 적과 충돌했을 때 Three Calamities 스택을 추가합니다.
    /// </summary>
    protected override void OnEnemyHit(Enemy enemy)
    {
        // 기본 데미지 적용
        base.OnEnemyHit(enemy);
        
        // Three Calamities 스택 추가
        if (weaponRef != null)
        {
            AddCalamitiesStack(enemy);
        }
    }

    /// <summary>
    /// 적에게 Three Calamities 스택을 추가합니다.
    /// </summary>
    /// <param name="enemy">스택을 추가할 적</param>
    private void AddCalamitiesStack(Enemy enemy)
    {
        // 적에게 Three Calamities 컴포넌트가 있는지 확인
        ThreeCalamitiesStack calamitiesComponent = enemy.GetComponent<ThreeCalamitiesStack>();
        if (calamitiesComponent == null)
        {
            calamitiesComponent = enemy.gameObject.AddComponent<ThreeCalamitiesStack>();
            calamitiesComponent.Init(calamitiesStackEffectTag);
        }
        
        // 스택 추가
        calamitiesComponent.AddStack(weaponRef, stackDuration);
    }
} 