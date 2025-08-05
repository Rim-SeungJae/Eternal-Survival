using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D;
using DG.Tweening;

/// <summary>
/// 유키 진화 무기의 반원 공격 이펙트를 관리하는 클래스입니다.
/// 차오름 → 베기 공격 + 마크 부여 → 마크 폭발의 공격 메커니즘을 처리합니다.
/// </summary>
public class YukiWeaponEvoEffect : MonoBehaviour
{
    [Header("# Mark Settings")]
    [Tooltip("마크 프리팹의 풀 태그")]
    [PoolTagSelector]
    public string markTag;
    
    [Header("# Public Components")]
    public GameObject effectObject;
    public GameObject afterImageObject1;
    public GameObject afterImageObject2;

    [Header("# Private Components")]
    private SpriteRenderer afterImageRenderer1;
    private SpriteRenderer afterImageRenderer2;
    private PolygonCollider2D effectCollider;
    private Animator animator;
    
    [Header("# Attack Settings")]
    private float slashDamage; // 베기 공격의 피해량
    private float markExplosionDamage; // 마크 폭발의 피해량
    private float attackRadius; // 공격 범위
    private float chargeDuration; // 차오름 지속 시간
    private float markDuration; // 마크 지속 시간
    
    [Header("# Follow Target")]
    private Transform playerTransform; // 플레이어를 따라다니기 위한 참조
    
    [Header("# State")]
    private List<Enemy> markedEnemies = new List<Enemy>(); // 마크가 부여된 적들

    void Awake()
    {
        animator = effectObject.GetComponent<Animator>();
        effectCollider = effectObject.GetComponent<PolygonCollider2D>();
        
        afterImageRenderer1 = afterImageObject1.GetComponent<SpriteRenderer>();
        afterImageRenderer2 = afterImageObject2.GetComponent<SpriteRenderer>();

        
        // 초기에는 AfterImage 비활성화
        if (afterImageObject1 != null)
        {
            afterImageObject1.SetActive(false);
            afterImageObject2.SetActive(false);
        }
        
        // 초기에는 PolygonCollider 비활성화 (베기 공격 시에만 활성화)
        if (effectCollider != null)
        {
            effectCollider.enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }
    }

    /// <summary>
    /// 반원 공격 이펙트를 초기화합니다.
    /// </summary>
    /// <param name="slashDmg">베기 공격의 피해량</param>
    /// <param name="markExplosionDmg">마크 폭발의 피해량</param>
    /// <param name="radius">공격 범위</param>
    /// <param name="chargeDur">차오름 지속 시간</param>
    /// <param name="markDur">마크 지속 시간</param>
    public void Init(float slashDmg, float markExplosionDmg, float radius, float chargeDur, float markDur)
    {
        slashDamage = slashDmg;
        markExplosionDamage = markExplosionDmg;
        attackRadius = radius;
        chargeDuration = chargeDur;
        markDuration = markDur;
        
        // 플레이어 참조 가져오기
        playerTransform = GameManager.instance.player.transform;
        
        // 마크된 적 목록 초기화
        markedEnemies.Clear();
        
        // AfterImage 초기화
        if (afterImageObject1 != null)
        {
            afterImageObject1.SetActive(false);
            if (afterImageRenderer1 != null)
            {
                Color resetColor = afterImageRenderer1.color;
                resetColor.a = 1.0f;
                afterImageRenderer1.color = resetColor;
            }
        }

        if(afterImageObject2 != null)
        {
            afterImageObject2.SetActive(false);
            if (afterImageRenderer2 != null)
            {
                Color resetColor = afterImageRenderer2.color;
                resetColor.a = 1.0f;
                afterImageRenderer2.color = resetColor;
            }
        }
        
        // 공격 시퀀스 시작
        StartCoroutine(AttackSequence());
    }

    /// <summary>
    /// 반원 공격의 전체 시퀀스를 관리합니다.
    /// </summary>
    private IEnumerator AttackSequence()
    {
        
        // 1. 차오름 애니메이션 속도 조절 및 시작
        if (animator != null)
        {
            // 현재 애니메이션 클립의 실제 길이를 동적으로 가져오기
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            
            float animationClipLength = 1.0f; // 기본값
            
            // 현재 상태의 애니메이션 클립 길이 찾기
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Contains("Yuki Weapon evo"))
                {
                    animationClipLength = clip.length;
                    break;
                }
            }
            
