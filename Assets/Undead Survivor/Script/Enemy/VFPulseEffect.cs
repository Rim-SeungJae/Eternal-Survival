using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// VF Pulse 공격의 시각적 이펙트를 관리하는 클래스입니다.
/// 경고 단계와 폭발 단계로 나뉘어 실행됩니다.
/// </summary>
public class VFPulseEffect : MonoBehaviour
{
    [Header("Effect Components")]
    [SerializeField] private SpriteRenderer warningRangeRenderer; // 경고 범위 표시용 스프라이트
    [SerializeField] private ParticleSystem explosionParticles1; // 폭발 파티클 시스템 1
    [SerializeField] private ParticleSystem explosionParticles2; // 폭발 파티클 시스템 2
    [SerializeField] private SpriteRenderer explosionSprite; // 폭발 스프라이트 (선택사항)
    
    [Header("Range Settings")]
    public Transform rangeIndicator; // 공격 범위 지시자 (에디터에서 연결)
    
    [Header("Warning Phase Settings")]
    [SerializeField] private float warningPulseSpeed = 2f; // 경고 펄스 속도
    [SerializeField] private float warningScaleMin = 0.8f; // 경고 최소 크기
    [SerializeField] private float warningScaleMax = 1.2f; // 경고 최대 크기
    
    [Header("Explosion Settings")]
    [SerializeField] private float explosionForceMultiplier = 10f; // 폭발 힘 배수
    [SerializeField] private AnimationCurve velocityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 속도 커브
    
    private Sequence warningSequence;
    private Sequence explosionSequence;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule1;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule2;
    private bool isInitialized = false;
    private Vector3 originalWarningScale; // 경고 스프라이트의 원래 스케일 저장
    
    void Awake()
    {
        InitializeComponents();
    }
    
    /// <summary>
    /// RangeIndicator를 기준으로 실제 공격 범위를 계산합니다.
    /// </summary>
    public float GetActualAttackRange()
    {
        if (rangeIndicator == null) return 4f; // 기본값
        
        // 로컬 거리 계산
        float localDistance = rangeIndicator.localPosition.magnitude;
        
        // 현재 스케일 적용 (경고 스프라이트의 스케일)
        float currentScale = warningRangeRenderer != null ? warningRangeRenderer.transform.localScale.x : 1f;
        float actualRange = localDistance * currentScale;
        
        return actualRange;
    }
    
