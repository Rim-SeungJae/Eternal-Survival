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
    public WeaponBase weapon;
    
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

        // itemAction을 통해 동적으로 생성된 설명을 가져와 표시합니다.
        if (textDesc != null && data.itemAction != null)
        {
            textDesc.text = data.itemAction.GetDescription(this);
        }
        // itemAction이 없는 아이템(예: Heal)은 기존 설명을 그대로 사용합니다.
        else if (textDesc != null)
        { 
            textDesc.text = data.itemDesc;
        }
    }
    
    /// <summary>
    /// 아이템 UI를 클릭했을 때 호출됩니다. (Button 컴포넌트의 OnClick 이벤트에 연결)
    /// </summary>
    public void OnClick()
    {
        // 진화 무기인지 확인하고 처리
        if (IsEvolutionWeapon())
        {
            HandleEvolution();
            return;
        }
        
        if (data.itemAction != null)
        {
            if (level == 0)
            {
                // 새로운 아이템 장착
                data.itemAction.OnEquip(this);
                GameManager.instance.player.AddItem(this); // 플레이어에게 아이템 추가
            }
            else
            {
                // 기존 아이템 레벨업
                // 플레이어가 보유한 동일한 아이템을 찾아서 레벨업
                Item existingItem = GameManager.instance.player.items.Find(item => item.data == this.data);
                if (existingItem != null)
                {
                    // 기존 아이템의 레벨업 처리
                    data.itemAction.OnLevelUp(existingItem);
                }
                else
                {
                    // 기존 아이템을 찾지 못한 경우 (예외 상황)
                    Debug.LogWarning($"기존 아이템을 찾을 수 없습니다: {data.itemName}");
                    data.itemAction.OnLevelUp(this);
                }
            }
        }
        // Heal과 같은 일회성 아이템 처리 (Action을 사용하지 않는 경우)
        else if (data.itemName == "Heal") // 임시로 이름으로 구분
        {
            GameManager.instance.health = GameManager.instance.maxHealth;
        }

        if(data.itemName != "Heal") level++;

        // 아이템이 최대 레벨에 도달하면 버튼을 비활성화합니다.
        if (level == data.maxLevel + 1)
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }

        // UI 갱신
        if (AcquiredItemsUI.instance != null)
        {
            AcquiredItemsUI.instance.UpdateUI(GameManager.instance.player.items);
        }
    }
    
    /// <summary>
    /// 이 아이템이 진화 무기인지 확인합니다.
    /// </summary>
    private bool IsEvolutionWeapon()
    {
        if (WeaponEvolutionManager.Instance == null) return false;
        
        // 진화된 무기 데이터인지 확인
        foreach (WeaponEvolutionData evolutionData in WeaponEvolutionManager.Instance.evolutionDataList)
        {
            if (evolutionData.evolvedWeapon == data)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 진화를 처리합니다.
    /// </summary>
    private void HandleEvolution()
    {
        if (WeaponEvolutionManager.Instance == null)
        {
            Debug.LogError("WeaponEvolutionManager를 찾을 수 없습니다.");
            return;
        }
        
        // 진화 데이터 찾기
        WeaponEvolutionData evolutionData = null;
        foreach (WeaponEvolutionData evoData in WeaponEvolutionManager.Instance.evolutionDataList)
        {
            if (evoData.evolvedWeapon == data)
            {
                evolutionData = evoData;
                break;
            }
        }
        
        if (evolutionData == null)
        {
            Debug.LogError("진화 데이터를 찾을 수 없습니다.");
            return;
        }
        
        // 원본 무기 찾기
        Item originalItem = GameManager.instance.player.items.Find(item => item.data == evolutionData.originalWeapon);
        if (originalItem == null)
        {
            Debug.LogError("진화할 원본 무기를 찾을 수 없습니다.");
            return;
        }
        
        // 진화 실행
        bool success = WeaponEvolutionManager.Instance.EvolveWeapon(originalItem, evolutionData);
        
        if (success)
        {
            Debug.Log($"무기 진화 성공: {evolutionData.originalWeapon.itemName} → {evolutionData.evolvedWeapon.itemName}");
            
            // 진화 후 UI 갱신
            if (AcquiredItemsUI.instance != null)
            {
                AcquiredItemsUI.instance.UpdateUI(GameManager.instance.player.items);
            }
            
            // LevelUp UI 숨기기
            GameManager.instance.uiLevelUp.Hide();
        }
        else
        {
            Debug.LogError("무기 진화에 실패했습니다.");
        }
    }
}
