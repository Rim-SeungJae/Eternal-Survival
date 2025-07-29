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
    
    // 중복 데미지 방지를 위한 적 추적
    private HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
    
    [Header("# Visual Effects")]
    [Tooltip("이펙트 시작 크기")]
    public float startScale = 0.1f;
    
    [Tooltip("이펙트 최대 크기")]
    public float maxScale = 1f;
    
    [Tooltip("이펙트 페이드아웃 시간")]
    public float fadeOutDuration = 0.5f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Init(float damage, float duration, float radius)
    {
        this.damage = damage;
        this.duration = duration;
        this.radius = radius;
        
        // 중복 데미지 방지용 HashSet 초기화
        damagedEnemies.Clear();
        
        // 이펙트 시작
        StartCoroutine(ExplosionEffectRoutine());
    }

    private IEnumerator ExplosionEffectRoutine()
    {
        // 초기 설정
        transform.localScale = Vector3.one * startScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // 크기 확대 애니메이션
        transform.DOScale(maxScale, duration * 0.3f).SetEase(Ease.OutBack);
        
        // 지속 시간 대기
        yield return new WaitForSeconds(duration * 0.7f);
        
        // 페이드아웃
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
        }
        
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                // 중복 데미지 방지를 위해 적을 HashSet에 추가
                damagedEnemies.Add(enemy);
                
                // 데미지 적용
                enemy.TakeDamage(damage);
                
                // 적에게 넉백 효과 적용 (선택사항)
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    void OnDisable()
    {
        // DOTween 애니메이션 정리
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
        }
        transform.DOKill();
        
        // HashSet 정리
        damagedEnemies.Clear();
    }
} 