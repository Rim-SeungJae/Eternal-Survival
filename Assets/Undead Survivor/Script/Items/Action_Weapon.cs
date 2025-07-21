using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 종류의 무기(WeaponBase를 상속하는) 행동을 범용적으로 정의합니다.
/// 인스펙터에서 지정한 클래스 이름의 컴포넌트를 동적으로 추가합니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_Weapon", menuName = "Item Actions/Generic Weapon")]
public class Action_Weapon : ItemAction
{
    [Tooltip("Item 오브젝트에 추가할 WeaponBase 상속 클래스를 선택하세요.")]
    [TypeDropdown(typeof(WeaponBase))] // 이 부분이 핵심입니다.
    public SerializableSystemType weaponType;

    public override void OnEquip(Item item)
    {
        if (weaponType == null || weaponType.Type == null)
        {
            Debug.LogError("Weapon Type이 지정되지 않았습니다! ItemData 에셋을 확인해주세요.");
            return;
        }

        // 선택된 타입을 사용하여 동적으로 컴포넌트를 추가합니다.
        GameObject newWeapon = new GameObject();
        item.weapon = newWeapon.AddComponent(weaponType.Type) as WeaponBase;
        if (item.weapon != null)
        {
            item.weapon.Init(item.data);
        }
    }

    public override void OnLevelUp(Item item)
    {
        // weapon 참조가 null이 아닐 경우에만 LevelUp을 호출합니다.
        item.weapon?.LevelUp();
    }

    public override void OnUpdate(Item item)
    {
        // 각 무기 컴포넌트의 Update가 자체적으로 로직을 처리하므로 여기서는 할 일이 없습니다.
    }

    public override string GetDescription(Item item)
    {
        ItemData data = item.data;
        int level = item.level;

        // 1레벨 (level 0)일 때는 기본 설명을 포맷팅하여 반환합니다.
        if (level == 0)
        {
            return data.itemDesc;
        }
        // 2레벨 이상일 때는 증가량을 계산하여 표시합니다.
        else
        {
            List<string> parts = new List<string>();
            if(data.damages.Length> level)
            { float damageDiff = data.damages[level] - data.damages[level - 1];
            if (damageDiff > 0) parts.Add($"피해량 +{damageDiff:F0}");}

            if (data.durations.Length > level)
            { float durationDiff = data.durations[level] - data.durations[level - 1];
            if (durationDiff > 0) parts.Add($"지속 시간 +{durationDiff :F2}초");}

            if (data.projectileSpeeds.Length > level)
            { float projectileSpeedDiff = data.projectileSpeeds[level] - data.projectileSpeeds[level - 1];
            if (projectileSpeedDiff > 0) parts.Add($"투사체 속도 +{projectileSpeedDiff:F0}");}

            if (data.cooldowns.Length > level)
            { float cooldownDiff = data.cooldowns[level] - data.cooldowns[level - 1];
            if (cooldownDiff < 0) parts.Add($"쿨타임 -{-cooldownDiff:F2}초");}

            if (data.areas.Length > level)
            { float areaDiff = data.areas[level] - data.areas[level - 1];
            if (areaDiff > 0) parts.Add($"범위 +{areaDiff * 100:F0}%");}

            if (data.counts.Length > level)
            { int countDiff = data.counts[level] - data.counts[level - 1];
            if (countDiff > 0) parts.Add($"발사체 수 +{countDiff}");}

            if (parts.Count > 0)
            {
                return string.Join(", ", parts);
            }
            else
            {
                return "더 이상 강화할 수 없습니다."; // 모든 스탯이 그대로일 경우
            }
        }
    }
}
