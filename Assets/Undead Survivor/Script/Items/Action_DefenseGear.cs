using UnityEngine;

/// <summary>
/// 방어력 증가 장비의 행동을 정의하는 ItemAction입니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_DefenseGear", menuName = "Item Actions/Gears/Defense Gear Action")]
public class Action_DefenseGear : ItemAction
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
            // 이전 레벨의 모디파이어를 정확히 제거하기 위해, 이전 레벨의 값으로 모디파이어를 다시 생성하여 비교하는 대신
            // StatModifier의 Source를 기반으로 제거하는 것이 더 안정적입니다.
            GameManager.instance.player.defense.RemoveAllModifiersFromSource(gearData);
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
                // 피해 감소율로 표시 (방어력 100 = 50% 감소)
                return string.Format(gearData.itemDesc, $"+{diff:F0}");
            }
            else
            {
                return string.Format(gearData.itemDesc, value);
            }
        }
        return item.data.itemDesc;
    }

    /// <summary>
    /// 플레이어의 방어력에 스탯 모디파이어를 적용합니다.
    /// </summary>
    private void ApplyStatModifier(GearData gearData, int level)
    {
        float value = (gearData.statValues.Length > level) ? gearData.statValues[level] : gearData.statValues[0];
        StatModifier modifier = new StatModifier(value, gearData.modifierType, gearData); // Source를 GearData 에셋 자체로 설정
        
        GameManager.instance.player.defense.AddModifier(modifier);
    }
}
