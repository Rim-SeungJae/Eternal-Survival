using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Star Fragment 무기의 메인 로직을 관리하는 클래스입니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class StarFragmentWeapon : WeaponBase
{
    private float timer;
    private StarFragmentData starFragmentData;

    public override void Init(ItemData data)
    {
        base.Init(data);
        this.starFragmentData = data as StarFragmentData;
        if (this.starFragmentData == null)
        {
            Debug.LogError("StarFragmentWeapon에 할당된 ItemData가 StarFragmentData 타입이 아닙니다!");
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
    /// 사정거리 내 적을 대상으로 메테오 공격을 실행합니다.
    /// </summary>
    private void Attack()
    {
        if (starFragmentData == null) return;

        // 사정거리 내 적 찾기
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy targetEnemy = null;
        float closestDistance = starFragmentData.attackRange;

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                targetEnemy = enemy;
            }
        }

        if (targetEnemy == null) return;

        Vector3 targetPosition = targetEnemy.transform.position;

        // 1. 경고 표시 이펙트 생성 및 초기화
        GameObject warningEffect = GameManager.instance.pool.Get(starFragmentData.warningProjectileTag);
        if (warningEffect != null)
        {
            warningEffect.transform.position = targetPosition;

            StarFragmentWarning warningLogic = warningEffect.GetComponent<StarFragmentWarning>();
            if (warningLogic != null)
            {
                warningLogic.Init(attackArea.Value);
                warningEffect.SetActive(true);
                warningLogic.StartWaveGrowth(duration.Value); // WeaponData.duration을 경고 시간으로 사용
            }
        }

        // 2. 메테오 이펙트 생성 및 초기화 (경고 시간 후 낙하하도록)
        StartCoroutine(LaunchMeteorAfterDelay(targetPosition, duration.Value));
    }

    /// <summary>
    /// 경고 시간 후 메테오를 발사합니다.
    /// </summary>
    private IEnumerator LaunchMeteorAfterDelay(Vector3 targetPosition, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject meteorEffect = GameManager.instance.pool.Get(starFragmentData.projectileTag);
        if (meteorEffect != null)
        {
            meteorEffect.transform.position = targetPosition;
            meteorEffect.transform.localScale = Vector3.one * attackArea.Value;

            StarFragmentMeteor meteorLogic = meteorEffect.GetComponent<StarFragmentMeteor>();
            if (meteorLogic != null)
            {
                meteorLogic.Init(damage.Value, attackArea.Value, targetPosition);
                meteorEffect.SetActive(true);
                meteorLogic.StartMeteorSequence();
            }

        }
    }
} 