
using UnityEngine;

// 아이템 드롭 정보를 담는 구조체입니다.
[System.Serializable]
public struct LootItem
{
    [Tooltip("PoolManager에 등록된 아이템의 태그")]
    [PoolTagSelector] // 이 어트리뷰트를 추가!
    public string itemTag;
    // 드롭될 확률입니다. (0.0 ~ 1.0)
    [Range(0, 1)]
    public float dropChance;
}

// 'Assets > Create > Data > Destructible' 메뉴를 통해 에셋을 생성할 수 있습니다.
[CreateAssetMenu(fileName = "New Destructible Data", menuName = "Data/Destructible")]
public class DestructibleData : ScriptableObject
{
    [Header("기본 정보")]
    public string objectName;
    // 파괴되기까지 필요한 타격 횟수입니다.
    public int health = 1;
    // 오브젝트의 기본 스프라이트입니다.
    public Sprite sprite;
    public float colliderXSize = 1f;
    public float colliderYSize = 1f;

    [Header("드롭 아이템")]
    // 드롭 가능한 아이템 목록과 각 아이템의 드롭 확률입니다.
    public LootItem[] lootTable;
}
