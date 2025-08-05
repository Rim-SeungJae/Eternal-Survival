using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Three Calamities 광역 공격의 시각적 이펙트를 관리하는 클래스입니다.
/// </summary>
public class CalamitiesExplosionEffect : MonoBehaviour
{
    private float damage;
    private float duration;
    private float radius;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    [Header("# Range Detection")]
    [Tooltip("범위 표시용 Square 스프라이트")]
    public GameObject rangeSquare;
    
    [Header("# Explosion Effect")]
    [Tooltip("폭발 이펙트 오브젝트")]
    public GameObject explosionEffect;
    
    [Header("# Activation Effect")]
    [Tooltip("활성화 이펙트 오브젝트 (마법진 빛남)")]
    public GameObject activationEffect;
    [Tooltip("활성화 애니메이션 재생 시간")]
    public float activationDuration = 0.2f;
    
    [Header("# Final Effect")]
    [Tooltip("최종 폭발 시 적을 끌어당기는 힘")]
    public float pullForce = 5f;
    
    [Tooltip("끌어당기는 효과 지속시간")]
    public float pullDuration = 0.3f;
    
    [Header("# Visual Effects")]
    
    [Tooltip("이펙트 페이드아웃 시간")]
    public float fadeOutDuration = 0.5f;

    [Header("# Arrow Effects")]
    [Tooltip("화살 오브젝트들 (프리팹에서 설정)")]
    public GameObject[] arrowObjects = new GameObject[4];
    
    [Tooltip("화살 크기")]
    public float arrowScale = 0.5f;
    
    [Tooltip("화살 낙하 시간")]
    public float arrowFallDuration = 0.3f;
    
    [Tooltip("화살 시작 높이")]
    public float arrowStartHeight = 2f;
    
