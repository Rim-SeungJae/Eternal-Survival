using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Alpha 보스 몬스터입니다. BossBase를 상속받아 Arc Blade 공격을 구현합니다.
/// 새로운 특수공격 시스템을 사용하여 다중 공격을 지원합니다.
/// </summary>
public class AlphaBoss : BossBase
{
    [Header("Alpha Boss Legacy Settings")]
    [SerializeField] private bool useLegacySystem = false; // 레거시 시스템 사용 여부
    
    // 레거시 시스템용 변수들 (호환성을 위해 유지)
    [PoolTagSelector] public string chargeEffectPoolTag = "AlphaSpecialAttack";
    public float chargeDuration = 3f;
    public float swirlWaitDuration = 0.1f;
    public LayerMask playerLayer = 1 << 6;
    
    private AlphaChargeEffect currentChargeEffect; // 레거시 시스템용
    
    /// <summary>
    /// 특수 공격을 수행합니다. (레거시 지원용 오버라이드)
    /// </summary>
    protected override void PerformSpecialAttack()
    {
        if (useLegacySystem)
        {
            // 레거시 시스템 사용
            if (GameManager.instance?.player == null || isDead || IsPerformingSpecialAttack()) return;
            StartCoroutine(ArcBladeAttackSequence());
        }
        else
        {
            // 새 시스템 사용 (BossBase에서 처리)
            base.PerformSpecialAttack();
        }
    }
    
    /// <summary>
    /// 레거시 아크 블레이드 공격 시퀀스를 실행합니다.
    /// </summary>
    private IEnumerator ArcBladeAttackSequence()
    {
        // 1. 보스 완전 고정 (모든 움직임과 외부 힘 차단)
        StartSpecialAttackImmobilization();
        
        // 2. 플레이어 방향 계산
        Vector2 playerDirection = GetDirectionToPlayer();
        
        // 3. 아크 블레이드 이펙트 생성 및 시작
        GameObject effectObject = CreateArcBladeEffect(playerDirection);
        if (effectObject != null)
        {
            currentChargeEffect = effectObject.GetComponent<AlphaChargeEffect>();
            if (currentChargeEffect != null)
            {
                currentChargeEffect.StartArcBladeCharging(chargeDuration, playerDirection);
            }
        }
        
        // 4. 애니메이션 상태 설정
        anim.SetBool("Special", true);
        
        // 5. 차징 시간만큼 대기 (애니메이션 상태 모니터링)
        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 6. 아크 블레이드 공격 실행
        ExecuteArcBladeAttack(playerDirection);
        
        // 7. Swirl 효과가 완료될 때까지 대기
        yield return new WaitForSeconds(swirlWaitDuration);
        
        // 8. 애니메이션 상태 복구
        anim.SetBool("Special", false);
        
        // 9. 이펙트는 자동으로 풀로 반환됨 (StopCharging 호출 안 함)
        currentChargeEffect = null;
        
        // 10. 보스 이동 복구 (마지막에 호출)
        EndSpecialAttackImmobilization();
        
    }
    
    /// <summary>
    /// 아크 블레이드 이펙트를 PoolManager에서 가져옵니다.
    /// </summary>
    private GameObject CreateArcBladeEffect(Vector2 direction)
    {
        
        // PoolManager에서 차징 이펙트 가져오기
        GameObject effect = GameManager.instance.pool.Get(chargeEffectPoolTag);
        
        if (effect != null)
        {
            effect.transform.position = transform.position;
            effect.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"PoolManager에서 '{chargeEffectPoolTag}' 태그의 아크 블레이드 이펙트를 찾을 수 없습니다!");
        }
        
        return effect;
    }
    
    /// <summary>
    /// 아크 블레이드 범위 공격을 실행합니다.
    /// </summary>
    private void ExecuteArcBladeAttack(Vector2 attackDirection)
    {
        // 반원 범위 내의 플레이어 검색
        Vector3 attackPosition = transform.position;
        
        // 아크 블레이드 이펙트에서 실제 공격 범위 가져오기 (RangeIndicator 기반)
        float attackRange = (currentChargeEffect != null) ? 
            currentChargeEffect.GetActualAttackRange() : bossData.specialAttackRange;
        
        // 범위 내 모든 콜라이더 검색
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, attackRange, playerLayer);
        
        foreach (var collider in hitColliders)
        {
            Vector2 directionToTarget = (collider.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(attackDirection, directionToTarget);
            
            // 아크 블레이드 범위 내에 있는지 확인 (90도 이내)
            if (angle <= 90f)
            {
                Player player = collider.GetComponent<Player>();
                if (player != null)
                {
                    // 플레이어에게 아크 블레이드 데미지 적용
                    GameManager.instance.health -= bossData.specialAttackDamage;
                    
                    // 아크 블레이드 넉백 효과
                    Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = directionToTarget;
                        playerRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                    }
                    
                }
            }
        }
        
        // 아크 블레이드 공격 효과음
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
    }
    
    /// <summary>
    /// 보스가 죽을 때 진행 중인 아크 블레이드 공격을 중단합니다.
    /// </summary>
    protected override void OnBossDeath()
    {
        // 진행 중인 아크 블레이드 공격 중단
        if (IsPerformingSpecialAttack())
        {
            StopAllCoroutines();
            
            if (currentChargeEffect != null)
            {
                currentChargeEffect.StopArcBladeCharging();
                currentChargeEffect = null;
            }

            anim.SetBool("Special", false);
            
            // 고정 상태 해제
            EndSpecialAttackImmobilization();
        }
        
        base.OnBossDeath();
    }

    public override void Dead()
    {
        GameManager.instance.UnregisterEnemy(this);
        GetComponent<SpriteRenderer>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            Poolable poolable = GetComponent<Poolable>();
            if (poolable != null)
            {
                GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        });
    }

}