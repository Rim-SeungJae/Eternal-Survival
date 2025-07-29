using UnityEngine;

/// <summary>
/// Hyejin 캐릭터의 기본 무기 클래스입니다.
/// 일정 시간마다 사거리 내의 가장 가까운 적을 향해 투사체를 발사합니다.
/// 기존 Weapon.cs의 원거리 무기 로직과 유사하게 동작합니다.
/// </summary>
public class HyejinWeapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머

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

    /// <summary>
    /// 가장 가까운 적을 향해 투사체를 발사합니다.
    /// </summary>
    private void Attack()
    {
        // 가장 가까운 적이 없으면 발사하지 않음
        if (!player.scanner.nearestTarget) return;

        WeaponData weaponData = itemData as WeaponData;
        if (weaponData == null) return;

        // 적의 위치와 방향 계산
        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        // 풀에서 투사체 가져오기
        Transform bullet = GameManager.instance.pool.Get(weaponData.projectileTag).transform;
        if (bullet == null) return;

        // 투사체 위치 및 회전 설정
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // 투사체 초기화
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bullet.gameObject.SetActive(true);
            ConfigureBullet(bulletComponent, dir);
        }

        // 사운드 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
    
    /// <summary>
    /// 투사체를 설정합니다. 상속받은 클래스에서 오버라이드하여 추가 효과를 적용할 수 있습니다.
    /// </summary>
    /// <param name="bulletComponent">설정할 투사체 컴포넌트</param>
    /// <param name="dir">투사체 방향</param>
    protected virtual void ConfigureBullet(Bullet bulletComponent, Vector3 dir)
    {
        bulletComponent.Init(damage.Value, (int)count.Value, duration.Value, projectileSpeed.Value, attackArea.Value, dir);
    }
} 