    [Tooltip("화살 간격 시간")]
    public float arrowInterval = 0.15f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 화살 오브젝트들 초기 비활성화
        InitializeArrowObjects();
        
    }

    private void InitializeArrowObjects()
    {
        for (int i = 0; i < arrowObjects.Length; i++)
        {
            if (arrowObjects[i] != null)
            {
                arrowObjects[i].SetActive(false);
                
                // 화살 스프라이트 설정
                SpriteRenderer arrowRenderer = arrowObjects[i].GetComponent<SpriteRenderer>();
                if (arrowRenderer != null)
                {
                    arrowRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
                }
            }
        }
    }

    public void Init(float damage, float duration, float radius)
    {
        this.damage = damage;
        this.duration = duration;
        this.radius = radius;
        spriteRenderer.enabled = true;
        explosionEffect.SetActive(false);
        activationEffect.SetActive(false);
        
        // 화살 오브젝트들 초기화
        DeactivateArrowObjects();
        
        // 이펙트 시작
        StartCoroutine(ExplosionEffectRoutine());
    }

    private IEnumerator ExplosionEffectRoutine()
    {
        // 초기 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // 화살 이펙트 시작
        StartCoroutine(CreateArrowEffects());
        
        // 지속 시간 대기 (이 시간 동안 범위 내 적들을 추적)
        yield return new WaitForSeconds(duration * 0.7f);
        
        // 최종 폭발 효과: 범위 내 적들을 끌어당기고 데미지 적용
        StartCoroutine(FinalExplosionEffect());

        // 화살 오브젝트 비활성화
        DeactivateArrowObjects();
        
        // 활성화 이펙트 시작 (마법진 빛남)
        StartCoroutine(ShowActivationEffect());
        
        // 활성화 애니메이션 완료 대기
        yield return new WaitForSeconds(activationDuration);
        
        // 폭발 이펙트 표시
        ShowExplosionEffect();
        
        // 폭발 이펙트 페이드아웃
        yield return new WaitForSeconds(fadeOutDuration);
        
        // 오브젝트 비활성화
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

    private IEnumerator CreateArrowEffects()
    {
        // 시계방향으로 순차적 화살 낙하
        for (int i = 0; i < 4; i++)
        {
            if (arrowObjects[i] != null)
            {
                CreateArrowAtPosition(i);
                yield return new WaitForSeconds(arrowInterval);
            }
        }
    }

    private void CreateArrowAtPosition(int cornerIndex)
    {
        GameObject arrowObj = arrowObjects[cornerIndex];
        if (arrowObj == null) return;
        
        // 기존 DOTween 애니메이션 완전히 정리
        arrowObj.transform.DOKill(true);
        
        // 화살 활성화
        arrowObj.SetActive(true);
        
        // 초기 위치 설정 (로컬 좌표 사용)
        Vector3 originalLocalPos = arrowObj.transform.localPosition;
        Vector3 startLocalPos = originalLocalPos + Vector3.up * arrowStartHeight;
        arrowObj.transform.localPosition = startLocalPos;
        arrowObj.transform.localScale = Vector3.one * arrowScale;
        
        // 화살 낙하 애니메이션 (로컬 좌표로 목표 위치 계산)
        Vector3 targetLocalPos = originalLocalPos;
        
        Debug.Log($"{cornerIndex}th arrow {startLocalPos} -> {targetLocalPos} ");
        
        // 낙하 애니메이션 (Sequence 대신 직접 DOLocalMove 사용)
        arrowObj.transform.DOLocalMove(targetLocalPos, arrowFallDuration)
            .SetEase(Ease.InQuad)
            .SetId($"arrow_{cornerIndex}"); // ID 설정으로 추적 가능
        
    }

    private void DeactivateArrowObjects()
    {
        for (int i = 0; i < arrowObjects.Length; i++)
        {
            if (arrowObjects[i] != null)
            {
                // DOTween 애니메이션 정리
                arrowObjects[i].transform.DOKill();
                arrowObjects[i].SetActive(false);
            }
        }
    }



    private IEnumerator FinalExplosionEffect()
    {
        // Square 범위 내 모든 적들을 찾아서 데미지 적용
        List<Enemy> enemiesInRange = FindEnemiesInRange();
        
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                // 적을 중앙으로 끌어당기는 효과
                StartCoroutine(PullEnemyToCenter(enemy));
                
                // 데미지 적용
                enemy.TakeDamage(damage);
            }
        }
        
        yield return new WaitForSeconds(pullDuration);
    }

    private List<Enemy> FindEnemiesInRange()
    {
        List<Enemy> enemies = new List<Enemy>();
        
        // Square 스프라이트의 실제 범위를 사용해서 탐지
        SpriteRenderer squareRenderer = rangeSquare.GetComponent<SpriteRenderer>();
        if (squareRenderer == null)
        {
            Debug.LogWarning("SquareRenderer is null, using radius-based detection");
            return FindEnemiesInRange(); // 재귀 호출로 fallback
        }
        
        Bounds squareBounds = squareRenderer.bounds;
        Vector2 center = squareBounds.center;
        Vector2 size = squareBounds.size;
        
        // Square 범위 내의 모든 콜라이더 찾기
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                    Debug.Log($"Enemy found: {enemy.name} at position {enemy.transform.position} (Square bounds: {squareBounds})");
                }
            }
        }
        
        return enemies;
    }

    private IEnumerator ShowActivationEffect()
    {
        // 원래 스프라이트 숨기기
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // 활성화 이펙트 표시 및 애니메이션 시작
        if (activationEffect != null)
        {
            activationEffect.SetActive(true);
            
            // Animator 컴포넌트 가져오기
            Animator activationAnimator = activationEffect.GetComponent<Animator>();
            if (activationAnimator != null)
            {
                // 애니메이션 완료 대기
                yield return new WaitForSeconds(activationDuration);
            }
            else
            {
                Debug.LogWarning("ActivationEffect has no Animator component");
                yield return new WaitForSeconds(activationDuration);
            }
        }
        else
        {
            Debug.LogWarning("ActivationEffect is null, skipping activation animation");
            yield return new WaitForSeconds(activationDuration);
        }
    }

    private void ShowExplosionEffect()
    {
        // 활성화 이펙트 비활성화
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
        
        // 폭발 이펙트 표시
        if (explosionEffect != null)
        {
            explosionEffect.SetActive(true);
            
            // 폭발 이펙트의 SpriteRenderer 가져오기
            SpriteRenderer explosionRenderer = explosionEffect.GetComponent<SpriteRenderer>();
            explosionRenderer.color = new Color(1, 1, 1, 1);
            if (explosionRenderer != null)
            {
                // 폭발 이펙트 페이드아웃
                explosionRenderer.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
            }
        }
    }

    private IEnumerator PullEnemyToCenter(Enemy enemy)
    {
        if (enemy == null) yield break;
        
        Vector3 centerPosition = transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 pullDirection = (centerPosition - enemyPosition).normalized;
        
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            // 순간적으로 강한 힘으로 끌어당기기
            enemyRb.AddForce(pullDirection * pullForce, ForceMode2D.Impulse);
        }
        
        yield return new WaitForSeconds(pullDuration);
    }

    void OnDisable()
    {
        // DOTween 애니메이션 정리
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
        }
        transform.DOKill();
        
        // 활성화 이펙트 정리
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
        
        // 폭발 이펙트 정리
        if (explosionEffect != null)
        {
            SpriteRenderer explosionRenderer = explosionEffect.GetComponent<SpriteRenderer>();
            if (explosionRenderer != null)
            {
                explosionRenderer.DOKill();
            }
            explosionEffect.SetActive(false);
        }
        
        // 화살 오브젝트 정리
        DeactivateArrowObjects();
    }
} 