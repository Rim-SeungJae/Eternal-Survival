using UnityEngine;

/// <summary>
/// 장비 아이템의 데이터를 정의하는 ScriptableObject 클래스입니다.
/// ItemData를 상속받아 장비 전용 데이터를 추가로 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "GearData", menuName = "Scriptable Objects/Gear Data")]
public class GearData : ItemData
{
    [Header("# Gear Stats")]
    [Tooltip("레벨별로 적용할 스탯 값 (예: 0.1은 10%)")]
    public float[] statValues;
    [Tooltip("스탯을 적용할 방식 (Flat, Additive, Multiplicative)")]
    public StatModifierType modifierType;
}
