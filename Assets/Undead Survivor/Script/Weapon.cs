using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시간 기반으로 작동하는 무기(근접, 원거리)의 행동 로직을 관리하는 클래스입니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class Weapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머

    public override void Awake()
    {
        base.Awake(); // 부모 클래스의 Awake()를 먼저 호출합니다.
    }

    // 레벨업 시 근접 무기는 즉시 재배치해야 하므로 ApplyLevelData를 오버라이드합니다.
    protected override void ApplyLevelData()
    {
        base.ApplyLevelData();
        if (itemData.itemType == ItemData.ItemType.Melee)
        {
            Deploy();
        }
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        // 아이템 타입에 따라 다른 공격 로직 수행
        switch (itemData.itemType)
        {
            case ItemData.ItemType.Melee:
                // 근접 무기는 회전 속도(projectileSpeed.Value)에 따라 회전
                transform.Rotate(Vector3.back * projectileSpeed.Value * Time.deltaTime);
                break;
            case ItemData.ItemType.Range:
                timer += Time.deltaTime;
                if (timer > cooldown.Value)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }
    }

    /// <summary>
    /// 근접 무기를 배치하거나 레벨업 시 갱신합니다。
    /// </summary>
    void Deploy()
    {
        // 모든 자식들을 List로 복사해서 처리
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        // 복사한 List를 기반으로 반복 처리
        for (int i = 0; i < children.Count; i++)
        {
            Transform child = children[i];
            Poolable poolable = child.GetComponent<Poolable>();
            if (poolable != null)
                GameManager.instance.pool.ReturnToPool(poolable.poolTag, child.gameObject);
            else
                child.gameObject.SetActive(false);
        }

        float angleStep = 360f / count.Value;
        for (int i = 0; i < (int)count.Value; i++)
        {
            Transform bullet = GameManager.instance.pool.Get(itemData.projectileTag).transform;
            bullet.parent = transform;
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * angleStep * i;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World); // 고정된 배치 거리 사용 (1.5f)

            bullet.GetComponent<Bullet>().Init(damage.Value, -100, duration.Value, projectileSpeed.Value, attackArea.Value, Vector3.zero);
        }
    }

    /// <summary>
    /// 원거리 무기를 발사합니다。
    /// </summary>
    void Fire()
    {
        if (!player.scanner.nearestTarget) return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        Transform bullet = GameManager.instance.pool.Get(itemData.projectileTag).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        bullet.GetComponent<Bullet>().Init(damage.Value, (int)count.Value, duration.Value, projectileSpeed.Value, attackArea.Value, dir);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}


