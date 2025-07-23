using UnityEngine;

/// <summary>
/// 쿨다운 감소 장비(Cooldown Reduction Gear)의 행동을 정의하는 ItemAction입니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_CooldownReductionGear", menuName = "Item Actions/Gears/Cooldown Reduction Gear Action")]
public class Action_CooldownReductionGear : ItemAction
{
    public override void OnEquip(Item item)
    {
        if (item.data is GearData gearData)
        {
            ApplyToAllWeapons(gearData, 0);
        }
    }

    public override void OnLevelUp(Item item)
    {
        if (item.data is GearData gearData)
        {
            RemoveFromAllWeapons(gearData);
            ApplyToAllWeapons(gearData, item.level);
        }
    }

    public override string GetDescription(Item item)
    {
        if (item.data is GearData gearData)
        {
            int level = item.level;
            float value = (gearData.statValues.Length > level) ? gearData.statValues[level] : gearData.statValues[0];

            if (level > 0)
            {
                float prevValue = (gearData.statValues.Length > level - 1) ? gearData.statValues[level - 1] : gearData.statValues[0];
                float diff = value - prevValue;
                return string.Format(gearData.itemDesc, $"+{diff * 100:F0}%");
            }
            else
            {
                return string.Format(gearData.itemDesc, $"{value * 100:F0}%");
            }
        }
        return item.data.itemDesc;
    }

    private void ApplyToAllWeapons(GearData gearData, int level)
    {
        float value = (gearData.statValues.Length > level) ? gearData.statValues[level] : gearData.statValues[0];
        // 쿨다운 감소는 음수 값으로 적용해야 합니다.
        StatModifier modifier = new StatModifier(-value, gearData.modifierType, gearData);

        foreach (var weaponItem in GameManager.instance.player.items)
        {
            if (weaponItem.weapon != null)
            {
                weaponItem.weapon.cooldown.AddModifier(modifier);
            }
        }
    }

    private void RemoveFromAllWeapons(GearData gearData)
    {
        foreach (var weaponItem in GameManager.instance.player.items)
        {
            if (weaponItem.weapon != null)
            {
                weaponItem.weapon.cooldown.RemoveAllModifiersFromSource(gearData);
            }
        }
    }
}
