using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 진화 조건과 결과를 정의하는 ScriptableObject 클래스입니다.
/// </summary>
[CreateAssetMenu(fileName = "WeaponEvolutionData", menuName = "Scriptable Objects/Weapon Evolution Data")]
public class WeaponEvolutionData : ScriptableObject
{
    [Header("# Evolution Info")]
    [Tooltip("진화할 원본 무기 데이터")]
    public WeaponData originalWeapon;
    
    [Tooltip("진화 후 무기 데이터")]
    public WeaponData evolvedWeapon;
    
    [Tooltip("진화 조건에 필요한 Gear 아이템들")]
    public List<GearData> requiredGears = new List<GearData>();
    
    [Tooltip("진화 조건에 필요한 최소 Gear 개수 (0이면 모든 requiredGears가 필요)")]
    public int minRequiredGearCount = 0;
    
    [Tooltip("진화 설명")]
    [TextArea]
    public string evolutionDescription;
    
    [Header("# Evolution Visuals")]
    [Tooltip("진화 UI에 표시될 아이콘")]
    public Sprite evolutionIcon;
    
    /// <summary>
    /// 플레이어가 진화 조건을 만족하는지 확인합니다.
    /// </summary>
    /// <param name="player">플레이어 참조</param>
    /// <returns>진화 가능 여부</returns>
    public bool CanEvolve(Player player)
    {
        // 1. 원본 무기가 최고 레벨인지 확인
        Item originalItem = player.items.Find(item => item.data == originalWeapon);
        if (originalItem == null || originalItem.level < originalWeapon.maxLevel)
        {
            Debug.Log($"진화 조건 실패 - 무기: {originalWeapon?.itemName}, 레벨: {originalItem?.level}/{originalWeapon?.maxLevel}");
            return false;
        }
        
        // 2. 필요한 Gear 아이템들을 보유하고 있는지 확인
        if (requiredGears.Count == 0)
        {
            Debug.Log($"진화 조건 성공 - 무기만 최고 레벨: {originalWeapon.itemName}");
            return true; // Gear 조건이 없으면 무기만 최고 레벨이면 진화 가능
        }
        
        int requiredCount = minRequiredGearCount > 0 ? minRequiredGearCount : requiredGears.Count;
        int ownedCount = 0;
        
        foreach (GearData requiredGear in requiredGears)
        {
            Item gearItem = player.items.Find(item => item.data == requiredGear);
            if (gearItem != null && gearItem.level > 0) // 장착된 Gear만 카운트
            {
                ownedCount++;
            }
        }
        
        bool canEvolve = ownedCount >= requiredCount;
        Debug.Log($"진화 조건 확인 - 무기: {originalWeapon.itemName}, Gear: {ownedCount}/{requiredCount}, 결과: {canEvolve}");
        
        return canEvolve;
    }
    
    /// <summary>
    /// 진화 조건을 만족하는 Gear 아이템 목록을 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 참조</param>
    /// <returns>보유 중인 필요한 Gear 아이템 목록</returns>
    public List<Item> GetOwnedRequiredGears(Player player)
    {
        List<Item> ownedGears = new List<Item>();
        
        foreach (GearData requiredGear in requiredGears)
        {
            Item gearItem = player.items.Find(item => item.data == requiredGear);
            if (gearItem != null && gearItem.level > 0)
            {
                ownedGears.Add(gearItem);
            }
        }
        
        return ownedGears;
    }
    
    /// <summary>
    /// 진화 조건을 만족하지 않는 Gear 아이템 목록을 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 참조</param>
    /// <returns>보유하지 않은 필요한 Gear 아이템 목록</returns>
    public List<GearData> GetMissingRequiredGears(Player player)
    {
        List<GearData> missingGears = new List<GearData>();
        
        foreach (GearData requiredGear in requiredGears)
        {
            Item gearItem = player.items.Find(item => item.data == requiredGear);
            if (gearItem == null || gearItem.level == 0)
            {
                missingGears.Add(requiredGear);
            }
        }
        
        return missingGears;
    }
} 