    /// <summary>
    /// 컴포넌트들을 초기화합니다.
    /// </summary>
    private void InitializeComponents()
    {
        // 경고 스프라이트 초기 설정
        if (warningRangeRenderer != null)
        {
            warningRangeRenderer.gameObject.SetActive(false);
            // 원래 스케일 저장
            originalWarningScale = warningRangeRenderer.transform.localScale;
        }
        
        // 파티클 시스템 초기 설정
        if (explosionParticles1 != null)
        {
            explosionParticles1.gameObject.SetActive(false);
            velocityModule1 = explosionParticles1.velocityOverLifetime;
            velocityModule1.enabled = true;
        }
        
        if (explosionParticles2 != null)
        {
            explosionParticles2.gameObject.SetActive(false);
            velocityModule2 = explosionParticles2.velocityOverLifetime;
            velocityModule2.enabled = true;
        }
        
        // 폭발 스프라이트 초기 설정
        if (explosionSprite != null)
        {
            explosionSprite.gameObject.SetActive(false);
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// 원하는 반지름에 맞는 스케일을 계산합니다.
    /// </summary>
    private float CalculateScaleForRadius(float targetRadius)
    {
        if (rangeIndicator == null) 
        {
            // RangeIndicator가 없으면 기본 방식 사용 (지름 방식)
            return targetRadius * 2f;
        }
        
        // RangeIndicator의 로컬 거리를 기준으로 스케일 계산
        float baseDistance = rangeIndicator.localPosition.magnitude;
        
        if (baseDistance <= 0f)
        {
            Debug.LogWarning("VFPulseEffect: RangeIndicator distance is zero or negative");
            return targetRadius * 2f; // fallback
        }
        
        // 목표 반지름을 달성하기 위해 필요한 스케일
        float requiredScale = targetRadius / baseDistance;
        
        // 원래 스케일을 고려하여 최종 스케일 계산
        float finalScale = requiredScale * originalWarningScale.x;
        
        return finalScale;
    }
    
    /// <summary>
    /// 경고 단계를 시작합니다.
    /// </summary>
    public void StartWarningPhase(float duration, float radius, AnimationCurve pulseCurve)
    {
        if (!isInitialized) InitializeComponents();
        
        if (warningRangeRenderer == null)
        {
            Debug.LogWarning("VFPulseEffect: Warning range renderer is not assigned");
            return;
        }
        
        // 오브젝트가 활성화되어 있는지 확인
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("VFPulseEffect: Cannot start warning phase - GameObject is inactive");
            return;
        }
        
        // 경고 스프라이트 활성화
        warningRangeRenderer.gameObject.SetActive(true);
        
        // 범위 크기 설정 (RangeIndicator 기준으로 조정)
        float targetScale = CalculateScaleForRadius(radius);
        warningRangeRenderer.transform.localScale = Vector3.one * targetScale;
        
        // 펄스 애니메이션 시작
        StartWarningPulseAnimation(duration, pulseCurve);
        
        Debug.Log($"VFPulse Warning Phase started - Duration: {duration}s, Radius: {radius}");
    }
    
    /// <summary>
    /// 경고 펄스 애니메이션을 시작합니다.
    /// </summary>
    private void StartWarningPulseAnimation(float duration, AnimationCurve pulseCurve)
    {
        if (warningRangeRenderer == null) return;
        
        // 기존 애니메이션 정리
        warningSequence?.Kill();
        
        // 펄스 애니메이션 시퀀스 생성
        warningSequence = DOTween.Sequence();
        
        // 크기 펄스 효과
        warningSequence.Append(
            warningRangeRenderer.transform.DOScale(warningScaleMax, warningPulseSpeed / 2f)
                .SetEase(Ease.InOutSine)
        );
        warningSequence.Append(
            warningRangeRenderer.transform.DOScale(warningScaleMin, warningPulseSpeed / 2f)
                .SetEase(Ease.InOutSine)
        );
        
        // 반복 설정
        int loopCount = Mathf.CeilToInt(duration / warningPulseSpeed);
        warningSequence.SetLoops(loopCount);
        
        // 투명도 페이드 (마지막 0.2초 동안)
        DOTween.To(() => warningRangeRenderer.color.a, a => {
            Color color = warningRangeRenderer.color;
            color.a = a;
            warningRangeRenderer.color = color;
        }, 0f, 0.2f).SetDelay(duration - 0.2f);
    }
    
    /// <summary>
    /// 폭발 단계를 시작합니다.
    /// </summary>
    public void StartExplosionPhase(float duration)
    {
        if (!isInitialized) InitializeComponents();
        
        // 경고 단계 정리
        StopWarningPhase();
        
        // 폭발 이펙트들 시작
        StartExplosionParticles(duration);
        StartExplosionSprite(duration);
        
        Debug.Log($"VFPulse Explosion Phase started - Duration: {duration}s");
    }
    
    /// <summary>
    /// 경고 단계를 중단합니다.
    /// </summary>
    private void StopWarningPhase()
    {
        warningSequence?.Kill();
        
        if (warningRangeRenderer != null)
        {
            warningRangeRenderer.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 폭발 파티클 시스템들을 시작합니다.
    /// </summary>
    private void StartExplosionParticles(float duration)
    {
        // 메인 오브젝트가 활성화되어 있는지 확인
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("VFPulseEffect: Cannot start particles - GameObject is inactive");
            return;
        }
        
        // 파티클 시스템 1 활성화
        if (explosionParticles1 != null)
        {
            explosionParticles1.gameObject.SetActive(true);
            explosionParticles1.Play();
        }
        
        // 파티클 시스템 2 활성화
        if (explosionParticles2 != null)
        {
            explosionParticles2.gameObject.SetActive(true);
            explosionParticles2.Play();
        }
        
        // Velocity over Lifetime 모듈로 폭발 효과 구현
        StartCoroutine(AnimateParticleVelocity(duration));
    }
    
    /// <summary>
    /// 파티클들의 속도를 애니메이션으로 제어하여 폭발 효과를 만듭니다.
    /// </summary>
    private IEnumerator AnimateParticleVelocity(float duration)
    {
        bool hasParticles = (explosionParticles1 != null && velocityModule1.enabled) || 
                           (explosionParticles2 != null && velocityModule2.enabled);
        
        if (!hasParticles) yield break;
        
        float elapsed = 0f;
        float halfDuration = duration * 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // 0 → 1 → 0으로 변화하는 Y축 속도
            float velocityY;
            if (elapsed < halfDuration)
            {
                // 첫 번째 절반: 0에서 1로 증가
                velocityY = (elapsed / halfDuration) * explosionForceMultiplier;
            }
            else
            {
                // 두 번째 절반: 1에서 0으로 감소
                velocityY = ((duration - elapsed) / halfDuration) * explosionForceMultiplier;
            }
            
            // 커브 적용
            velocityY *= velocityCurve.Evaluate(normalizedTime);
            
            // 두 파티클 시스템에 동일하게 적용
            if (explosionParticles1 != null && velocityModule1.enabled)
            {
                velocityModule1.y = new ParticleSystem.MinMaxCurve(velocityY);
            }
            
            if (explosionParticles2 != null && velocityModule2.enabled)
            {
                velocityModule2.y = new ParticleSystem.MinMaxCurve(velocityY);
            }
            
            yield return null;
        }
        
        // 최종적으로 두 시스템 다 속도를 0으로 설정
        if (explosionParticles1 != null && velocityModule1.enabled)
        {
            velocityModule1.y = new ParticleSystem.MinMaxCurve(0f);
        }
        
        if (explosionParticles2 != null && velocityModule2.enabled)
        {
            velocityModule2.y = new ParticleSystem.MinMaxCurve(0f);
        }
    }
    
    /// <summary>
    /// 폭발 스프라이트 효과를 시작합니다.
    /// </summary>
    private void StartExplosionSprite(float duration)
    {
        if (explosionSprite == null) return;
        
        // 폭발 스프라이트 활성화
        explosionSprite.gameObject.SetActive(true);
        
        // 폭발 애니메이션 시퀀스
        explosionSequence?.Kill();
        explosionSequence = DOTween.Sequence();
        
        // 크기 확대 + 페이드 아웃
        explosionSequence.Append(
            explosionSprite.transform.DOScale(Vector3.one * 2f, duration * 0.3f)
                .SetEase(Ease.OutQuad)
        );
        explosionSequence.Join(
            explosionSprite.DOFade(0f, duration)
                .SetEase(Ease.OutQuad)
        );
    }
    
    /// <summary>
    /// 이펙트를 정리하고 풀에 반환합니다.
    /// </summary>
    public void ReturnToPool()
    {
        // 코루틴 먼저 정지 (비활성화 전에)
        StopAllCoroutines();
        
        // 모든 애니메이션 정리
        warningSequence?.Kill();
        explosionSequence?.Kill();
        
        // 파티클 시스템 정지
        if (explosionParticles1 != null)
        {
            explosionParticles1.Stop();
            explosionParticles1.Clear();
        }
        
        if (explosionParticles2 != null)
        {
            explosionParticles2.Stop();
            explosionParticles2.Clear();
        }
        
        // 컴포넌트 상태 초기화
        ResetEffect();
        
        // 풀로 반환
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
    /// 이펙트 상태를 초기화합니다.
    /// </summary>
    private void ResetEffect()
    {
        // 경고 스프라이트 초기화
        if (warningRangeRenderer != null)
        {
            warningRangeRenderer.gameObject.SetActive(false);
            warningRangeRenderer.transform.localScale = originalWarningScale; // 원래 스케일로 복구
            
            // 색상 초기화 (알파값 복구)
            Color color = warningRangeRenderer.color;
            color.a = 1f;
            warningRangeRenderer.color = color;
        }
        
        // 폭발 스프라이트 초기화
        if (explosionSprite != null)
        {
            explosionSprite.gameObject.SetActive(false);
            explosionSprite.transform.localScale = Vector3.one;
            
            // 색상 초기화 (알파값 복구)
            Color color = explosionSprite.color;
            color.a = 1f;
            explosionSprite.color = color;
        }
        
        // 파티클 시스템 초기화
        if (explosionParticles1 != null)
        {
            explosionParticles1.gameObject.SetActive(false);
            
            // Velocity Over Lifetime 초기화
            if (velocityModule1.enabled)
            {
                velocityModule1.y = new ParticleSystem.MinMaxCurve(0f);
            }
        }
        
        if (explosionParticles2 != null)
        {
            explosionParticles2.gameObject.SetActive(false);
            
            // Velocity Over Lifetime 초기화
            if (velocityModule2.enabled)
            {
                velocityModule2.y = new ParticleSystem.MinMaxCurve(0f);
            }
        }
    }
    
    void OnDisable()
    {
        // 비활성화 시 애니메이션 정리
        warningSequence?.Kill();
        explosionSequence?.Kill();
        
        // 코루틴도 정지
        StopAllCoroutines();
    }
    
    void OnDestroy()
    {
        // 메모리 누수 방지
        warningSequence?.Kill();
        explosionSequence?.Kill();
    }
}