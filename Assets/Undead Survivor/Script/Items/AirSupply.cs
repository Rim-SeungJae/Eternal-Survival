using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보스가 드롭하는 보상 상자입니다. 플레이어가 접촉하면 현재 보유한 아이템 중 하나를 무작위로 레벨업하거나 진화시킵니다.
/// </summary>
public class AirSupply : MonoBehaviour
{
    [Header("Reward Settings")]
    [SerializeField] private int expReward = 50; // 업그레이드할 아이템이 없을 때 주는 경험치
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true; // 디버깅 로그 활성화
    
    private bool isCollected = false;
    
    void OnEnable()
    {
        // 재사용 시 상태 초기화
        isCollected = false;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌했는지 확인
        if (other.CompareTag("Player") && !isCollected)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                CollectAirSupply(player);
            }
        }
    }
    
    /// <summary>
    /// AirSupply를 수집하고 플레이어의 아이템을 무작위로 업그레이드합니다.
    /// </summary>
    private void CollectAirSupply(Player player)
    {
        isCollected = true;
        
        // 플레이어가 보유한 아이템 중에서 업그레이드 가능한 아이템 찾기
        List<Item> upgradableItems = GetUpgradableItems(player);
        
        Item selectedItem = null;
        bool isExperienceReward = false;
        
        if (upgradableItems.Count > 0)
        {
            // 무작위로 하나 선택
            selectedItem = upgradableItems[Random.Range(0, upgradableItems.Count)];
        }
        else
        {
            // 업그레이드할 아이템이 없으면 경험치 보상으로 설정
            isExperienceReward = true;
            
            if (enableDebugLog)
            {
                Debug.Log($"[AirSupply] 업그레이드 가능한 아이템이 없어 경험치 {expReward}를 지급할 예정입니다.");
            }
        }
        
        // UI 애니메이션 시작
        if (AirSupplyUI.Instance != null)
        {
            Sprite rewardSprite = GetRewardSprite(selectedItem, isExperienceReward);
            AirSupplyUI.Instance.PlayRewardAnimation(rewardSprite, () =>
            {
                // 애니메이션 완료 후 실제 보상 지급
                ApplyReward(selectedItem, isExperienceReward, player);
                DestroyAirSupply();
            });
        }
        else
        {
            // UI가 없으면 바로 보상 지급
            ApplyReward(selectedItem, isExperienceReward, player);
            DestroyAirSupply();
        }
    }
    
    /// <summary>
    /// 실제 보상을 지급합니다.
    /// </summary>
    private void ApplyReward(Item selectedItem, bool isExperienceReward, Player player)
    {
        if (isExperienceReward)
        {
            GameManager.instance.GetExp(expReward);
        }
        else if (selectedItem != null)
        {
            UpgradeItem(selectedItem, player);
        }
    }
    
    /// <summary>
    /// 보상에 해당하는 스프라이트를 반환합니다.
    /// </summary>
    private Sprite GetRewardSprite(Item selectedItem, bool isExperienceReward)
    {
        if (isExperienceReward)
        {
            // 경험치 아이콘 스프라이트 반환 (게임에 경험치 아이콘이 있다면)
            // 임시로 null 반환 - 실제로는 경험치 아이콘 스프라이트를 설정해야 함
            return null;
        }
        else if (selectedItem != null)
        {
            // 진화 가능한지 확인하여 적절한 스프라이트 반환
            if (WeaponEvolutionManager.Instance != null && selectedItem.data is WeaponData weaponData)
            {
                List<EvolutionCandidate> evolvableWeapons = WeaponEvolutionManager.Instance.GetEvolvableWeapons(GameManager.instance.player.GetComponent<Player>());
                EvolutionCandidate evolution = evolvableWeapons.FirstOrDefault(e => e.evolutionData.originalWeapon == weaponData);
                
                if (evolution.evolutionData != null)
                {
                    // 진화 무기의 아이콘 반환
                    return evolution.evolutionData.evolvedWeapon.itemIcon;
                }
            }
            
            // 일반 아이템의 아이콘 반환
            return selectedItem.data.itemIcon;
        }
        
        return null;
    }
    
    /// <summary>
    /// 업그레이드 가능한 아이템 목록을 반환합니다.
    /// </summary>
    private List<Item> GetUpgradableItems(Player player)
    {
        List<Item> upgradableItems = new List<Item>();
        
        foreach (Item item in player.items)
        {
            // 최대 레벨이 아닌 아이템만 업그레이드 가능
            if (item.level < item.data.maxLevel)
            {
                upgradableItems.Add(item);
            }
        }
        
        // 진화 가능한 무기들도 추가
        if (WeaponEvolutionManager.Instance != null)
        {
            List<EvolutionCandidate> evolvableWeapons = WeaponEvolutionManager.Instance.GetEvolvableWeapons(player);
            
            foreach (var evolution in evolvableWeapons)
            {
                // 기본 무기가 플레이어 아이템에 있는지 확인
                Item baseWeapon = player.items.FirstOrDefault(item => 
                    item.data == evolution.evolutionData.originalWeapon);
                
                if (baseWeapon != null)
                {
                    upgradableItems.Add(baseWeapon);
                }
            }
        }
        
        return upgradableItems;
    }
    
    /// <summary>
    /// 선택된 아이템을 업그레이드하거나 진화시킵니다.
    /// </summary>
    private void UpgradeItem(Item item, Player player)
    {
        // 진화 가능한지 먼저 확인
        if (WeaponEvolutionManager.Instance != null && item.data is WeaponData weaponData)
        {
            List<EvolutionCandidate> evolvableWeapons = WeaponEvolutionManager.Instance.GetEvolvableWeapons(player);
            EvolutionCandidate evolution = evolvableWeapons.FirstOrDefault(e => e.evolutionData.originalWeapon == weaponData);
            
            if (evolution.evolutionData != null)
            {
                // 진화 실행
                WeaponEvolutionManager.Instance.EvolveWeapon(evolution.originalItem, evolution.evolutionData);
                
                if (enableDebugLog)
                {
                    Debug.Log($"[AirSupply] {item.data.itemName}을(를) {evolution.evolutionData.evolvedWeapon.itemName}(으)로 진화시켰습니다!");
                }
                
                return;
            }
        }
        
        // 진화가 불가능하면 레벨업
        if (item.level < item.data.maxLevel)
        {
            // ItemAction을 통한 레벨업 처리
            if (item.data.itemAction != null)
            {
                item.data.itemAction.OnLevelUp(item);
                item.level++;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[AirSupply] {item.data.itemName}을(를) 레벨 {item.level}(으)로 업그레이드했습니다!");
                }
            }
            else
            {
                // ItemAction이 없는 아이템의 경우 단순 레벨업
                item.level++;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[AirSupply] {item.data.itemName}을(를) 레벨 {item.level}(으)로 업그레이드했습니다!");
                }
            }
        }
    }
    
    /// <summary>
    /// AirSupply 오브젝트를 풀로 반환하거나 비활성화합니다.
    /// </summary>
    private void DestroyAirSupply()
    {
        // 풀로 반환 시도
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null && GameManager.instance?.pool != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            // Poolable이 없다면 비활성화
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 에디터에서 디버깅용으로 강제 수집을 실행합니다.
    /// </summary>
    [ContextMenu("Test Collect")]
    private void TestCollect()
    {
        if (Application.isPlaying && GameManager.instance?.player != null)
        {
            Player player = GameManager.instance.player.GetComponent<Player>();
            if (player != null)
            {
                CollectAirSupply(player);
            }
        }
    }
}