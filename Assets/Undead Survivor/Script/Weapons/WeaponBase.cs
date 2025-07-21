using UnityEngine;

/// <summary>
/// 모든 무기 클래스가 상속받는 추상 부모 클래스입니다.
/// 공통적인 초기화, 레벨업, 데이터 적용 로직과 ModifiableStat 변수들을 관리합니다.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Common Stats")]
    public ItemData itemData; // 원본 데이터 참조
    public int level;

    // 모든 무기가 공통으로 사용하는 ModifiableStat 능력치
    public ModifiableStat damage;       // 피해량
    public ModifiableStat count;        // 개수 (투사체, 관통 등)
    public ModifiableStat projectileSpeed; // 투사체/회전 속도
    public ModifiableStat duration;     // 지속 시간
    public ModifiableStat attackArea;   // 공격 범위
    public ModifiableStat cooldown;     // 쿨타임 또는 발동 조건

    protected Player player; // 플레이어 참조

    // 자식 클래스에서 Awake를 재정의할 수 있도록 virtual로 선언
    public virtual void Awake()
    {
        player = GameManager.instance.player;

        // ModifiableStat 인스턴스 초기화
        damage = new ModifiableStat(0);
        count = new ModifiableStat(0);
        projectileSpeed = new ModifiableStat(0);
        duration = new ModifiableStat(0);
        attackArea = new ModifiableStat(1); // 기본값 1 (100%)
        cooldown = new ModifiableStat(0);
    }

    /// <summary>
    /// ItemData를 기반으로 무기를 처음 초기화합니다.
    /// </summary>
    public virtual void Init(ItemData data)
    {
        this.itemData = data;
        this.level = 0;

        name = "Weapon " + data.itemName;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        // 레벨 0 데이터로 능력치 적용
        ApplyLevelData();
    }

    /// <summary>
    /// 무기 레벨업 시 호출됩니다.
    /// </summary>
    public void LevelUp()
    {
        if (level < itemData.maxLevel)
        {
            this.level++;
            ApplyLevelData();
        }
    }

    /// <summary>
    /// 현재 레벨에 맞는 데이터를 ItemData에서 가져와 능력치를 갱신합니다.
    /// 배열의 길이가 레벨보다 짧으면, 인덱스 0의 값을 기본값으로 사용합니다.
    /// </summary>
    protected virtual void ApplyLevelData()
    {
        damage.BaseValue = (itemData.damages.Length > level) ? itemData.damages[level] : itemData.damages[0];
        count.BaseValue = (itemData.counts.Length > level) ? itemData.counts[level] : itemData.counts[0];
        projectileSpeed.BaseValue = (itemData.projectileSpeeds.Length > level) ? itemData.projectileSpeeds[level] : itemData.projectileSpeeds[0];
        duration.BaseValue = (itemData.durations.Length > level) ? itemData.durations[level] : itemData.durations[0];
        attackArea.BaseValue = (itemData.areas.Length > level) ? itemData.areas[level] : itemData.areas[0];
        cooldown.BaseValue = (itemData.cooldowns.Length > level) ? itemData.cooldowns[level] : itemData.cooldowns[0];
    }
}
