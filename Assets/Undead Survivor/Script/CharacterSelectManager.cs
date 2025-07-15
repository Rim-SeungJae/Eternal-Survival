using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// 기존 CharacterData 클래스는 더 이상 사용되지 않습니다.
// ScriptableObject 기반의 CharacterDataSO로 대체되었습니다.
// [System.Serializable]
// public class CharacterData {
//     public string name;
//     public string description;
//     public Sprite portrait;
//     public Sprite skillImage;
//     public int characterId; // 고유 ID
// }

/// <summary>
/// 캐릭터 선택 화면의 UI와 로직을 관리하는 클래스입니다.
/// ScriptableObject 기반의 캐릭터 데이터를 사용하여 유연성을 높였습니다.
/// </summary>
public class CharacterSelectManager : MonoBehaviour
{
    [Tooltip("인스펙터에서 설정할 캐릭터 데이터 리스트 (ScriptableObject 참조)")]
    public List<CharacterDataSO> characterList; // CharacterDataSO 타입으로 변경
    public Transform gridParent;
    public GameObject characterButtonPrefab;

    public TextMeshProUGUI descriptionText;
    public Image skillImage;

    private CharacterDataSO selectedCharacterData; // 현재 선택된 CharacterDataSO 참조
    private CharacterButtonUI selectedButton;

    void Start()
    {
        // characterList에 있는 모든 CharacterDataSO에 대해 버튼을 생성합니다.
        foreach (CharacterDataSO data in characterList)
        {
            GameObject obj = Instantiate(characterButtonPrefab, gridParent);
            CharacterButtonUI btn = obj.GetComponent<CharacterButtonUI>();
            btn.Setup(data, this); // CharacterDataSO를 CharacterButtonUI에 전달
        }
    }

    /// <summary>
    /// 캐릭터 버튼이 클릭되었을 때 호출되는 함수입니다.
    /// 선택된 캐릭터의 정보를 UI에 표시하고, 버튼의 선택 상태를 업데이트합니다.
    /// </summary>
    /// <param name="data">클릭된 버튼의 CharacterDataSO</param>
    /// <param name="clickedButton">클릭된 버튼의 UI 스크립트</param>
    public void OnCharacterSelected(CharacterDataSO data, CharacterButtonUI clickedButton)
    {
        selectedCharacterData = data; // 선택된 CharacterDataSO 저장
        descriptionText.text = data.description;
        skillImage.sprite = data.skillImage;
        skillImage.preserveAspect = true;
        skillImage.color = new Color(1f, 1f, 1f, 1f); // 이미지가 투명하지 않도록 설정

        if (selectedButton != null)
            selectedButton.SetSelected(false);

        selectedButton = clickedButton;
        selectedButton.SetSelected(true);
    }

    /// <summary>
    /// '확인' 버튼 클릭 시 호출됩니다.
    /// 선택된 CharacterDataSO를 GlobalData에 저장하고 게임 씬으로 이동합니다.
    /// </summary>
    public void Confirm()
    {
        if (selectedCharacterData == null) return; // 선택된 캐릭터가 없으면 아무것도 하지 않음

        GlobalData.selectedCharacterDataSO = selectedCharacterData; // 선택된 CharacterDataSO를 GlobalData에 저장
        SceneManager.LoadScene(1); // 게임 씬으로 전환
    }
}