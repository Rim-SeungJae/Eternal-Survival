using UnityEngine;

/// <summary>
/// LightningWeapon에 필요한 추가 데이터를 정의하는 ScriptableObject 클래스입니다.
/// WeaponData를 상속받아 고유한 능력치를 추가합니다.
/// </summary>
[CreateAssetMenu(fileName = "LightningWeaponData", menuName = "Scriptable Objects/Weapon Data/Lightning Weapon Data")]
public class LightningWeaponData : WeaponData
{
    [Header("Lightning Weapon Specific Stats")]
    [Tooltip("공격 대상 레이어")]
    public LayerMask targetLayer; // 공격 대상 레이어
    [Tooltip("번개의 공격 사거리")]
    public float lightningRange; // 번개 공격 사거리
    [Header("# Lightning Specific Stats")]
    [Tooltip("추가 피해가 적용되기 시작하는 플레이어로부터의 거리")]
    public float bonusDistanceThreshold = 5f;

    [Tooltip("거리 밖에 있는 적에게 적용될 추가 피해 배율 (예: 0.5는 50% 추가 피해)")]
    public float bonusDamageMultiplier = 0.5f;
}
