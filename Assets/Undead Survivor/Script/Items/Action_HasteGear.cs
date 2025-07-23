using UnityEngine;

/// <summary>
/// 이동 속도 증가 장비(Haste Gear)의 행동을 정의하는 ItemAction입니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_HasteGear", menuName = "Item Actions/Gears/Haste Gear Action")]
public class Action_HasteGear : ItemAction
{
    public override void OnEquip(Item item)
    {
        if (item.data is GearData gearData)
        {
            ApplyStatModifier(gearData, 0);
        }
    }

    public override void OnLevelUp(Item item)
    {
        if (item.data is GearData gearData)
        {
            GameManager.instance.player.speed.RemoveAllModifiersFromSource(gearData);
            ApplyStatModifier(gearData, item.level);
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

    private void ApplyStatModifier(GearData gearData, int level)
    {
        float value = (gearData.statValues.Length > level) ? gearData.statValues[level] : gearData.statValues[0];
        StatModifier modifier = new StatModifier(value, gearData.modifierType, gearData);
        GameManager.instance.player.speed.AddModifier(modifier);
    }
}
