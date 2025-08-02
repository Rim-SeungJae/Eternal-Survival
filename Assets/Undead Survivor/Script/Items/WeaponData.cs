using UnityEngine;

/// <summary>
/// 무기 아이템의 데이터를 정의하는 ScriptableObject 클래스입니다.
/// ItemData를 상속받아 무기 전용 데이터를 추가로 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/Weapon Data/Ordinary Weapon")]
public class WeaponData : ItemData
{
    [Header("# Weapon Stats")]
    [Tooltip("레벨별 피해량")]
    public float[] damages;
    [Tooltip("레벨별 투사체 속도 또는 회전 속도")]
    public float[] projectileSpeeds;
    [Tooltip("레벨별 지속 시간 (음수일 경우 무한)")]
    public float[] durations;
    [Tooltip("레벨별 공격 범위")]
    public float[] areas;
    [Tooltip("레벨별 쿨타임 또는 발동 조건")]
    public float[] cooldowns;
    [Tooltip("레벨별 투사체 개수 또는 관통 횟수")]
    public int[] counts;

    [Header("# Weapon Visuals")]
    [Tooltip("PoolManager에 등록된 발사체 또는 이펙트의 태그")]
    [PoolTagSelector]
    public string projectileTag;
}
