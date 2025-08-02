using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유키의 기본무기 로직을 관리하는 클래스입니다.
/// 사거리 내 가장 가까운 적에게 즉시 데미지를 주고 애니메이션을 재생합니다.
/// </summary>
public class YukiWeapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머
    private Animator animator; // 애니메이터 컴포넌트

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
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
    /// 사거리 내 가장 가까운 적을 찾아 즉시 공격합니다.
    /// </summary>
    private void Attack()
    {
        // 사거리 내 모든 적 찾기
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackArea.Value);
        
        if (enemiesInRange.Length == 0) return;

        // 가장 가까운 적 찾기
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        if (closestEnemy != null)
        {
            // 애니메이션 재생
            PlayAttackAnimation(closestEnemy.transform.position);
            
            // 데미지 적용
            closestEnemy.TakeDamage(damage.Value);
        }
    }

    /// <summary>
    /// 공격 애니메이션을 적 위치에서 재생합니다.
    /// </summary>
    /// <param name="targetPosition">타겟 위치</param>
    private void PlayAttackAnimation(Vector3 targetPosition)
    {
        WeaponData data = itemData as WeaponData;
        // 이펙트 프리팹 생성
        GameObject effectPrefab = GameManager.instance.pool.Get(data.projectileTag);
        if (effectPrefab != null)
        {
            effectPrefab.transform.position = targetPosition;
            effectPrefab.SetActive(true);
            
            // 이펙트 초기화
            YukiWeaponEffect effect = effectPrefab.GetComponent<YukiWeaponEffect>();
            if (effect != null)
            {
                effect.Init(damage.Value, attackArea.Value, duration.Value);
            }
        }
    }
} 