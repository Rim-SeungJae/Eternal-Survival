using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// 수정 가능한 능력치를 나타내는 클래스입니다.
/// 기본값과 적용된 모든 StatModifier들을 관리하며, 최종 유효 능력치를 계산합니다.
/// </summary>
[Serializable]
public class ModifiableStat
{
    private float _baseValue; // 기본 능력치
    private readonly List<StatModifier> _modifiers; // 적용된 수정자 목록
    private bool _isDirty = true; // 최종 값이 변경되었는지 여부 (캐싱을 위함)
    private float _lastCalculatedValue; // 마지막으로 계산된 최종 값

    /// <summary>
    /// 능력치 값이 변경될 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action OnValueChange;

    /// <summary>
    /// 기본 능력치입니다. 설정 시 최종 값이 다시 계산됩니다.
    /// </summary>
    public float BaseValue
    {
        get { return _baseValue; }
        set
        {
            if (_baseValue != value)
            {
                _baseValue = value;
                _isDirty = true; // 기본값이 변경되면 최종 값도 다시 계산해야 함
                OnValueChange?.Invoke();
            }
        }
    }

    /// <summary>
    /// 적용된 수정자들의 읽기 전용 컬렉션입니다.
    /// </summary>
    public ReadOnlyCollection<StatModifier> Modifiers;

    /// <summary>
    /// ModifiableStat의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="baseValue">초기 기본 능력치</param>
    public ModifiableStat(float baseValue)
    {
        _baseValue = baseValue;
        _modifiers = new List<StatModifier>();
        Modifiers = _modifiers.AsReadOnly(); // 외부에서 수정 불가능하도록 ReadOnlyCollection으로 제공
    }

    /// <summary>
    /// 수정자를 추가합니다. 추가 후 최종 값이 다시 계산됩니다.
    /// </summary>
    /// <param name="mod">추가할 StatModifier</param>
    public void AddModifier(StatModifier mod)
    {
        _modifiers.Add(mod);
        _modifiers.Sort(CompareModifierOrder); // 수정자 타입에 따라 정렬
        _isDirty = true;
        OnValueChange?.Invoke();
    }

    /// <summary>
    /// 특정 출처(Source)에 의해 적용된 모든 수정자를 제거합니다.
    /// </summary>
    /// <param name="source">수정자를 적용한 출처</param>
    /// <returns>제거된 수정자의 수</returns>
    public int RemoveAllModifiersFromSource(object source)
    {
        int numRemoved = _modifiers.RemoveAll(mod => mod.Source == source);
        if (numRemoved > 0)
        {
            _isDirty = true;
            OnValueChange?.Invoke();
        }
        return numRemoved;
    }

    /// <summary>
    /// 최종 유효 능력치를 계산하여 반환합니다.
    /// 값이 변경되지 않았다면 캐시된 값을 반환합니다.
    /// </summary>
    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _lastCalculatedValue = CalculateFinalValue();
                _isDirty = false;
            }
            return _lastCalculatedValue;
        }
    }

    /// <summary>
    /// 모든 수정자를 적용하여 최종 능력치를 계산합니다.
    /// </summary>
    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0; // Additive 타입 수정자들의 합

        foreach (StatModifier mod in _modifiers)
        {
            if (mod.Type == StatModifierType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModifierType.Additive)
            {
                sumPercentAdd += mod.Value; // 비율은 일단 합산
            }
            else if (mod.Type == StatModifierType.Multiplicative)
            {
                // Additive 타입이 먼저 적용된 후 Multiplicative가 적용됩니다.
                finalValue *= (1 + sumPercentAdd); // 합산된 비율 적용
                sumPercentAdd = 0; // 적용 후 초기화
                finalValue *= (1 + mod.Value); // Multiplicative 비율 적용
            }
        }

        // 마지막으로 남은 Additive 비율이 있다면 적용
        finalValue *= (1 + sumPercentAdd);

        // 소수점 처리 (선택 사항: 필요에 따라 반올림/내림 등)
        // finalValue = (float)Math.Round(finalValue, 4); // 예시: 소수점 4자리까지 반올림

        return finalValue;
    }

    /// <summary>
    /// 수정자 정렬 순서를 정의합니다.
    /// Flat -> Additive -> Multiplicative 순으로 적용되도록 합니다.
    /// </summary>
    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Type < b.Type) return -1;
        if (a.Type > b.Type) return 1;
        return 0; // 타입이 같으면 순서 유지
    }
}
