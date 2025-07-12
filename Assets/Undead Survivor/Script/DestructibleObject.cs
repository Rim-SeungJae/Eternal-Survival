
using UnityEngine;

// SpriteRenderer와 BoxCollider2D 컴포넌트가 반드시 필요합니다.
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class DestructibleObject : MonoBehaviour
{
    // 인스펙터 창에서 각 오브젝트에 맞는 데이터를 할당합니다.
    public DestructibleData data;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // 컴포넌트를 가져오고 데이터를 기반으로 오브젝트를 초기화합니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        Initialize();
    }

    // 데이터에 따라 오브젝트의 초기 상태를 설정합니다.
    private void Initialize()
    {
        if (data != null)
        {
            spriteRenderer.sprite = data.sprite;
            currentHealth = data.health;
        }
    }

    // 플레이어의 무기 등 외부 스크립트에서 이 함수를 호출하여 오브젝트에 피해를 줍니다.
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 체력이 0 이하가 되면 파괴 로직을 실행합니다.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 오브젝트가 파괴될 때 호출됩니다.
    private void Die()
    {        
        // 아이템 드롭 로직을 실행합니다.
        DropLoot();

        // 게임 오브젝트를 씬에서 제거합니다.
        Destroy(gameObject);
    }

    // 설정된 드롭 테이블에 따라 아이템을 생성합니다.
    private void DropLoot()
    {
        if (data == null || data.lootTable.Length == 0)
        {
            return;
        }

        // 드롭 테이블의 각 아이템에 대해 드롭 확률을 계산합니다.
        foreach (var loot in data.lootTable)
        {
            // Random.value는 0과 1 사이의 무작위 실수를 반환합니다.
            if (Random.value <= loot.dropChance)
            {
                // 아이템 프리팹을 현재 위치에 생성합니다.
                Instantiate(loot.itemPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
