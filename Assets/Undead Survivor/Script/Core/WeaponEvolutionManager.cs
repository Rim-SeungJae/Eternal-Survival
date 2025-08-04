using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 진화 시스템을 관리하는 매니저 클래스입니다.
/// 진화 데이터를 관리하고 진화 가능한 무기들을 추적합니다.
/// </summary>
public class WeaponEvolutionManager : MonoBehaviour
{
    [Header("# Evolution Data")]
    [Tooltip("모든 무기 진화 데이터 목록")]
    public List<WeaponEvolutionData> evolutionDataList = new List<WeaponEvolutionData>();
    
    private static WeaponEvolutionManager instance;
    public static WeaponEvolutionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<WeaponEvolutionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("WeaponEvolutionManager");
                    instance = go.AddComponent<WeaponEvolutionManager>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 진화 데이터 자동 로드
            LoadEvolutionData();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 특정 무기가 진화 무기인지 확인합니다.
    /// </summary>
    /// <param name="weaponData">확인할 무기 데이터</param>
    /// <returns>진화 무기 여부</returns>
    public bool IsEvolvableWeapon(WeaponData weaponData)
    {
        foreach (WeaponEvolutionData evolutionData in evolutionDataList)
        {
            if (evolutionData.evolvedWeapon == weaponData)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 진화 데이터를 자동으로 로드합니다.
    /// </summary>
    private void LoadEvolutionData()
    {
        // 진화 데이터가 비어있으면 경고
        if (evolutionDataList.Count == 0)
        {
            Debug.LogWarning("진화 데이터가 설정되지 않았습니다. Inspector에서 수동으로 추가해주세요.");
            Debug.LogWarning("추가할 진화 데이터: Assets/Undead Survivor/Data/Weapon Evolution/Hyejin Weapon Evolution.asset");
        }
        else
        {
            Debug.Log($"진화 데이터 {evolutionDataList.Count}개가 설정되어 있습니다.");
        }
    }
    
    /// <summary>
    /// 플레이어가 보유한 무기 중 진화 가능한 무기 목록을 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 참조</param>
    /// <returns>진화 가능한 무기와 진화 데이터의 쌍 목록</returns>
    public List<EvolutionCandidate> GetEvolvableWeapons(Player player)
    {
        List<EvolutionCandidate> candidates = new List<EvolutionCandidate>();
        
        foreach (WeaponEvolutionData evolutionData in evolutionDataList)
        {
            if (evolutionData.CanEvolve(player))
            {
                Item originalItem = player.items.Find(item => item.data == evolutionData.originalWeapon);
                if (originalItem != null)
                {
                    candidates.Add(new EvolutionCandidate
                    {
                        originalItem = originalItem,
                        evolutionData = evolutionData
                    });
                }
            }
        }
        
        return candidates;
    }
    
    /// <summary>
    /// 특정 무기의 진화 데이터를 찾습니다.
    /// </summary>
    /// <param name="weaponData">찾을 무기 데이터</param>
    /// <returns>진화 데이터 (없으면 null)</returns>
    public WeaponEvolutionData GetEvolutionData(WeaponData weaponData)
    {
        return evolutionDataList.Find(data => data.originalWeapon == weaponData);
    }
    
    /// <summary>
    /// 무기를 진화시킵니다.
    /// </summary>
    /// <param name="originalItem">진화할 원본 아이템</param>
    /// <param name="evolutionData">진화 데이터</param>
    /// <returns>진화 성공 여부</returns>
    public bool EvolveWeapon(Item originalItem, WeaponEvolutionData evolutionData)
    {
        if (originalItem == null || evolutionData == null)
        {
            Debug.LogError("진화에 필요한 데이터가 null입니다.");
            return false;
        }
        
        Player player = GameManager.instance.player;
        if (!evolutionData.CanEvolve(player))
        {
            Debug.LogWarning("진화 조건을 만족하지 않습니다.");
            return false;
        }
        
        // 1. 기존 무기 제거
        if (originalItem.weapon != null)
        {
            Destroy(originalItem.weapon.gameObject);
        }
        player.items.Remove(originalItem);
        
        // 2. 진화된 무기 생성 및 장착
        GameObject evolvedItemObject = new GameObject("Evolved Weapon Item");
        Item evolvedItem = evolvedItemObject.AddComponent<Item>();
        evolvedItem.data = evolutionData.evolvedWeapon;
        evolvedItem.level = 0; // 진화된 무기는 레벨 0부터 시작
        
        if (evolvedItem.data.itemAction != null)
        {
            evolvedItem.data.itemAction.OnEquip(evolvedItem);
            player.AddItem(evolvedItem);
        }
        
        // 3. UI 갱신
        if (AcquiredItemsUI.instance != null)
        {
            AcquiredItemsUI.instance.UpdateUI(player.items);
        }
        
        Debug.Log($"무기 '{evolutionData.originalWeapon.itemName}'이(가) '{evolutionData.evolvedWeapon.itemName}'으로 진화했습니다!");
        
        // 4. 진화 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        
        return true;
    }
}

/// <summary>
/// 진화 가능한 무기와 진화 데이터를 담는 구조체입니다.
/// </summary>
[System.Serializable]
public struct EvolutionCandidate
{
    public Item originalItem;
    public WeaponEvolutionData evolutionData;
} 