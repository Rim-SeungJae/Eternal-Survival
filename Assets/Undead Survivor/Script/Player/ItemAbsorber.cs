using UnityEngine;

/// <summary>
/// 플레이어에게 부착되어 주변의 소모성 아이템(Consumable)을 능동적으로 스캔하고 끌어당기는 역할을 합니다.
/// </summary>
public class ItemAbsorber : MonoBehaviour
{
    [Tooltip("아이템을 감지할 반경")]
    public float absorbRadius = 3f;
    [Tooltip("아이템을 감지할 레이어")]
    public LayerMask itemLayer;

    // 물리 업데이트 주기에 맞춰 아이템을 스캔합니다.
    void FixedUpdate()
    {
        // 지정된 반경 내에 있는 모든 아이템 레이어의 콜라이더를 감지합니다.
        // 이 함수는 물리 이벤트보다 훨씬 가볍고 능동적인 방식입니다.
        Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, absorbRadius, itemLayer);

        foreach (var itemCollider in items)
        {
            // 감지된 아이템에서 Consumable 컴포넌트를 가져옵니다.
            Consumable consumable = itemCollider.GetComponent<Consumable>();
            if (consumable != null)
            {
                // Consumable에게 흡수를 시작하라고 명령합니다.
                consumable.StartAbsorb(transform.parent); // 부모(Player)의 Transform을 전달
            }
        }
    }

    /// <summary>
    /// 아이템 흡수 범위를 설정(변경)하는 함수입니다.
    /// '자석' 아이템 같은 능력치 강화에 사용될 수 있습니다.
    /// </summary>
    /// <param name="radius">새로운 흡수 반경</param>
    public void SetAbsorptionRadius(float radius)
    {
        this.absorbRadius = radius;
    }

    // 기즈모를 사용하여 Scene 뷰에서 흡수 범위를 시각적으로 확인할 수 있습니다.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, absorbRadius);
    }
}
