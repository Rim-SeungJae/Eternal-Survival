using UnityEngine;

/// <summary>
/// 'Ghost Light' 무기에 필요한 추가 데이터를 정의하는 ScriptableObject 클래스입니다.
/// WeaponData를 상속받아 장판 관련 능력치를 추가합니다.
/// </summary>
[CreateAssetMenu(fileName = "GhostLightData", menuName = "Scriptable Objects/Weapon Data/Ghost Light Data")]
public class GhostLightData : WeaponData
{
    [Header("# Ghost Light Specific Stats")]
    [Tooltip("투사체가 폭발한 자리에 생성되는 화염 장판의 지속 시간(초)")]
    public float groundEffectDuration = 3f;

    [Tooltip("화염 장판이 초당 입히는 피해량")]
    public float groundEffectDamagePerSecond = 5f;

    [Tooltip("화염 장판의 지속 피해가 적용되는 간격(초). 낮을수록 자주 피해를 줍니다.")]
    public float groundEffectTickRate = 0.5f;
    [Tooltip("도깨비불의 발사 사거리")]
    public float fireRange = 10f;
}