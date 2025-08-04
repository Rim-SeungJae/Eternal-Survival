using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 진화된 Yuki Weapon의 로직을 관리하는 클래스입니다.
/// YukiWeapon을 상속받아 기본 로직을 재사용하고,
/// 반원 형태의 강화된 공격 시스템을 추가합니다.
/// </summary>
public class YukiWeaponEvo : YukiWeapon
{
    [Header("# Evolution Settings")]
    [Tooltip("반원 공격의 최대 범위")]
    public float semicircleRadius = 5f;
    
    [Tooltip("반원 공격의 차오름 지속 시간")]
    public float chargeDuration = 1.5f;
    
    [Tooltip("베기 공격의 데미지 배율")]
    public float slashAttackMultiplier = 1.2f;
    
    [Tooltip("마크 폭발의 데미지 배율")]
    public float markExplosionMultiplier = 1.5f;
    
    [Tooltip("마크 지속 시간")]
    public float markDuration = 2.0f;

    /// <summary>
    /// 진화된 공격 패턴을 실행합니다.
    /// 기존 단일 타겟 공격 대신 반원 범위 공격을 시전합니다.
    /// </summary>
    protected override void Attack()
    {
        // 사거리 내 모든 적 찾기
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackArea.Value);
        
        if (enemiesInRange.Length == 0) return;

        // 가장 가까운 적 찾기 (반원의 중심 방향 결정용)
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
            // 반원 공격 시전
            LaunchSemicircleAttack(closestEnemy.transform.position);
        }
    }

    /// <summary>
    /// 반원 형태의 공격을 시전합니다.
    /// </summary>
    /// <param name="targetPosition">공격 방향의 기준이 되는 타겟 위치</param>
    private void LaunchSemicircleAttack(Vector3 targetPosition)
    {
        // 타겟 방향 계산
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        
        WeaponData data = itemData as WeaponData;
        // 반원 공격 이펙트 생성
        GameObject semicircleEffect = GameManager.instance.pool.Get(data.projectileTag);
        if (semicircleEffect != null)
        {
            // 플레이어 위치에서 시전
            semicircleEffect.transform.position = transform.position;
            
            // 타겟 방향으로 회전 (Effect가 위쪽을 기본 방향으로 하므로 -90도 보정)
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
            semicircleEffect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // 공격 범위에 따른 스케일 조정
            semicircleEffect.transform.localScale = Vector3.one * (semicircleRadius / 3f); // 기본 스케일 3 기준
            
            // 이펙트 초기화
            YukiWeaponEvoEffect effectComponent = semicircleEffect.GetComponent<YukiWeaponEvoEffect>();
            if (effectComponent != null)
            {
                semicircleEffect.SetActive(true);
                effectComponent.Init(
                    damage.Value * slashAttackMultiplier,    // 베기 공격 피해량
                    damage.Value * markExplosionMultiplier,  // 마크 폭발 피해량
                    semicircleRadius,                        // 공격 범위
                    chargeDuration,                          // 차오름 지속 시간
                    markDuration                             // 마크 지속 시간
                );
            }
        }

        // 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Melee);
    }
}