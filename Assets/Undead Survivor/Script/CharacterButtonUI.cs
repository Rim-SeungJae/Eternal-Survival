using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonUI : MonoBehaviour {
    public Image portraitImage;
    public Outline highlight;
    private CharacterData data;
    private CharacterSelectManager manager;

    public void Setup(CharacterData data, CharacterSelectManager manager){
        this.data = data;
        this.manager = manager;
        portraitImage.sprite = data.portrait;
        SetSelected(false);
    }

    public void OnClick() {
        manager.OnCharacterSelected(data, this);
    }

    public void SetSelected(bool isSelected)
    // 캐릭터가 선택되었는지 여부에 따라 UI를 업데이트합니다.
    {
        highlight.enabled = isSelected;
        GetComponent<Image>().color = isSelected ? new Color(47f/255f, 155f/255f, 227f/255f, 1f) : new Color(80f/255f, 80f/255f, 80f/255f, 1f);
    }
}