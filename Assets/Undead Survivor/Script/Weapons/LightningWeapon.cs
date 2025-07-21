using UnityEngine;

/// <summary>
/// 'Red Sprite' 무기의 행동 로직을 관리하는 클래스입니다.
/// 일정 시간마다 사거리 안의 적에게 번개 공격을 시전합니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class LightningWeapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머

    [Header("Bonus Damage Settings")]
    [Tooltip("추가 피해가 적용되기 시작하는 플레이어로부터의 거리")]
    public float bonusDistanceThreshold = 5f;
    [Tooltip("거리 밖에 있는 적에게 적용될 추가 피해 배율 (예: 0.5f는 50% 추가 피해)")]
    public float bonusDamageMultiplier = 0.5f;

    public override void Awake()
    {
        base.Awake(); // 부모 클래스의 Awake()를 먼저 호출합니다.
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        if (timer > cooldown.Value)
        {
            timer = 0f;
            Attack();
        }
    }

    /// <summary>
    /// 사거리 내의 적에게 번개 공격을 시전하고 시각 효과를 생성합니다.
    /// </summary>
    private void Attack()
    {
        // 스캐너를 사용하여 사거리 내의 적들을 탐지합니다.
        RaycastHit2D[] targetsInScanRange = player.scanner.targets;

        // 탐지된 적이 없으면 공격하지 않습니다.
        if (targetsInScanRange.Length == 0) return;

        // count.Value 만큼의 번개 공격을 시전합니다.
        for (int i = 0; i < count.Value; i++)
        {
            // 1. 사거리 내의 적들 중에서 랜덤하게 하나의 적을 타겟으로 선택합니다.
            int randomIndex = Random.Range(0, targetsInScanRange.Length);
            Transform targetEnemyTransform = targetsInScanRange[randomIndex].transform;
            Vector3 attackPosition = targetEnemyTransform.position; // 공격이 시전될 위치

            // 2. 선택된 타겟 위치를 중심으로 attackArea.Value 범위 내의 모든 적에게 피해를 줍니다.
            Collider2D[] enemiesInArea = Physics2D.OverlapCircleAll(attackPosition, attackArea.Value, player.scanner.targetLayer); // player.scanner.targetLayer 사용
            foreach (Collider2D enemyCollider in enemiesInArea)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    float finalDamage = damage.Value; // 기본 데미지
                    // 플레이어와 적 사이의 거리를 계산하여 추가 피해 적용 여부 결정
                    float distanceToPlayer = Vector3.Distance(player.transform.position, enemy.transform.position);
                    if (distanceToPlayer > bonusDistanceThreshold)
                    {
                        finalDamage += finalDamage * bonusDamageMultiplier; // 추가 피해 적용
                    }
                    enemy.TakeDamage(finalDamage); // 최종 데미지 적용
                }
            }

            // 3. 해당 위치에 시각 효과를 생성합니다.
            GameObject effect = GameManager.instance.pool.Get(itemData.projectileTag);
            if (effect == null)
            {
                Debug.LogWarning($"PoolManager에서 태그 '{itemData.projectileTag}'에 해당하는 이펙트를 가져오지 못했습니다. PoolManager 설정을 확인하세요.");
                continue; // 다음 공격 시도
            }

            effect.transform.position = attackPosition; // 공격이 시전된 위치에 이펙트 생성
            // 이펙트의 크기는 시각적인 부분으로, 필요에 따라 attackArea.Value를 사용하거나 고정값을 사용합니다.
            // 여기서는 시각적인 번개 줄기 효과이므로, 크기 조절은 LightningEffect 스크립트 내부에서 처리하는 것이 더 적합할 수 있습니다.
            // effect.transform.localScale = Vector3.one; // 기본 크기 유지

            // LightningEffect 스크립트에 Init 메서드가 있다면 호출합니다.
            LightningEffect effectLogic = effect.GetComponent<LightningEffect>();
            effect.SetActive(true); // 오브젝트 활성화
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range); // 적절한 효과음으로 변경 가능
    }
}
