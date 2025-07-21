
using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

// SpriteRenderer와 BoxCollider2D 컴포넌트가 반드시 필요합니다.
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class DestructibleObject : MonoBehaviour
{
    // 인스펙터 창에서 각 오브젝트에 맞는 데이터를 할당합니다.
    public DestructibleData data;
    [Tooltip("파괴된 후 다시 생성되기까지 걸리는 시간(초)")]
    public float respawnTime = 10f;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        // 컴포넌트를 가져오고 데이터를 기반으로 오브젝트를 초기화합니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        Initialize();
    }

    // 데이터에 따라 오브젝트의 초기 상태를 설정하고, 보이도록 만듭니다.
    private void Initialize()
    {
        if (data != null)
        {
            spriteRenderer.enabled = true; // 스프라이트 활성화
            boxCollider.enabled = true;    // 콜라이더 활성화
            spriteRenderer.sprite = data.sprite;
            currentHealth = data.health;
            boxCollider.size = new Vector2(data.colliderXSize, data.colliderYSize);
            boxCollider.isTrigger = true;
        }
    }

    // 플레이어의 무기 등 외부 스크립트에서 이 함수를 호출하여 오브젝트에 피해를 줍니다.
    public void TakeDamage(int damage)
    {
        // 이미 파괴된 상태(콜라이더가 꺼진 상태)라면 피해를 받지 않습니다.
        if (!boxCollider.enabled) return;

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

        // 오브젝트를 즉시 파괴하는 대신, 비활성화하고 재생성 코루틴을 시작합니다.
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
        StartCoroutine(RespawnRoutine());
    }

    // 지정된 시간 후에 오브젝트를 다시 활성화하는 코루틴입니다.
    private IEnumerator RespawnRoutine()
    {
        // 지정된 시간만큼 기다립니다.
        yield return new WaitForSeconds(respawnTime);

        // 오브젝트의 상태를 초기화하여 다시 나타나게 합니다.
        Initialize();
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
                // 풀 매니저에서 태그를 사용하여 아이템을 가져옵니다.
                GameObject item = GameManager.instance.pool.Get(loot.itemTag);
                if (item != null)
                {
                    item.transform.position = transform.position;
                    item.SetActive(true); // 오브젝트 활성화
                }
            }
        }
    }
}
