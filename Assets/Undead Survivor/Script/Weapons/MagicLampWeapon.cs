using UnityEngine;

/// <summary>
/// 사거리 내 가장 가까운 적을 향해 주먹 투사체를 발사하는 무기 로직을 처리합니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class MagicLampWeapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머

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
    /// 가장 가까운 적을 향해 주먹 투사체를 발사합니다.
    /// </summary>
    private void Attack()
    {
        // 스캐너를 사용하여 사거리 내의 적들을 탐지합니다.
        // 가장 가까운 적이 없으면 공격하지 않습니다.
        if (!player.scanner.nearestTarget) return;

        // 가장 가까운 적의 위치를 가져옵니다.
        Vector3 targetPos = player.scanner.nearestTarget.position;
        // 플레이어 위치에서 적을 향하는 방향 벡터를 계산합니다.
        Vector3 dir = (targetPos - player.transform.position).normalized;

        // count.Value 만큼의 주먹 투사체를 발사합니다.
        for (int i = 0; i < count.Value; i++)
        {
            WeaponData weaponData = itemData as WeaponData;
            // 이펙트 풀에서 주먹 투사체 오브젝트를 가져옵니다.
            GameObject fist = GameManager.instance.pool.Get(weaponData.projectileTag);
            if (fist == null)
            {
                Debug.LogWarning($"PoolManager에서 태그 '{weaponData.projectileTag}'에 해당하는 이펙트를 가져오지 못했습니다. PoolManager 설정을 확인하세요.");
                continue;
            }

            // 플레이어 위치에서 주먹 투사체를 생성합니다.
            fist.transform.position = player.transform.position;
            // 주먹 투사체의 회전을 적을 향하도록 설정합니다.
            fist.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            // 최종 계산된 범위(attackArea.Value)를 투사체의 크기에 적용합니다.
            fist.transform.localScale = Vector3.one * attackArea.Value;

            // MagicLampFist 스크립트에 데미지 정보를 전달하고 활성화합니다.
            MagicLampEffect fistLogic = fist.GetComponent<MagicLampEffect>();
            if (fistLogic != null)
            {
                fistLogic.Init(damage.Value, duration.Value, projectileSpeed.Value, dir);
            }
            fist.SetActive(true); // 오브젝트 활성화
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Melee); // 적절한 효과음으로 변경 가능
    }
}
