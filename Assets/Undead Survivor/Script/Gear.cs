using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 능력치를 강화하는 장비 아이템의 로직을 관리하는 클래스입니다.
/// </summary>
public class Gear : MonoBehaviour
{
    [Tooltip("장비 아이템의 종류")]
    public ItemData.ItemType type;
    [Tooltip("장비의 효과 적용 비율 (예: 0.1은 10% 증가)")]
    public float rate;

    /// <summary>
    /// ItemData를 기반으로 장비를 초기화합니다.
    /// </summary>
    /// <param name="data">장비 정보가 담긴 ItemData</param>
    public void Init(ItemData data)
    {
        // 장비 오브젝트의 이름과 위치를 설정합니다.
        name = "Gear " + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        // 장비의 타입과 초기 효과 비율을 설정합니다.
        type = data.itemType;
        rate = data.damages[0]; // 레벨 0의 효과 수치
        ApplyGear();
    }

    /// <summary>
    /// 장비 레벨업 시 호출됩니다. 효과 비율을 업데이트합니다.
    /// </summary>
    /// <param name="rate">새로운 효과 비율</param>
    public void LevelUp(float rate)
    {
        this.rate = rate;
        ApplyGear();
    }

    /// <summary>
    /// 이 장비가 장착/레벨업될 때 호출되어, 관련된 모든 것에 효과를 적용합니다.
    /// </summary>
    void ApplyGear()
    {
        switch (type)
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
        if (type != ItemData.ItemType.Glove) return;

        switch (weapon.id)
        {
            case 0: // 근접 무기
                // Character.WeaponSpeed 대신 Player.WeaponSpeedMultiplier 사용
                float meleeSpeed = 150 * GameManager.instance.player.WeaponSpeedMultiplier;
                weapon.speed = meleeSpeed * (1f + rate);
                break;
            default: // 원거리 무기
                // Character.WeaponRate 대신 Player.WeaponRateMultiplier 사용
                float rangeSpeed = 0.5f * GameManager.instance.player.WeaponRateMultiplier;
                weapon.speed = rangeSpeed * (1f - rate);
                break;
        }
    }

    /// <summary>
    /// 플레이어의 이동 속도를 증가시킵니다.
    /// </summary>
    void SpeedUp()
    {
        // Character.Speed 대신 Player.speed (Player.speed는 이미 CharacterDataSO의 speedMultiplier가 적용된 값) 사용
        // GameManager.instance.player.speed는 이미 CharacterDataSO의 speedMultiplier가 적용된 값이므로,
        // 여기에 다시 Character.Speed를 곱할 필요가 없습니다.
        // baseSpeed는 플레이어의 기본 속도(CharacterDataSO.speedMultiplier)에 장비 효과를 적용한 값입니다.
        float baseSpeed = GameManager.instance.player.speed; // 플레이어의 현재 속도
        GameManager.instance.player.speed = baseSpeed * (1f + rate);
    }
}