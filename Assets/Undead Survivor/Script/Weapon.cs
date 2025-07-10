using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기의 행동 로직을 관리하는 클래스입니다.
/// 무기 ID에 따라 근접(회전) 또는 원거리(발사) 공격을 수행합니다.
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Info")]
    [Tooltip("무기 고유 ID")]
    public int id;
    [Tooltip("오브젝트 풀에서 사용할 프리팹 ID")]
    public int prefabId;
    [Tooltip("공격력")]
    public float damage;
    [Tooltip("무기 개수 또는 관통 횟수")]
    public int count;
    [Tooltip("공격 속도 또는 회전 속도")]
    public float speed;

    private float timer; // 원거리 무기 발사 타이머
    private Player player; // 플레이어 참조

    void Awake()
    {
        player = GameManager.instance.player;
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        switch (id)
        {
            case 0: // ID 0: 근접 무기 (회전)
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            default: // 그 외: 원거리 무기 (발사)
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }
    }

    /// <summary>
    /// 무기 레벨업 시 호출됩니다. 데미지와 개수를 업데이트합니다.
    /// </summary>
    /// <param name="damage">증가된 데미지</param>
    /// <param name="count">증가된 개수</param>
    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count = count; // 변경된 부분: += 에서 = 으로 변경

        if (id == 0)
        {
            Deploy();
        }
    }

    /// <summary>
    /// ItemData를 기반으로 무기를 초기화합니다.
    /// </summary>
    /// <param name="data">무기 정보가 담긴 ItemData</param>
    public void Init(ItemData data)
    {
        // 기본 정보 설정
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        id = data.itemId;
        damage = data.baseDamage;
        count = data.baseCount;

        for (int i = 0; i < GameManager.instance.pool.prefabs.Length; i++)
        {
            if (data.projectile == GameManager.instance.pool.prefabs[i])
            {
                prefabId = i;
                break;
            }
        }

        // 무기 ID에 따라 특성 초기화
        switch (id)
        {
            case 0: // 근접 무기
                speed = 150; // 기본 속도만 설정, 보너스는 Gear에서 적용
                Deploy();
                break;
            default: // 원거리 무기
                speed = 0.5f; // 기본 속도만 설정, 보너스는 Gear에서 적용
                break;
        }

        // 이 무기가 생성될 때, 플레이어에 이미 장착된 모든 Gear의 효과를 적용받습니다.
        Gear[] gears = player.GetComponentsInChildren<Gear>();
        foreach (Gear gear in gears)
        {
            gear.ApplyGearEffectTo(this);
        }
    }

    void Deploy()
    {
        for (int i = 0; i < count; i++)
        {
            Transform bullet;

            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * i / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);

            bullet.GetComponent<Bullet>().Init(damage, -100, Vector3.zero);
        }
    }

    void Fire()
    {
        if (!player.scanner.nearestTarget) return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        bullet.GetComponent<Bullet>().Init(damage, count, dir);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}