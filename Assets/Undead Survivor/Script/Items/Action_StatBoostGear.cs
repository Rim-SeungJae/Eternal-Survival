using UnityEngine;

/// <summary>
/// 플레이어 또는 무기의 능력치를 강화하는 장비의 행동을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "Action_StatBoostGear", menuName = "Item Actions/Stat Boost Gear")]
public class Action_StatBoostGear : ItemAction
{
    public enum Target { Player, Weapon }
    public Target targetType;

    // 이 필드들은 ScriptableObject 에셋에서 직접 설정합니다.
    public float[] statValues; // 레벨별로 적용할 값 (예: 0.1, 0.2...)
    public StatModifierType modifierType; // Flat, Additive, Multiplicative

    public override void OnEquip(Item item)
    {
        // 장비를 처음 장착할 때, 레벨 0의 능력치를 적용합니다.
        ApplyStat(item, 0);
    }

    public override void OnLevelUp(Item item)
    {
        // 레벨업 시, 이전 레벨의 효과를 제거하고 새 레벨의 효과를 적용합니다.
        RemoveStat(item, item.level - 1); // 이전 레벨 효과 제거
        ApplyStat(item, item.level);      // 현재 레벨 효과 적용
    }

    public override void OnUpdate(Item item)
    {
        // 장비는 보통 지속적인 업데이트 로직이 필요 없습니다.
    }

    public override string GetDescription(Item item)
    {
        ItemData data = item.data;
        int level = item.level;

        // 1레벨 (level 0)일 때는 기본 설명을 포맷팅하여 반환합니다.
        if (level == 0)
        {
            return string.Format(data.itemDesc, statValues[0] * 100);
        }
        // 2레벨 이상일 때는 증가량을 계산하여 표시합니다.
        else
        {
            float diff = statValues[level] - statValues[level - 1];
            if (diff > 0)
            {
                return string.Format(data.itemDesc, $"+{diff * 100:F0}");
            }
            else
            {
                return "더 이상 강화할 수 없습니다.";
            }
        }
    }

    private void ApplyStat(Item item, int level)
    {
        float value = (statValues.Length > level) ? statValues[level] : statValues[0];
        StatModifier modifier = new StatModifier(value, modifierType, this);

        switch (targetType)
        {
            case Target.Player:
                // 예시: 플레이어 속도 증가 (추후 다른 스탯도 추가 가능)
                GameManager.instance.player.speed.AddModifier(modifier);
                break;
            case Target.Weapon:
                // 현재 장착된 모든 무기에 효과 적용
                foreach (var weapon in item.GetComponentsInChildren<WeaponBase>())
                {
                    // 예시: 무기 쿨타임 감소 (추후 다른 스탯도 추가 가능)
                    weapon.cooldown.AddModifier(modifier);
                }
                break;
        }
    }

    private void RemoveStat(Item item, int level)
    {
        // StatModifier의 Source를 this(ScriptableObject 자신)로 설정했기 때문에,
        // 이 Source를 기준으로 모든 모디파이어를 쉽게 제거할 수 있습니다.
        switch (targetType)
        {
            case Target.Player:
                GameManager.instance.player.speed.RemoveAllModifiersFromSource(this);
                break;
            case Target.Weapon:
                foreach (var weapon in item.GetComponentsInChildren<WeaponBase>())
                {
                    weapon.cooldown.RemoveAllModifiersFromSource(this);
                }
                break;
        }
    }
}
