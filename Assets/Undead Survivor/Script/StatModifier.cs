using System;

/// <summary>
/// 능력치 수정자의 타입을 정의합니다.
/// </summary>
public enum StatModifierType
{
    Flat = 0,       // 기본값에 직접 더하는 방식 (예: +5 데미지)
    Additive = 1,   // 기본값에 비율을 더하는 방식 (예: +10% 데미지)
    Multiplicative = 2 // 최종값에 비율을 곱하는 방식 (예: x1.5 데미지)
}

/// <summary>
/// 능력치에 적용될 수정자 정보를 담는 구조체입니다.
/// </summary>
public struct StatModifier
{
    public readonly float Value;            // 수정 값
    public readonly StatModifierType Type;  // 수정 타입
    public readonly object Source;          // 누가 이 수정자를 적용했는지 (예: Gear 인스턴스, Buff 인스턴스)

    /// <summary>
    /// 새로운 능력치 수정자를 생성합니다.
    /// </summary>
    /// <param name="value">수정 값</param>
    /// <param name="type">수정 타입</param>
    /// <param name="source">수정자를 적용한 출처 (선택 사항)</param>
    public StatModifier(float value, StatModifierType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}
