using UnityEngine;

/// <summary>
/// 아이템의 행동 로직을 정의하는 ScriptableObject 기반의 추상 클래스입니다.
/// 전략 패턴(Strategy Pattern)을 사용하여 아이템의 행동을 데이터로 분리합니다.
/// </summary>
public abstract class ItemAction : ScriptableObject
{
    /// <summary>
    /// 아이템이 처음 장착될 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="item">이 액션을 소유한 Item 컴포넌트</param>
    public abstract void OnEquip(Item item);

    /// <summary>
    /// 아이템이 레벨업할 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="item">이 액션을 소유한 Item 컴포넌트</param>
    public abstract void OnLevelUp(Item item);

    /// <summary>
    /// 매 프레임 호출되어 지속적인 로직을 처리합니다. (필요 없는 경우 비워둘 수 있음)
    /// </summary>
    /// <param name="item">이 액션을 소유한 Item 컴포넌트</param>
    public abstract void OnUpdate(Item item);

    /// <summary>
    /// 레벨업 UI에 표시될 아이템 설명을 생성합니다.
    /// 자식 클래스에서 재정의하여 동적인 설명을 제공할 수 있습니다.
    /// </summary>
    /// <param name="item">이 액션을 소유한 Item 컴포넌트</param>
    /// <returns>UI에 표시될 최종 설명 문자열</returns>
    public virtual string GetDescription(Item item)
    {
        // 기본적으로 ItemData에 있는 설명을 그대로 반환합니다.
        return item.data.itemDesc;
    }
}
