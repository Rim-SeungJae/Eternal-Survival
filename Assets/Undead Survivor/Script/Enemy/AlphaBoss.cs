using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Alpha 보스 몬스터입니다. BossBase를 상속받아 반원 모양의 차징 공격을 구현합니다.
/// </summary>
public class AlphaBoss : BossBase
{
    [Header("Alpha Boss Settings")]
    [PoolTagSelector] public string chargeEffectPoolTag = "AlphaSpecialAttack"; // 차징 이펙트 풀 태그
    public float chargeDuration = 3f; // 차징 시간
    public float swirlWaitDuration = 0.1f; // swirl 효과 대기 시간
    public LayerMask playerLayer = 1 << 6; // 플레이어 레이어
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableAnimationDebug = true; // 애니메이션 디버깅 활성화
    
    private AlphaChargeEffect currentChargeEffect;
    
    /// <summary>
    /// 반원 모양의 차징 공격을 수행합니다.
    /// </summary>
    protected override void PerformSpecialAttack()
    {
        if (GameManager.instance?.player == null || isDead || IsPerformingSpecialAttack()) return;
        
        StartCoroutine(ChargeAttackSequence());
    }
    
    /// <summary>
    /// 차징 공격 시퀀스를 실행합니다.
    /// </summary>
    private IEnumerator ChargeAttackSequence()
    {
        // 1. 보스 완전 고정 (모든 움직임과 외부 힘 차단)
        StartSpecialAttackImmobilization();
        
        // 2. 플레이어 방향 계산
        Vector2 playerDirection = GetDirectionToPlayer();
        
        // 3. 차징 이펙트 생성 및 시작
        GameObject effectObject = CreateChargeEffect(playerDirection);
        if (effectObject != null)
        {
            currentChargeEffect = effectObject.GetComponent<AlphaChargeEffect>();
            if (currentChargeEffect != null)
            {
                currentChargeEffect.StartCharging(chargeDuration, playerDirection);
            }
        }
        
        // 4. 애니메이션 상태 설정
        anim.SetBool("Special", true);
        DebugAnimationState("Special Bool 설정됨");
        
        // 5. 차징 시간만큼 대기 (애니메이션 상태 모니터링)
        float elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            if (enableAnimationDebug)
            {
                DebugAnimationState($"차징 중 ({elapsed:F1}s)");
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 6. 공격 실행
        ExecuteSemiCircleAttack(playerDirection);
        
        // 7. Swirl 효과가 완료될 때까지 대기
        yield return new WaitForSeconds(swirlWaitDuration);
        
        // 8. 애니메이션 상태 복구
        anim.SetBool("Special", false);
        DebugAnimationState("Special Bool 해제됨");
        
        // 9. 이펙트는 자동으로 풀로 반환됨 (StopCharging 호출 안 함)
        currentChargeEffect = null;
        
        // 10. 보스 이동 복구 (마지막에 호출)
        EndSpecialAttackImmobilization();
        
        Debug.Log($"{bossData.bossName}이(가) 반원 공격을 완료했습니다!");
    }
    
    /// <summary>
    /// 차징 이펙트를 PoolManager에서 가져옵니다.
    /// </summary>
    private GameObject CreateChargeEffect(Vector2 direction)
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
            Debug.LogWarning($"PoolManager에서 '{chargeEffectPoolTag}' 태그의 오브젝트를 찾을 수 없습니다!");
        }
        
