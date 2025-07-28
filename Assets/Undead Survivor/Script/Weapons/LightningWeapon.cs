using UnityEngine;

/// <summary>
/// 'Red Sprite' 무기의 행동 로직을 관리하는 클래스입니다.
/// 일정 시간마다 사거리 안의 적에게 번개 공격을 시전합니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class LightningWeapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머
    private LightningWeaponData lightningWeaponData; // 캐시된 데이터 참조
    
    // GC 압박 줄이기 위한 배열 캐시
    private RaycastHit2D[] cachedTargetsArray = new RaycastHit2D[50]; // 최대 50개의 타겟 배열
    private Collider2D[] cachedEnemiesArray = new Collider2D[20]; // 최대 20개의 적 배열

    public override void Init(ItemData data)
    {
        base.Init(data); // 부모 클래스의 Init을 먼저 호출하여 공통 데이터를 설정합니다.
        // 부모에서 설정된 itemData를 LightningWeaponData로 캐스팅하여 저장합니다.
        this.lightningWeaponData = data as LightningWeaponData;
        if (this.lightningWeaponData == null)
        {
            Debug.LogError("LightningWeapon에 할당된 ItemData가 LightningWeaponData 타입이 아닙니다!");
        }
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
        if (lightningWeaponData == null) return; // 데이터가 없으면 공격 중단

        // 스캐너를 사용하여 사거리 내의 적들을 탐지합니다. (캐시된 배열 사용)
        int targetCount = Physics2D.CircleCastNonAlloc(player.transform.position, lightningWeaponData.lightningRange, Vector2.zero, cachedTargetsArray, 0f, lightningWeaponData.targetLayer);

        // 탐지된 적이 없으면 공격하지 않습니다.
        if (targetCount == 0) return;

        // count.Value 만큼의 번개 공격을 시전합니다.
        for (int i = 0; i < count.Value; i++)
        {
            // 1. 사거리 내의 적들 중에서 랜덤하게 하나의 적을 타겟으로 선택합니다.
            int randomIndex = Random.Range(0, targetCount);
            Transform targetEnemyTransform = cachedTargetsArray[randomIndex].transform;
            Vector3 attackPosition = targetEnemyTransform.position; // 공격이 시전될 위치

            // 2. 선택된 타겟 위치를 중심으로 attackArea.Value 범위 내의 모든 적에게 피해를 줍니다. (캐시된 배열 사용)
            int enemyCount = Physics2D.OverlapCircleNonAlloc(attackPosition, attackArea.Value, cachedEnemiesArray, lightningWeaponData.targetLayer);
            for (int j = 0; j < enemyCount; j++)
            {
                Collider2D enemyCollider = cachedEnemiesArray[j];
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    float finalDamage = damage.Value; // 기본 데미지
                    // 플레이어와 적 사이의 거리를 계산하여 추가 피해 적용 여부 결정
                    float distanceToPlayer = Vector3.Distance(player.transform.position, enemy.transform.position);
                    // 데이터 에셋의 값을 사용합니다.
                    if (distanceToPlayer > lightningWeaponData.bonusDistanceThreshold)
                    {
                        finalDamage += finalDamage * lightningWeaponData.bonusDamageMultiplier; // 추가 피해 적용
                    }
                    enemy.TakeDamage(finalDamage); // 최종 데미지 적용
                }
            }

            // 3. 해당 위치에 시각 효과를 생성합니다。
            GameObject effect = GameManager.instance.pool.Get(lightningWeaponData.projectileTag);
            if (effect == null) continue;

            effect.transform.position = attackPosition;
            effect.SetActive(true); // 오브젝트 활성화
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range); // 적절한 효과음으로 변경 가능
    }
}
