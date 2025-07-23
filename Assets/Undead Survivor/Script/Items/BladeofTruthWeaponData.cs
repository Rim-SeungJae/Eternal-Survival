using UnityEngine;

/// <summary>
/// LightningWeapon에 필요한 추가 데이터를 정의하는 ScriptableObject 클래스입니다.
/// WeaponData를 상속받아 고유한 능력치를 추가합니다.
/// </summary>
[CreateAssetMenu(fileName = "BladeofTruthWeaponData", menuName = "Scriptable Objects/Weapon Data/BladeofTruth Weapon Data")]
public class BladeofTruthWeaponData : WeaponData
{
    [Header("Blade of Truth Specific Stats")]
    [Tooltip("공격 대상 레이어")]
    public LayerMask targetLayer; // 공격 대상 레이어
    [Tooltip("데미지 적용 딜레이")]
    public float damageDelay = 0.5f; // 공격 딜레이 시간
    [Header("Bonus Haste Settings")]
    [Tooltip("기본 적중 시 이동 속도 증가량")]
    public float bonusHasteAmount = 0.2f; // 기본 이동 속도 증가량
    [Tooltip("이동 속도 증가가 적용되는 시간")]
    public float bonusHasteDuration = 1f; // 이동 속도 증가가 적용되는 시간
    [Tooltip("추가 피격 대상 당 이동속도 증가량")]
    public float additionalHastePerHit = 0.05f; // 추가 피격 대상 당 이동속도 증가량
    [Tooltip("최대 이동 속도 증가량")]
    public float maxHasteAmount = 0.4f; // 최대 이동 속도 증가량
}
