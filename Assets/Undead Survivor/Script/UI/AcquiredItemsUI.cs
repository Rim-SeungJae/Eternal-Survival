using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 획득한 무기와 장비 아이템 목록을 화면에 표시하는 UI를 관리합니다.
/// </summary>
public class AcquiredItemsUI : MonoBehaviour
{
    public static AcquiredItemsUI instance; // 싱글톤 인스턴스

    [Header("UI References")]
    [Tooltip("무기 아이콘 슬롯들이 생성될 부모 Transform")]
    public Transform weaponSlotParent;
    [Tooltip("장비 아이콘 슬롯들이 생성될 부모 Transform")]
    public Transform gearSlotParent;
    [Tooltip("아이콘을 표시할 UI 슬롯 프리팹")]
    public GameObject slotPrefab;
    [Tooltip("비어있는 슬롯에 표시할 기본 스프라이트")]
    public Sprite emptySlotSprite;

    private List<Image> weaponSlots = new List<Image>();
    private List<Image> gearSlots = new List<Image>();

    void Awake()
    {
        instance = this;
        InitializeSlots();
    }

    /// <summary>
    /// UI 슬롯들을 미리 생성하고 초기화합니다.
    /// </summary>
    private void InitializeSlots()
    {
        // 기존 슬롯들 초기화
        weaponSlots = new List<Image>();
        gearSlots = new List<Image>();


        // 무기 슬롯 생성
        for (int i = 0; i < Player.MAX_WEAPONS; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, weaponSlotParent);
            Image slotImage = newSlot.GetComponent<Image>();
            if (slotImage != null)
            {
                weaponSlots.Add(slotImage);
                newSlot.SetActive(false);
            }
        }

        // 장비 슬롯 생성
        for (int i = 0; i < Player.MAX_GEARS; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, gearSlotParent);
            Image slotImage = newSlot.GetComponent<Image>();
            gearSlots.Add(slotImage);
            newSlot.SetActive(false);
        }

    }

    /// <summary>
    /// 획득한 아이템 목록 UI를 갱신합니다.
    /// </summary>
    public void UpdateUI(List<Item> items)
    {

        // 1. 획득한 아이템을 무기와 장비로 분류합니다. (null 체크 추가)
        List<Item> acquiredWeapons = items.Where(item => item != null && item.data != null && item.data.itemType == ItemData.ItemType.Weapon).ToList();
        List<Item> acquiredGears = items.Where(item => item != null && item.data != null && item.data.itemType == ItemData.ItemType.Gear).ToList();

        // 2. 무기 UI를 갱신합니다.
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (i < acquiredWeapons.Count)
            {
                // 획득한 무기 아이콘 표시
                weaponSlots[i].sprite = acquiredWeapons[i].data.itemIcon;
                weaponSlots[i].color = Color.white; // 불투명하게
            }
            else
            {
                // 빈 슬롯 표시
                weaponSlots[i].sprite = emptySlotSprite;
                weaponSlots[i].color = new Color(1, 1, 1, 0.5f); // 반투명하게
            }
            weaponSlots[i].gameObject.SetActive(true); // 모든 슬롯을 활성화
        }

        // 3. 장비 UI를 갱신합니다.
        for (int i = 0; i < gearSlots.Count; i++)
        {
            if (i < acquiredGears.Count)
            {
                // 획득한 장비 아이콘 표시
                gearSlots[i].sprite = acquiredGears[i].data.itemIcon;
                gearSlots[i].color = Color.white;
            }
            else
            {
                // 빈 슬롯 표시
                gearSlots[i].sprite = emptySlotSprite;
                gearSlots[i].color = new Color(1, 1, 1, 0.5f);
            }
            gearSlots[i].gameObject.SetActive(true); // 모든 슬롯을 활성화
        }
    }
}
