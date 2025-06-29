using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterData {
    public string name;
    public string description;
    public Sprite portrait;
    public Sprite skillImage;
    public int characterId; // 고유 ID
}

public class CharacterSelectManager : MonoBehaviour
{
    public List<CharacterData> characterList;// 캐릭터 데이터 리스트. 에디터에서 수정해서 캐릭터 정보를 추가할 수 있습니다.
    public Transform gridParent;
    public GameObject characterButtonPrefab;

    public TextMeshProUGUI descriptionText;
    public Image skillImage;

    private int selectedId = -1;
    private CharacterButtonUI selectedButton;

    void Start()
    // 캐릭터 버튼을 Prefab을 활용해 생성합니다.
    {
        foreach (CharacterData data in characterList)
        {
            GameObject obj = Instantiate(characterButtonPrefab, gridParent);
            CharacterButtonUI btn = obj.GetComponent<CharacterButtonUI>();
            btn.Setup(data, this);
        }
    }

    public void OnCharacterSelected(CharacterData data, CharacterButtonUI clickedButton)
    // 캐릭터가 선택되었을 때 캐릭터의 정보를 UI에 표시하고, 이전에 선택된 버튼의 상태를 업데이트합니다.
    {
        selectedId = data.characterId;
        descriptionText.text = data.description;
        skillImage.sprite = data.skillImage;
        skillImage.preserveAspect = true;

        if (selectedButton != null)
            selectedButton.SetSelected(false);

        selectedButton = clickedButton;
        selectedButton.SetSelected(true);
    }

    public void Confirm()
    // 선택된 캐릭터 ID가 -1이 아닌 경우, GlobalData에 선택된 캐릭터 ID를 저장하고 게임 씬으로 이동합니다.
    {
        if (selectedId == -1) return;
        GlobalData.selectedCharacterId = selectedId;
        SceneManager.LoadScene(1);
    }
}