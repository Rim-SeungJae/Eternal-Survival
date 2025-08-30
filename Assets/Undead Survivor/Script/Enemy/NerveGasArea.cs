using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 위클라인의 신경가스 영역을 관리하는 클래스입니다.
/// 파티클 시스템과 스프라이트 페이드 효과를 조합하여 신경가스 연출을 구현합니다.
/// PoolManager와 호환됩니다.
/// </summary>
public class NerveGasArea : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private ParticleSystem activationParticles; // 활성화 파티클 시스템
    [SerializeField] private SpriteRenderer gasAreaSprite; // 신경가스 영역 스프라이트
    
    [Header("Range Settings")]
    public Transform rangeIndicator; // 실제 공격 범위 지시자 (에디터에서 연결)
    
    // 상태 관리
    private bool isActive = false;
    private bool isFading = false;
    private Tween currentFadeTween;
    
    // 원본 색상 저장 (페이드 효과용)
    private Color originalSpriteColor;
    
    void Awake()
    {
        // 컴포넌트 자동 할당
        if (activationParticles == null)
            activationParticles = GetComponentInChildren<ParticleSystem>();
        
        if (gasAreaSprite == null)
            gasAreaSprite = GetComponentInChildren<SpriteRenderer>();
        
        // 원본 색상 저장
        if (gasAreaSprite != null)
        {
            originalSpriteColor = gasAreaSprite.color;
        }
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        // 풀에서 재사용될 때마다 초기화
        isActive = false;
        isFading = false;
        
        // 기존 트윈 정리
        if (currentFadeTween != null)
        {
            currentFadeTween.Kill();
            currentFadeTween = null;
        }
        
        // 스프라이트 초기화 (완전 투명)
        if (gasAreaSprite != null)
        {
            Color transparentColor = originalSpriteColor;
            transparentColor.a = 0f;
            gasAreaSprite.color = transparentColor;
            gasAreaSprite.enabled = true;
        }
        
        // 파티클 시스템 초기화
        if (activationParticles != null)
        {
            activationParticles.Stop();
            activationParticles.Clear();
        }
    }
    
    void OnDisable()
    {
        // 비활성화 시 상태 정리
        isActive = false;
        isFading = false;
        
        // 트윈 정리
        if (currentFadeTween != null)
        {
            currentFadeTween.Kill();
            currentFadeTween = null;
        }
    }
    
    /// <summary>
    /// RangeIndicator를 기준으로 실제 공격 범위를 계산합니다. (Scale 고려)
    /// </summary>
    public float GetActualGasAreaRange()
    {
        if (rangeIndicator == null) return 6f; // 기본값
        
        // 로컬 거리 계산
        float localDistance = rangeIndicator.localPosition.magnitude;
        
        // 현재 스케일 적용
        float actualRange = localDistance * transform.localScale.x;
        
        return actualRange;
    }
    
    /// <summary>
    /// 활성화 파티클 효과를 재생합니다.
    /// </summary>
    public void PlayActivationParticles()
    {
        if (activationParticles == null)
        {
            Debug.LogWarning("NerveGasArea: Activation particles not assigned");
            return;
        }
        
        // 파티클 재생 (설정은 에디터에서 미리 구성됨)
        activationParticles.Play();
        
        Debug.Log("NerveGasArea: Activation particles played");
    }
    
    /// <summary>
    /// 신경가스 영역 스프라이트를 페이드인합니다.
    /// </summary>
    public void StartFadeIn(float duration)
    {
        if (gasAreaSprite == null)
        {
            Debug.LogWarning("NerveGasArea: Gas area sprite not assigned");
            return;
        }
        
        isFading = true;
        
        // 기존 트윈 정리
        if (currentFadeTween != null)
        {
            currentFadeTween.Kill();
        }
        
        // 투명 상태에서 시작
        Color startColor = originalSpriteColor;
        startColor.a = 0f;
        gasAreaSprite.color = startColor;
        
        // 페이드인 트윈
        currentFadeTween = gasAreaSprite.DOColor(originalSpriteColor, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isActive = true;
                isFading = false;
                Debug.Log("NerveGasArea: Fade in completed");
            });
        
        Debug.Log($"NerveGasArea: Starting fade in over {duration} seconds");
    }
    
    /// <summary>
    /// 신경가스 영역 스프라이트를 페이드아웃합니다.
    /// </summary>
    public void StartFadeOut(float duration)
    {
        if (gasAreaSprite == null)
        {
            Debug.LogWarning("NerveGasArea: Gas area sprite not assigned");
            ReturnToPool();
            return;
        }
        
        isActive = false;
        isFading = true;
        
        // 기존 트윈 정리
        if (currentFadeTween != null)
        {
            currentFadeTween.Kill();
        }
        
        // 투명 상태로 페이드아웃
        Color transparentColor = originalSpriteColor;
        transparentColor.a = 0f;
        
        currentFadeTween = gasAreaSprite.DOColor(transparentColor, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                isFading = false;
                ReturnToPool();
                Debug.Log("NerveGasArea: Fade out completed and returned to pool");
            });
        
        Debug.Log($"NerveGasArea: Starting fade out over {duration} seconds");
    }
    
    /// <summary>
    /// 신경가스 영역이 활성화되어 있는지 확인합니다.
    /// </summary>
    public bool IsActive()
    {
        return isActive && gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// 현재 페이드 중인지 확인합니다.
    /// </summary>
    public bool IsFading()
    {
        return isFading;
    }
    
    /// <summary>
    /// 강제로 정리하고 풀로 반환합니다.
    /// </summary>
    public void ForceCleanup()
    {
        // 트윈 정리
        if (currentFadeTween != null)
        {
            currentFadeTween.Kill();
            currentFadeTween = null;
        }
        
        // 파티클 정지
        if (activationParticles != null)
        {
            activationParticles.Stop();
            activationParticles.Clear();
        }
        
        // 상태 초기화
        isActive = false;
        isFading = false;
        
        ReturnToPool();
    }
    
    /// <summary>
    /// 풀로 반환합니다.
    /// </summary>
    private void ReturnToPool()
    {
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null && GameManager.instance?.pool != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            // Poolable이 없다면 비활성화
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 에디터에서 신경가스 영역을 시각화합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 실제 공격 범위 표시
        float actualRange = GetActualGasAreaRange();
        
        Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, actualRange);
        
        // 테두리 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, actualRange);
        
        // RangeIndicator 시각화
        if (rangeIndicator != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 indicatorWorldPos = transform.TransformPoint(rangeIndicator.localPosition);
            Gizmos.DrawWireSphere(indicatorWorldPos, 0.2f);
            Gizmos.DrawLine(transform.position, indicatorWorldPos);
        }
        
        // 활성화 상태 표시
        if (Application.isPlaying)
        {
            if (isActive)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
            else if (isFading)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
        }
    }
}