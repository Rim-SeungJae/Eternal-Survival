using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [TypeDropdown] 어트리뷰트가 붙은 SerializableSystemType 필드를 위한 커스텀 프로퍼티 드로어입니다.
/// </summary>
[CustomPropertyDrawer(typeof(TypeDropdownAttribute))]
public class SerializableSystemTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        TypeDropdownAttribute dropdownAttribute = (TypeDropdownAttribute)attribute;
        SerializedProperty typeNameProperty = property.FindPropertyRelative("assemblyQualifiedName");

        // 모든 어셈블리에서 지정된 기본 타입을 상속하는 모든 구체적인 클래스를 찾습니다.
        var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => dropdownAttribute.BaseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToList();

        // 표시할 이름 목록과 실제 타입 이름 목록을 만듭니다.
        List<string> displayNames = new List<string> { "(None)" };
        displayNames.AddRange(derivedTypes.Select(t => t.Name));

        List<string> qualifiedNames = new List<string> { string.Empty };
        qualifiedNames.AddRange(derivedTypes.Select(t => t.AssemblyQualifiedName));

        EditorGUI.BeginProperty(position, label, property);

        int currentIndex = qualifiedNames.IndexOf(typeNameProperty.stringValue);
        if (currentIndex < 0) currentIndex = 0; // 찾지 못하면 (None)으로 설정

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames.ToArray());

        if (newIndex != currentIndex)
        {
            typeNameProperty.stringValue = qualifiedNames[newIndex];
        }

        EditorGUI.EndProperty();
    }
}
