using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 UI에 표시되는 개별 아이템의 동작을 관리하는 클래스입니다.
/// </summary>
public class Item : MonoBehaviour
{
    [Tooltip("아이템의 모든 정보를 담고 있는 ScriptableObject")]
    public ItemData data;
    [Tooltip("현재 아이템 레벨")]
    public int level;
    [Tooltip("이 아이템이 생성/관리하는 무기")]
    public Weapon weapon;
    [Tooltip("이 아이템이 생성/관리하는 장비")]
    public Gear gear;

    [Header("UI References")]
    public Image icon;
    public Text textLevel;
    public Text textName;
    public Text textDesc;

    void Awake()
    {
        // UI 컴포넌트 참조는 Inspector에서 직접 연결하는 것을 권장합니다.
        // 여기서는 연결된 참조를 사용하여 초기 설정을 진행합니다.
        if (icon != null)
        {
            icon.sprite = data.itemIcon;
        }
        if (textName != null)
        {
            textName.text = data.itemName;
        }
        // textLevel과 textDesc는 OnEnable에서 설정되므로 여기서는 초기화만 합니다.
    }

    // UI가 활성화될 때마다 호출됩니다.
    void OnEnable()
    {
        // 현재 레벨에 맞는 정보를 텍스트로 표시합니다.
        // 레벨은 0부터 시작하므로, 표시할 때는 +1을 해줍니다.
        if (textLevel != null)
        {
            textLevel.text = "Lv." + (level + 1);
        }

        // 아이템 타입에 따라 설명 텍스트를 다르게 포맷팅합니다.
        if (textDesc != null)
        {
            switch (data.itemType)
            {
                case ItemData.ItemType.Melee:
                case ItemData.ItemType.Range:
                    // 설명: "피해량 {0}%, 발사체 수 {1}개"
                    textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
                    break;
                case ItemData.ItemType.Glove:
                case ItemData.ItemType.Shoe:
                    // 설명: "효과 {0}% 증가"
                    textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
                    break;
                default: // Heal 등
                    textDesc.text = string.Format(data.itemDesc);
                    break;
            }
        }
    }

    /// <summary>
    /// 아이템 UI를 클릭했을 때 호출됩니다. (Button 컴포넌트의 OnClick 이벤트에 연결)
    /// </summary>
    public void OnClick()
    {
        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level == 0) // 처음 획득하는 무기
                {
                    // 새 게임 오브젝트를 만들고 Weapon 컴포넌트를 추가하여 초기화합니다.
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else // 이미 가진 무기 레벨업
                {
                    // 다음 레벨에 맞는 데미지와 개수로 무기를 업그레이드합니다.
                    // 현재 로직은 데미지 계산에 오류가 있을 수 있습니다. (baseDamage에 계속 더하는 방식)
                    // LevelUp 함수에서 총 데미지를 계산하도록 수정하는 것이 좋습니다.
                    float nextDamage = data.baseDamage + data.baseDamage * data.damages[level];
                    int nextCount = data.counts[level];
                    weapon.LevelUp(nextDamage, nextCount);
                }
                level++;
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0) // 처음 획득하는 장비
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else // 이미 가진 장비 레벨업
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }
                level++;
                break;
            case ItemData.ItemType.Heal:
                // 즉시 체력을 모두 회복합니다.
                GameManager.instance.health = GameManager.instance.maxHealth;
                break;
        }

        // 아이템이 최대 레벨에 도달하면 버튼을 비활성화합니다.
        if (level == data.damages.Length)
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }
}
