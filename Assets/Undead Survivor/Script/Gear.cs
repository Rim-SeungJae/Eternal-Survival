using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 능력치를 강화하는 장비 아이템의 로직을 관리하는 클래스입니다.
/// ItemData에 정의된 속성을 기반으로 플레이어와 무기에 효과를 적용합니다.
/// </summary>
public class Gear : MonoBehaviour
{
    public ItemData itemData; // 원본 데이터 참조
    public int level; // 현재 장비 레벨

    // 현재 레벨에 따라 적용될 최종 효과 비율
    public float rate;

    /// <summary>
    /// ItemData를 기반으로 장비를 처음 초기화합니다.
    /// </summary>
    public void Init(ItemData data)
    {
        this.itemData = data;
        this.level = 0;

        name = "Gear " + data.itemName;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        // 레벨 0 데이터로 능력치 적용
        ApplyLevelData();
    }

    /// <summary>
    /// 장비 레벨업 시 호출됩니다.
    /// </summary>
    public void LevelUp()
    {
        this.level++;
        ApplyLevelData();
    }

    /// <summary>
    /// 현재 레벨에 맞는 데이터를 ItemData에서 가져와 능력치를 갱신합니다.
    /// </summary>
    private void ApplyLevelData()
    {
        // 레벨별 데이터가 없으면 기본값을, 있으면 해당 레벨의 값을 대입합니다.
        // 장비의 경우 damages 배열을 효과 비율로 사용합니다.
        rate = (itemData.damages.Length > level) ? itemData.damages[level] : itemData.damages[0];

        // 능력치 적용
        ApplyGear();
    }

    /// <summary>
    /// 이 장비가 장착/레벨업될 때 호출되어, 관련된 모든 것에 효과를 적용합니다.
    /// </summary>
    void ApplyGear()
    {
        // 이전에 적용했던 모든 모디파이어를 제거하여 중복 적용을 방지합니다.
        RemoveAllModifiers();

        switch (itemData.itemType)
        {
            case ItemData.ItemType.Glove:
                // 이 장비가 장착될 때, 플레이어의 모든 무기에 효과를 적용합니다.
                Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
                foreach (Weapon weapon in weapons)
                {
                    ApplyGearEffectTo(weapon);
                }
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
        }
    }

    /// <summary>
    /// 특정 무기 하나에 이 장비의 효과를 적용합니다.
    /// </summary>
    /// <param name="weapon">효과를 적용할 무기</param>
    public void ApplyGearEffectTo(Weapon weapon)
    {
        // 이 장비가 장갑이 아니면 무기에 영향을 주지 않습니다.
        if (itemData.itemType != ItemData.ItemType.Glove) return;

        // 장갑은 무기의 공격 속도(projectileSpeed)와 쿨타임(cooldown)에 영향을 줍니다.
        // rate는 비율이므로 StatModifierType.Additive를 사용합니다.
        weapon.projectileSpeed.AddModifier(new StatModifier(rate, StatModifierType.Additive, this));
        weapon.cooldown.AddModifier(new StatModifier(-rate, StatModifierType.Additive, this)); // 쿨타임은 감소이므로 음수 적용
    }

    /// <summary>
    /// 플레이어의 이동 속도를 증가시킵니다.
    /// </summary>
    void SpeedUp()
    {
        // 신발은 플레이어의 이동 속도에 영향을 줍니다.
        // rate는 비율이므로 StatModifierType.Additive를 사용합니다.
        if (GameManager.instance.player != null)
        {
            GameManager.instance.player.speed.AddModifier(new StatModifier(rate, StatModifierType.Additive, this));
        }
    }

    /// <summary>
    /// 이 장비가 비활성화될 때 적용했던 모든 모디파이어를 제거합니다.
    /// </summary>
    void OnDisable()
    {
        RemoveAllModifiers();
    }

    /// <summary>
    /// 이 장비가 적용했던 모든 모디파이어를 제거합니다.
    /// </summary>
    private void RemoveAllModifiers()
    {
        // 무기에 적용된 모디파이어 제거
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            weapon.projectileSpeed.RemoveAllModifiersFromSource(this);
            weapon.cooldown.RemoveAllModifiersFromSource(this);
        }

        // 플레이어에 적용된 모디파이어 제거
        if (GameManager.instance.player != null)
        {
            GameManager.instance.player.speed.RemoveAllModifiersFromSource(this);
        }
    }
}

