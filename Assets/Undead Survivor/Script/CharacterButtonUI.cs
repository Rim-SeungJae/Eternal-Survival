using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 선택 화면의 각 캐릭터 버튼 UI를 관리하는 클래스입니다.
/// </summary>
public class CharacterButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("캐릭터 초상화를 표시할 이미지")]
    public Image portraitImage;
    [Tooltip("선택되었을 때 표시될 하이라이트 효과")]
    public Outline highlight;

    private CharacterDataSO data; // CharacterDataSO로 타입 변경
    private CharacterSelectManager manager;

    /// <summary>
    /// 버튼을 초기화하고 필요한 데이터를 설정합니다.
    /// </summary>
    /// <param name="data">이 버튼에 해당하는 CharacterDataSO</param>
    /// <param name="manager">캐릭터 선택 매니저</param>
    public void Setup(CharacterDataSO data, CharacterSelectManager manager) // CharacterDataSO로 타입 변경
    {
        this.data = data;
        this.manager = manager;
        portraitImage.sprite = data.portrait;
        // 초기에는 선택되지 않은 상태로 설정합니다.
        SetSelected(false);
    }

    /// <summary>
    /// 버튼이 클릭되었을 때 호출됩니다. (Button 컴포넌트의 OnClick 이벤트에 연결)
    /// </summary>
    public void OnClick()
    {
        // 매니저에게 이 버튼이 선택되었음을 알립니다.
        manager.OnCharacterSelected(data, this);
    }

    /// <summary>
    /// 버튼의 선택 상태에 따라 UI를 업데이트합니다.
    /// </summary>
    /// <param name="isSelected">선택되었으면 true, 아니면 false</param>
    public void SetSelected(bool isSelected)
    {
        // 하이라이트 효과를 켜거나 끕니다.
        highlight.enabled = isSelected;
        // 선택 상태에 따라 버튼의 배경색을 변경합니다.
        GetComponent<Image>().color = isSelected ? new Color(47f / 255f, 155f / 255f, 227f / 255f, 1f) : new Color(80f / 255f, 80f / 255f, 80f / 255f, 1f);
    }
}