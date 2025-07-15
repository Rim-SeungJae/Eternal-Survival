using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// [PoolTagSelector] 어트리뷰트가 붙은 string 필드를
/// PoolManager에 등록된 태그 목록을 보여주는 드롭다운 메뉴로 그려주는 에디터 스크립트입니다.
/// </summary>
[CustomPropertyDrawer(typeof(PoolTagSelectorAttribute))]
public class PoolTagSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 이 드로어가 string 타입에만 적용되도록 보장합니다.
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 씬에서 PoolManager 인스턴스를 찾습니다.
            PoolManager poolManager = Object.FindFirstObjectByType<PoolManager>();

            if (poolManager == null)
            {
                // PoolManager가 없으면 일반 텍스트 필드를 표시하고 경고 메시지를 보여줍니다.
                EditorGUI.HelpBox(position, "Scene에 PoolManager가 없습니다.", MessageType.Warning);
            }
            else
            {
                // PoolManager의 pools 리스트에서 태그 목록을 추출합니다.
                List<string> tags = new List<string>();
                if (poolManager.pools != null)
                {
                    foreach (var pool in poolManager.pools)
                    {
                        if (!string.IsNullOrEmpty(pool.tag))
                        {
                            tags.Add(pool.tag);
                        }
                    }
                }

                if (tags.Count > 0)
                {
                    // 현재 프로퍼티(string 변수)의 값을 기반으로 현재 선택된 인덱스를 찾습니다.
                    int currentIndex = Mathf.Max(0, tags.IndexOf(property.stringValue));

                    // 드롭다운 메뉴(Popup)를 그립니다.
                    int newIndex = EditorGUI.Popup(position, label.text, currentIndex, tags.ToArray());

                    // 사용자가 드롭다운에서 새로운 항목을 선택했다면, 프로퍼티의 값을 업데이트합니다.
                    if (newIndex != currentIndex)
                    {
                        property.stringValue = tags[newIndex];
                    }
                    // 드롭다운 메뉴를 그릴 때 현재 값을 보장하기 위해 추가
                    else
                    {
                        property.stringValue = tags[newIndex];
                    }
                }
                else
                {
                    // PoolManager에 등록된 태그가 없으면 경고 메시지를 표시합니다.
                    EditorGUI.HelpBox(position, "PoolManager에 등록된 태그가 없습니다.", MessageType.Info);
                }
            }

            EditorGUI.EndProperty();
        }
        else
        {
            // string이 아닌 다른 타입에 어트리뷰트를 사용한 경우 에러 메시지를 표시합니다.
            EditorGUI.LabelField(position, label.text, "Error: PoolTagSelector는 string 타입에만 사용할 수 있습니다.");
        }
    }
}