            // 애니메이션 속도를 chargeDuration에 맞춰 조절
            float animationSpeed = animationClipLength / chargeDuration;
            animator.speed = animationSpeed;
            
            
            Debug.Log($"애니메이션 길이: {animationClipLength}초, 목표 시간: {chargeDuration}초, 속도: {animationSpeed}");
        }
        effectObject.SetActive(true);
        
        // 2. 차오름 지속 시간 동안 대기 (이제 실제 chargeDuration과 동기화됨)
        yield return new WaitForSeconds(chargeDuration);
        
        // 3. 애니메이션 속도 정상화 및 비활성화    
        if (animator != null)
        {
            animator.speed = 1.0f;
        }
        
        // 4. 베기 공격 실행 (즉시 피해 + 마크 부여)
        ExecuteSlashAttack();
        
        // 5. 마크 폭발까지 대기
        yield return new WaitForSeconds(markDuration);

        // afterImage2 활성화
        if(afterImageObject2 != null)
        {
            afterImageObject2.SetActive(true);
            afterImageRenderer2.DOFade(0, 0.3f);
        }

        // afterImage2 페이드아웃 완료까지 대기
        yield return new WaitForSeconds(0.3f);
        
        // 6. 풀에 반환
        DeactivateEffect();
    }

    /// <summary>
    /// 베기 공격을 실행합니다. (즉시 피해 + 마크 부여)
    /// </summary>
    private void ExecuteSlashAttack()
    {
        // 애니메이션 대신 AfterImage 활성화 및 페이드아웃
        if (afterImageObject1 != null)
        {
            afterImageObject1.SetActive(true);
            afterImageRenderer1.DOFade(0, 0.3f);
        }
        
        // 반원 범위 내 모든 적에게 베기 공격
        SlashEnemiesInSemicircle();
        
        // 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);

        // 이펙트 비활성화
        effectObject.SetActive(false);

        Debug.Log($"유키 베기 공격 실행! 마크된 적 수: {markedEnemies.Count}");
    }

    /// <summary>
    /// PolygonCollider2D를 사용해 정확한 반원 모양의 범위 내 모든 적에게 베기 공격을 실행하고 마크를 부여합니다.
    /// </summary>
    private void SlashEnemiesInSemicircle()
    {
        if (effectCollider == null)
        {
            Debug.LogError("Effect PolygonCollider2D가 없습니다!");
            return;
        }
        
        // 베기 공격 시 잠깐 PolygonCollider 활성화
        effectCollider.enabled = true;
        
        // PolygonCollider와 겹치는 모든 콜라이더 찾기 (정확한 반원 모양)
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.AllLayers);
        contactFilter.useTriggers = true;
        
        List<Collider2D> results = new List<Collider2D>();
        int hitCount = Physics2D.OverlapCollider(effectCollider, contactFilter, results);
        
        foreach (Collider2D target in results)
        {
            if (target.CompareTag("Enemy"))
            {
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    // 1. 즉시 베기 피해 적용
                    enemy.TakeDamage(slashDamage);
                    
                    // 2. 마크 부여
                    ApplyMarkToEnemy(enemy);
                    
                    // 3. 마크된 적 목록에 추가
                    markedEnemies.Add(enemy);
                    
                    Debug.Log($"적 {enemy.name}에게 베기 공격! 위치: {enemy.transform.position}");
                }
            }
        }
        
        // 충돌 검사 완료 후 PolygonCollider 비활성화
        effectCollider.enabled = false;
        
        Debug.Log($"반원 PolygonCollider 공격! 히트 수: {hitCount}, 마크된 적 수: {markedEnemies.Count}");
    }

    /// <summary>
    /// 적에게 마크를 부여합니다.
    /// </summary>
    /// <param name="enemy">마크를 부여할 적</param>
    private void ApplyMarkToEnemy(Enemy enemy)
    {
        // 풀에서 마크 프리팹 가져오기
        GameObject markEffect = GameManager.instance.pool.Get(markTag);
        if (markEffect != null)
        {
            // 마크를 적의 위치에 배치
            markEffect.transform.position = enemy.transform.position;
            markEffect.transform.SetParent(enemy.transform);
            markEffect.SetActive(true);
            
            // 마크 컴포넌트 초기화
            YukiWeaponEvoMark markComponent = markEffect.GetComponent<YukiWeaponEvoMark>();
            if (markComponent != null)
            {
                markComponent.InitializeMark(markExplosionDamage, markDuration);
            }
            
            Debug.Log($"적 {enemy.name}에게 유키 마크 프리팹 부여!");
        }
    }


    /// <summary>
    /// 이펙트를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void DeactivateEffect()
    {
        // 상태 초기화
        markedEnemies.Clear();
        
        // 풀에 반환
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 마크된 적들을 초록색으로 표시
        Gizmos.color = Color.green;
        foreach (Enemy enemy in markedEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
            }
        }
    }
}