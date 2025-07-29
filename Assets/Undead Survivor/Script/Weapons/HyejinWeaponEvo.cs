using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 진화된 Hyejin Weapon의 로직을 관리하는 클래스입니다.
/// HyejinWeapon을 상속받아 기본 투사체 발사 로직을 재사용하고,
/// Three Calamities 스택 시스템을 추가합니다.
/// </summary>
public class HyejinWeaponEvo : HyejinWeapon
{
    [Header("# Three Calamities Settings")]
    [Tooltip("Three Calamities 스택이 발동되는 횟수")]
    public int calamitiesStackThreshold = 3;
    
    [Tooltip("광역 공격 범위")]
    public float calamitiesExplosionRadius = 3f;
    
    [Tooltip("광역 공격 데미지")]
    public float calamitiesExplosionDamage = 50f;
    
    [Tooltip("광역 공격 지속 시간")]
    public float calamitiesExplosionDuration = 2f;


    /// <summary>
    /// 투사체에 Three Calamities 효과를 추가합니다.
    /// </summary>
    protected override void ConfigureBullet(Bullet bulletComponent, Vector3 dir)
    {
        // 기본 투사체 설정
        base.ConfigureBullet(bulletComponent, dir);
        
        // Three Calamities 효과 추가
        HyejinBulletEvo evoBullet = bulletComponent as HyejinBulletEvo;
        if (evoBullet != null)
        {
            evoBullet.SetEvolutionWeapon(this);
        }
        else
        {
            Debug.LogWarning("HyejinWeaponEvo는 HyejinBulletEvo 투사체를 사용해야 합니다.");
        }
    }

    /// <summary>
    /// Three Calamities 스택이 3회에 도달했을 때 광역 공격을 발동합니다.
    /// </summary>
    /// <param name="explosionPosition">광역 공격 위치</param>
    public void TriggerCalamitiesExplosion(Vector3 explosionPosition)
    {
        // 광역 공격 이펙트 생성
        GameObject explosionEffect = GameManager.instance.pool.Get("HyejinWeaponEvo_effect");
        if (explosionEffect != null)
        {
            explosionEffect.transform.position = explosionPosition;
            explosionEffect.transform.localScale = Vector3.one * calamitiesExplosionRadius;
            
            CalamitiesExplosionEffect effectComponent = explosionEffect.GetComponent<CalamitiesExplosionEffect>();
            if (effectComponent != null)
            {
                explosionEffect.SetActive(true);
                effectComponent.Init(calamitiesExplosionDamage, calamitiesExplosionDuration, calamitiesExplosionRadius);
            }
        }

        // 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Melee);
    }
} 