using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기의 행동 로직을 관리하는 클래스입니다.
/// ItemData에 정의된 속성에 따라 다양한 공격(회전, 발사 등)을 수행합니다.
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int id;
    public int level;
    public ItemData itemData; // 원본 데이터 참조

    // 현재 레벨과 플레이어 스탯이 적용된 최종 능력치 (ModifiableStat으로 변경)
    public ModifiableStat damage; // 피해량
    public ModifiableStat count; // 투사체 개수 또는 관통 횟수
    public ModifiableStat projectileSpeed; // 투사체 속도 또는 회전 속도
    public ModifiableStat duration; // 지속 시간
    public ModifiableStat attackArea; // 공격 범위 (크기 조절용)
    public ModifiableStat cooldown; // 쿨타임

    private float timer; // 공격 쿨타임 타이머
    private Player player; // 플레이어 참조

    void Awake()
    {
        player = GameManager.instance.player;

        // ModifiableStat 인스턴스 초기화
        damage = new ModifiableStat(0);
        count = new ModifiableStat(0);
        projectileSpeed = new ModifiableStat(0);
        duration = new ModifiableStat(0);
        attackArea = new ModifiableStat(0);
        cooldown = new ModifiableStat(0);
    }

    /// <summary>
    /// ItemData를 기반으로 무기를 처음 초기화합니다。
    /// </summary>
    public void Init(ItemData data)
    {
        this.itemData = data;
        this.id = data.itemId;
        this.level = 0;

        name = "Weapon " + data.itemName;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        // 레벨 0 데이터로 능력치 적용
        ApplyLevelData();
    }

    /// <summary>
    /// 무기 레벨업 시 호출됩니다。
    /// </summary>
    public void LevelUp()
    {
        this.level++;
        ApplyLevelData();
    }

    /// <summary>
    /// 현재 레벨에 맞는 데이터를 ItemData에서 가져와 능력치를 갱신합니다.
    /// </summary>
    private void ApplyLevelData()
    {
        // 레벨별 데이터가 없으면 기본값을, 있으면 해당 레벨의 값을 ModifiableStat의 BaseValue에 대입합니다.
        damage.BaseValue = (itemData.damages.Length > level) ? itemData.damages[level] : itemData.damages[0];
        count.BaseValue = (itemData.counts.Length > level) ? itemData.counts[level] : itemData.counts[0];
        projectileSpeed.BaseValue = (itemData.projectileSpeeds.Length > level) ? itemData.projectileSpeeds[level] : itemData.projectileSpeeds[0];
        duration.BaseValue = (itemData.durations.Length > level) ? itemData.durations[level] : itemData.durations[0];
        attackArea.BaseValue = (itemData.areas.Length > level) ? itemData.areas[level] : itemData.areas[0];
        cooldown.BaseValue = (itemData.cooldowns.Length > level) ? itemData.cooldowns[level] : itemData.cooldowns[0];

        // 레벨업 시, 무기 타입에 따라 즉시 변경사항을 적용해야 하는 경우 처리
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
            // TODO: Magic 타입 등 다른 무기 로직 추가
        }
    }

    /// <summary>
    /// 근접 무기를 배치하거나 레벨업 시 갱신합니다。
    /// </summary>
    void Deploy()
    {
        
        // 모든 자식들을 List로 복사해서 처리
        List<Transform> children = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
        
        // 복사한 List를 기반으로 반복 처리
        for(int i = 0; i < children.Count; i++)
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

            // 근접 무기는 무한 관통이므로 관통 횟수를 -100으로 설정합니다。
            // attackArea는 투사체의 크기(scale)로 전달합니다.
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
        
        // 원거리 무기는 관통 횟수로 'count' 속성을 사용합니다.
        // attackArea는 투사체의 크기(scale)로 전달합니다.
        bullet.GetComponent<Bullet>().Init(damage.Value, (int)count.Value, duration.Value, projectileSpeed.Value, attackArea.Value, dir);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}


