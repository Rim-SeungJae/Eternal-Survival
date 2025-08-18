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
    public GameObject swirlObject;

    [Tooltip("잔상 지속 시간")]
    public float afterImageDuration = 0.3f;
    
    [Header("# Swirl Effect Settings")]
    [Tooltip("Swirl 효과 지속 시간")]
    public float swirlDuration = 0.5f;
    [Tooltip("Swirl 색상")]
    public Color swirlColor = Color.white;

    [Header("# Private Components")]
    private SpriteRenderer afterImageRenderer1;
    private SpriteRenderer afterImageRenderer2;
    private SpriteRenderer swirlRenderer;
    private PolygonCollider2D effectCollider;
    private Animator animator;
    private YukiChargeEffect chargeEffect;
    
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
    
    // DOTween 애니메이션 참조
    private Sequence currentSwirlSequence;

    void Awake()
    {
        animator = effectObject.GetComponent<Animator>();
        effectCollider = effectObject.GetComponent<PolygonCollider2D>();
        chargeEffect = effectObject.GetComponent<YukiChargeEffect>();
        
        afterImageRenderer1 = afterImageObject1.GetComponent<SpriteRenderer>();
        afterImageRenderer2 = afterImageObject2.GetComponent<SpriteRenderer>();
        
        // SwirlObject에서 SpriteRenderer 가져오기
        if (swirlObject != null)
        {
            swirlRenderer = swirlObject.GetComponent<SpriteRenderer>();
            
        }
        
        // 초기에는 AfterImage와 Swirl 비활성화
        if (afterImageObject1 != null)
        {
            afterImageObject1.SetActive(false);
            afterImageObject2.SetActive(false);
        }
        
        if (swirlObject != null)
        {
            swirlObject.SetActive(false);
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
        
        // 1. Effect 오브젝트 먼저 활성화 (코루틴 시작을 위해 필수)
        effectObject.SetActive(true);
        
        // 2. 새로운 스크립트 기반 차오름 효과 시작
        if (chargeEffect != null)
        {
            chargeEffect.StartCharging(chargeDuration);
            Debug.Log($"스크립트 기반 차오름 효과 시작! 지속 시간: {chargeDuration}초");
        }
        else
        {
            // 차오름 효과가 없는 경우 기존 애니메이션 사용
            if (animator != null)
            {
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                float animationClipLength = 1.0f;
                
                foreach (AnimationClip clip in clips)
                {
                    if (clip.name.Contains("Yuki Weapon evo"))
                    {
                        animationClipLength = clip.length;
                        break;
                    }
                }
                
                float animationSpeed = animationClipLength / chargeDuration;
                animator.speed = animationSpeed;
                
                Debug.Log($"기존 애니메이션 사용: 길이 {animationClipLength}초, 속도: {animationSpeed}");
            }
        }
        
        // 3. 차오름 지속 시간 동안 대기
        yield return new WaitForSeconds(chargeDuration);
        
        // 4. 애니메이션 속도 정상화
        if (animator != null)
        {
            animator.speed = 1.0f;
        }
        
        // 5. 베기 공격 실행 (즉시 피해 + 마크 부여)
        ExecuteSlashAttack();
        
        // 6. Swirl 효과 시작 (베기 공격 직후)
        StartCoroutine(PlaySwirlEffect());
        
        // 5. 마크 폭발까지 대기
        yield return new WaitForSeconds(markDuration);

        // afterImage2 활성화
        if(afterImageObject2 != null)
        {
            afterImageObject2.SetActive(true);
            afterImageRenderer2.DOFade(0, afterImageDuration);
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
            afterImageRenderer1.DOFade(0, afterImageDuration);
        }
        
        // 반원 범위 내 모든 적에게 베기 공격
        SlashEnemiesInSemicircle();
        
        // 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);

        // 이펙트 비활성화
        effectObject.SetActive(false);
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
                    
                }
            }
        }
        
        // 충돌 검사 완료 후 PolygonCollider 비활성화
        effectCollider.enabled = false;
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
    
    /// <summary>
    /// RadialFillController를 사용한 Swirl 공격 효과를 재생합니다.
    /// </summary>
    private IEnumerator PlaySwirlEffect()
    {
        if (swirlRenderer == null || swirlObject == null) yield break;
        
        // Swirl 활성화
        swirlObject.SetActive(true);
        
        // RadialFillUtils를 사용한 Swirl 효과
        if (swirlRenderer != null && swirlRenderer.material != null && swirlRenderer.material.HasProperty("_FillAmount"))
        {
            // 색상 설정
            if (swirlRenderer.material.HasProperty("_Color"))
            {
                swirlRenderer.material.SetColor("_Color", swirlColor);
            }
            
            // 시계방향 설정 (유키는 시계방향)
            swirlRenderer.material.SetFloat("_Clockwise", 0f);
            
            // DOTween 애니메이션 실행
            bool effectCompleted = false;
            currentSwirlSequence = RadialFillUtils.PlaySwirlEffect(
                swirlRenderer.material, 
                swirlRenderer, 
                swirlDuration, 
                0.3f, 
                () =>
                {
                    // Swirl 완료 후 정리
                    swirlObject.SetActive(false);
                    
                    // 머티리얼 초기 색상 복구
                    if (swirlRenderer.material != null && swirlRenderer.material.HasProperty("_Color"))
                    {
                        swirlRenderer.material.SetColor("_Color", swirlColor);
                    }
                    
                    effectCompleted = true;
                }
            );
            
            // 완료까지 대기
            yield return new WaitUntil(() => effectCompleted);
        }
        else
        {
            // 기존 방식 fallback
            yield return StartCoroutine(PlaySwirlEffectLegacy());
        }
    }
    
    /// <summary>
    /// 레거시 Swirl 효과 (RadialFillController가 없을 때)
    /// </summary>
    private IEnumerator PlaySwirlEffectLegacy()
    {
        // 기존 머티리얼 활용 - radial fill 초기화
        Material swirlMaterial = swirlRenderer.material;
        if (swirlMaterial != null)
        {
            swirlMaterial.SetFloat("_FillAmount", 0f);
            swirlMaterial.SetFloat("_Clockwise", 0f); // 시계방향 (유키는 시계방향)
            swirlMaterial.SetColor("_Color", swirlColor);
        }
        else
        {
            swirlRenderer.color = swirlColor;
        }
        
        float elapsed = 0f;
        
        // 1단계: 빠른 채움 효과 (휘두르는 효과)
        float fillDuration = swirlDuration * 0.3f; // 전체 시간의 30%로 빠르게 채움
        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fillDuration;
            
            if (swirlMaterial != null)
            {
                // 빠르게 채우기 (0에서 1까지)
                swirlMaterial.SetFloat("_FillAmount", progress);
            }
            
            yield return null;
        }
        
        // 2단계: 페이드아웃하면서 사라지기
        elapsed = 0f;
        float fadeOutDuration = swirlDuration * 0.7f; // 나머지 70% 시간으로 페이드아웃
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            
            // 점진적 페이드아웃
            Color currentColor = swirlColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            
            if (swirlMaterial != null && swirlMaterial.HasProperty("_Color"))
            {
                swirlMaterial.SetColor("_Color", currentColor);
            }
            else
            {
                swirlRenderer.color = currentColor;
            }
            
            yield return null;
        }
        
        // Swirl 비활성화
        swirlObject.SetActive(false);
        
        // 머티리얼 초기 색상 복구
        if (swirlMaterial != null && swirlMaterial.HasProperty("_Color"))
        {
            swirlMaterial.SetColor("_Color", swirlColor);
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