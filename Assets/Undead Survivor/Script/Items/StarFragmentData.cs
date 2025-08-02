using UnityEngine;

/// <summary>
/// Star Fragment 무기의 특수 데이터를 정의하는 ScriptableObject 클래스입니다.
/// WeaponData를 상속받아 Star Fragment 전용 데이터를 추가로 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "StarFragmentData", menuName = "Scriptable Objects/Weapon Data/Star Fragment")]
public class StarFragmentData : WeaponData
{
    [Header("# Star Fragment 특수 설정")]
    [Tooltip("메테오 경고 Projectile Tag")]
    [PoolTagSelector]
    public string warningProjectileTag;

    [Tooltip("메테오 충돌 예고 시간")]
    public float[] warningDurations;
    
    [Tooltip("메테오 사정거리")]
    public float attackRange = 8f;

} 