        return effect;
    }
    
    /// <summary>
    /// 반원 모양 범위 공격을 실행합니다.
    /// </summary>
    private void ExecuteSemiCircleAttack(Vector2 attackDirection)
    {
        // 반원 범위 내의 플레이어 검색
        Vector3 attackPosition = transform.position;
        
        // 차징 이펙트에서 실제 공격 범위 가져오기 (RangeIndicator 기반)
        float attackRange = (currentChargeEffect != null) ? 
            currentChargeEffect.GetActualAttackRange() : bossData.specialAttackRange;
        
        // 범위 내 모든 콜라이더 검색
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, attackRange, playerLayer);
        
        foreach (var collider in hitColliders)
        {
            Vector2 directionToTarget = (collider.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(attackDirection, directionToTarget);
            
            // 반원 범위 내에 있는지 확인 (90도 이내)
            if (angle <= 90f)
            {
                Player player = collider.GetComponent<Player>();
                if (player != null)
                {
                    // 플레이어에게 데미지 적용
                    GameManager.instance.health -= bossData.specialAttackDamage;
                    
                    // 넉백 효과
                    Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDirection = directionToTarget;
                        playerRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                    }
                    
                    Debug.Log("Alpha 보스의 반원 공격이 플레이어를 명중했습니다!");
                }
            }
        }
        
        // 공격 효과음
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
    }
    
    /// <summary>
    /// 보스가 죽을 때 진행 중인 특수 공격을 중단합니다.
    /// </summary>
    protected override void OnBossDeath()
    {
        // 진행 중인 특수 공격 중단
        if (IsPerformingSpecialAttack())
        {
            StopAllCoroutines();
            
            if (currentChargeEffect != null)
            {
                currentChargeEffect.StopCharging();
                currentChargeEffect = null;
            }
            
            // 고정 상태 해제
            EndSpecialAttackImmobilization();
        }
        
        base.OnBossDeath();
    }
    
    /// <summary>
    /// 특수 공격 중에는 Hit 애니메이션을 방지하기 위해 TakeDamage를 오버라이드합니다.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;

        // 특수 공격 중에는 넉백과 Hit 애니메이션을 방지
        bool canPlayHitAnimation = !IsPerformingSpecialAttack();
        
        // 시간 정지 중이 아니고 특수 공격 중이 아닐 때만 넉백 효과를 적용
        bool canKnockback = !GameManager.instance.isTimeStopped && !IsPerformingSpecialAttack();
        
        if (canKnockback) StartCoroutine(KnockBack());

        if (health > 0)
        {
            // 특수 공격 중이 아닐 때만 Hit 애니메이션 재생
            if (canPlayHitAnimation)
            {
                anim.SetTrigger("Hit");
            }
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            // 사망 처리
            isLive = false;
            isDead = true;
            OnBossDeath();
        }
        
        // 체력바 업데이트 (BossBase에서 처리하지 않으므로 여기서 직접 처리)
        if (healthBar != null)
        {
            healthBar.UpdateHealth(health, maxHealth);
        }
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
    
    /// <summary>
    /// 애니메이션 상태를 디버깅용으로 출력합니다.
    /// </summary>
    private void DebugAnimationState(string context)
    {
        if (!enableAnimationDebug || anim == null) return;
        
        // 현재 애니메이터 상태 정보
        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
        
        // 모든 Bool 파라미터 확인
        bool specialBool = anim.GetBool("Special");
        bool deadBool = anim.GetBool("Dead");
        
        // 현재 재생 중인 애니메이션 이름
        string stateName = GetAnimationStateName(currentState.fullPathHash);
        
        // 정규화된 시간 (0~1, 1 이상이면 루프)
        float normalizedTime = currentState.normalizedTime;
        
        // 애니메이션 길이와 속도
        float animationLength = currentState.length;
        float animatorSpeed = anim.speed;
        
        // 전환 정보
        bool isInTransition = anim.IsInTransition(0);
        string transitionInfo = "";
        if (isInTransition)
        {
            AnimatorTransitionInfo transition = anim.GetAnimatorTransitionInfo(0);
            transitionInfo = $"Transitioning (Progress: {transition.normalizedTime:F3})";
        }
        
        // 로그 출력
        Debug.Log($"[Alpha Animation Debug] {context}\n" +
                 $"- State: {stateName}\n" +
                 $"- Special Bool: {specialBool}\n" +
                 $"- Dead Bool: {deadBool}\n" +
                 $"- Normalized Time: {normalizedTime:F3}\n" +
                 $"- Length: {animationLength:F2}s\n" +
                 $"- Animator Speed: {animatorSpeed}\n" +
                 $"- Is Looping: {currentState.loop}\n" +
                 $"- Is In Transition: {isInTransition}\n" +
                 $"- {transitionInfo}");
    }
    
    /// <summary>
    /// 애니메이션 상태 해시를 이름으로 변환 (대략적)
    /// </summary>
    private string GetAnimationStateName(int stateHash)
    {
        // 주요 상태들의 해시값을 확인 (디버깅용)
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Special"))
            return "Special";
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Walk"))
            return "Walk";
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Hit"))
            return "Hit";
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Dead"))
            return "Dead";
        else
            return $"Unknown (Hash: {stateHash})";
    }
}