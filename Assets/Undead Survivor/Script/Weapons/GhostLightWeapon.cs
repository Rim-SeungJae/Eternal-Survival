using UnityEngine;

/// <summary>
/// 'Ghost Light' 무기의 주 로직을 관리합니다.
/// 주기적으로 사거리 내의 랜덤한 적을 향해 유도 투사체를 발사합니다.
/// </summary>
public class GhostLightWeapon : WeaponBase
{
    private float timer;

    public override void Init(ItemData data)
    {
        base.Init(data);
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

    private void Attack()
    {
        if (!(itemData is GhostLightData ghostLightData)) return;

        // count.Value 만큼의 투사체를 발사합니다.
        for (int i = 0; i < count.Value; i++)
        {
            // 플레이어 주변 attackArea.Value 반경 내의 무작위 위치를 생성합니다.
            Vector3 randomTargetPosition = player.transform.position + (Vector3)Random.insideUnitCircle * attackArea.Value;

            // 풀에서 투사체를 가져옵니다.
            GameObject projectileObj = GameManager.instance.pool.Get(ghostLightData.projectileTag);
            if (projectileObj == null) continue;

            projectileObj.transform.position = player.transform.position;
            GhostLightProjectile projectile = projectileObj.GetComponent<GhostLightProjectile>();
            if (projectile != null)
            {
                projectile.Init(damage.Value, duration.Value, projectileSpeed.Value, attackArea.Value, randomTargetPosition, ghostLightData.groundEffectTickRate, player.transform);
            }
            projectileObj.SetActive(true);
        }

        // TODO: 발사 효과음 재생
    }
}
