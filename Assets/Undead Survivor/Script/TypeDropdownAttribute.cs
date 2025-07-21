using System;
using UnityEngine;

/// <summary>
/// SerializableSystemType 필드를 특정 기본 타입을 상속하는 클래스 목록을 보여주는
/// 드롭다운 메뉴로 표시하도록 지정하는 어트리뷰트입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class TypeDropdownAttribute : PropertyAttribute
{
    public Type BaseType { get; }

    public TypeDropdownAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}
