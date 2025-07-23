using UnityEngine;

/// <summary>
/// 부활 횟수를 늘려주는 장비의 행동을 정의하는 ItemAction입니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_ReviveGear", menuName = "Item Actions/Gears/Revive Gear Action")]
public class Action_ReviveGear : ItemAction
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
            GameManager.instance.player.revive.RemoveAllModifiersFromSource(gearData);
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
                return string.Format(gearData.itemDesc, $"+{diff:F0}");
            }
            else
            {
                return string.Format(gearData.itemDesc, value);
            }
        }
        return item.data.itemDesc;
    }

    private void ApplyStatModifier(GearData gearData, int level)
    {
        float value = (gearData.statValues.Length > level) ? gearData.statValues[level] : gearData.statValues[0];
        // 부활 횟수는 고정 수치(Flat)로 더하는 것이 일반적입니다.
        StatModifier modifier = new StatModifier(value, StatModifierType.Flat, gearData);
        GameManager.instance.player.revive.AddModifier(modifier);
    }
}
