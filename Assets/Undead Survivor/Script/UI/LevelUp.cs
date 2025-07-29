using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 시 아이템 선택 UI를 관리하는 클래스입니다.
/// </summary>
public class LevelUp : MonoBehaviour
{
    private RectTransform rect;
    private Item[] items; // 자식으로 있는 아이템 UI 요소들

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        // 비활성화된 자식도 포함하여 모든 Item 컴포넌트를 가져옵니다.
        items = GetComponentsInChildren<Item>(true);
    }

    /// <summary>
    /// 레벨업 UI를 표시합니다.
    /// </summary>
    public void Show()
    {
        // 선택할 아이템들을 랜덤하게 준비합니다.
        Next();
        
        // UI를 화면에 보이게 하고 게임을 정지시킵니다.
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        // 관련 효과음을 재생합니다.
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    /// <summary>
    /// 레벨업 UI를 숨깁니다.
    /// </summary>
    public void Hide()
    {
        // UI를 화면에서 보이지 않게 하고 게임을 재개합니다.
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        // 관련 효과음을 재생합니다.
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    /// <summary>
    /// 특정 인덱스의 아이템을 선택합니다.
    /// </summary>
    /// <param name="index">선택한 아이템의 인덱스</param>
    public void Select(int index)
    {
        items[index].OnClick();
    }

    /// <summary>
    /// 다음 레벨업 시 보여줄 아이템 3개를 랜덤하게 선택하여 활성화합니다.
    /// </summary>
    private void Next()
    {
        // 먼저 모든 아이템을 비활성화합니다.
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 1. 유효한 후보 아이템 목록을 생성합니다.
        List<Item> candidates = new List<Item>();
        foreach (Item item in items)
        {
            // 조건 1: 최대 레벨이 아닌 아이템
            if (item.level < item.data.maxLevel)
            {
                // 조건 2: 데이터의 itemType을 확인하여 무기/장비 획득 제한을 검사합니다.
                switch (item.data.itemType)
                {
                    case ItemData.ItemType.Weapon:
                        // 진화 무기는 별도 처리하므로 제외
                        if (IsEvolutionWeapon(item)) continue;
                        if (item.level == 0 && GameManager.instance.player.WeaponCount >= Player.MAX_WEAPONS) continue;
                        candidates.Add(item);
                        break;
                    case ItemData.ItemType.Gear:
                        if (item.level == 0 && GameManager.instance.player.GearCount >= Player.MAX_GEARS) continue;
                        candidates.Add(item);
                        break;
                    default: // 소모품 등 기타 아이템은 항상 후보에 포함
                        candidates.Add(item);
                        break;
                }
            }
        }

        // 2. 진화 가능한 무기들을 후보에 추가
        List<EvolutionCandidate> evolvableWeapons = GetEvolvableWeapons();
        foreach (EvolutionCandidate evolution in evolvableWeapons)
        {
            // 진화 무기를 표시할 아이템 찾기 (진화 무기용 아이템은 별도로 추가되어야 함)
            Item evolutionItem = FindEvolutionItem(evolution.evolutionData.evolvedWeapon);
            if (evolutionItem != null)
            {
                candidates.Add(evolutionItem);
            }
        }

        // 4. 후보 중에서 랜덤하게 3개를 선택합니다.
        List<Item> selectedItems = new List<Item>();
        while (candidates.Count > 0 && selectedItems.Count < 3)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            selectedItems.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }

        // 5. 선택된 아이템들을 활성화합니다.
        foreach (Item item in selectedItems)
        {
            item.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 진화 가능한 무기들을 가져옵니다.
    /// </summary>
    private List<EvolutionCandidate> GetEvolvableWeapons()
    {
        List<EvolutionCandidate> candidates = new List<EvolutionCandidate>();
        
        if (WeaponEvolutionManager.Instance != null)
        {
            candidates = WeaponEvolutionManager.Instance.GetEvolvableWeapons(GameManager.instance.player);
        }
        
        return candidates;
    }
    
    /// <summary>
    /// 진화 무기인지 확인합니다.
    /// </summary>
    private bool IsEvolutionWeapon(Item item)
    {
        if (item.data is WeaponData weaponData)
        {
            return WeaponEvolutionManager.Instance.IsEvolvableWeapon(weaponData);
        }
        return false;
    }

    /// <summary>
    /// 진화 무기에 해당하는 아이템을 찾습니다.
    /// </summary>
    private Item FindEvolutionItem(WeaponData evolvedWeapon)
    {
        foreach (Item item in items)
        {
            if (item.data == evolvedWeapon)
            {
                return item;
        }
    }
        return null;
    }
    
}