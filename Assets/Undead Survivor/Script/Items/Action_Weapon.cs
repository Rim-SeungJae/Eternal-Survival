using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 무기 아이템의 행동을 정의합니다.
/// WeaponData로부터 로직 타입을 받아 동적으로 WeaponBase 컴포넌트를 생성합니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_Weapon", menuName = "Item Actions/Weapon Action")]
public class Action_Weapon : ItemAction
{
    [Header("# Weapon Logic")]
    [Tooltip("이 무기가 사용할 로직 클래스를 선택하세요.")]
    [TypeDropdown(typeof(WeaponBase))] // WeaponBase를 상속하는 클래스만 선택 가능
    public SerializableSystemType weaponLogicType;

    public override void OnEquip(Item item)
    {
        if (item.data is WeaponData weaponData)
        {
            if (weaponLogicType == null || weaponLogicType.Type == null)
            {
                Debug.LogError($"WeaponData '{weaponData.name}'에 weaponLogicType이 지정되지 않았습니다!");
                return;
            }

            GameObject newWeapon = new GameObject();
            item.weapon = newWeapon.AddComponent(weaponLogicType.Type) as WeaponBase;
            if (item.weapon != null)
            {
                item.weapon.Init(weaponData);
            }
        }
    }

    public override void OnLevelUp(Item item)
    {
        item.weapon?.LevelUp();
    }

    public override string GetDescription(Item item)
    {
        if (item.data is WeaponData weaponData)
        {
            int level = item.level;
            if (level == 0)
            {
                return weaponData.itemDesc;
            }
            else
            {
                List<string> parts = new List<string>();
                if (weaponData.damages.Length > level) { float diff = weaponData.damages[level] - weaponData.damages[level - 1]; if (diff != 0) parts.Add($"피해량 {(diff > 0 ? "+" : "")}{diff:F0}"); }
                if (weaponData.counts.Length > level) { int diff = weaponData.counts[level] - weaponData.counts[level - 1]; if (diff != 0) parts.Add($"개수 {(diff > 0 ? "+" : "")}{diff}"); }
                if (weaponData.cooldowns.Length > level) { float diff = weaponData.cooldowns[level] - weaponData.cooldowns[level - 1]; if (diff != 0) parts.Add($"쿨타임 {(diff > 0 ? "+" : "")}{diff:F2}초"); }
                if (weaponData.projectileSpeeds.Length > level) { float diff = weaponData.projectileSpeeds[level] - weaponData.projectileSpeeds[level - 1]; if (diff != 0) parts.Add($"속도 {(diff > 0 ? "+" : "")}{diff:F0}"); }
                if (weaponData.durations.Length > level) { float diff = weaponData.durations[level] - weaponData.durations[level - 1]; if (diff != 0) parts.Add($"지속시간 {(diff > 0 ? "+" : "")}{diff:F2}초"); }
                if (weaponData.areas.Length > level) { float diff = weaponData.areas[level] - weaponData.areas[level - 1]; if (diff != 0) parts.Add($"범위 {(diff > 0 ? "+" : "")}{diff:F2}m"); }

                return parts.Count > 0 ? string.Join(", ", parts) : "더 이상 강화할 수 없습니다.";
            }
        }
        return item.data.itemDesc;
    }
}