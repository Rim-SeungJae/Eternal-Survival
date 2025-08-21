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
    [SerializeField] private ParticleSystem chargeParticles; // 충전 파티클 시스템
    
    [Header("Range Settings")]
    public Transform rangeIndicator; // 공격 범위 지시자 (에디터에서 연결)
    
    [Header("Warning Phase Settings")]
    [SerializeField] private float warningPulseSpeed = 0.6f; // 경고 펄스 속도 (한 번의 깜빡임 주기)
    
    [Header("Explosion Settings")]
    // 파티클 시스템 자체 설정을 사용하므로 런타임 조작 불필요
    
    private Sequence warningSequence;
    private Sequence explosionSequence;
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
        // RangeIndicator의 로컬 거리 계산
        float localDistance = rangeIndicator.localPosition.magnitude;
        
        // 원래 스케일을 그대로 사용하여 실제 범위 계산
        float actualRange = localDistance;
        
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
        
        if (chargeParticles != null)
        {
            chargeParticles.gameObject.SetActive(false);
        }

        // 파티클 시스템 초기 설정
        if (explosionParticles1 != null)
        {
            explosionParticles1.gameObject.SetActive(false);
        }
        
        if (explosionParticles2 != null)
        {
            explosionParticles2.gameObject.SetActive(false);
        }

        
        isInitialized = true;
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

        if(chargeParticles == null)
        {
            Debug.LogWarning("VFPulseEffect: Charge particles are not assigned");
            return;
        }
        
        // 경고 스프라이트 활성화 (원래 스케일 그대로 사용)
        warningRangeRenderer.gameObject.SetActive(true);
        chargeParticles.gameObject.SetActive(true);
        
        
        // 초기 투명도 설정 (깜빡임 시작값)
        Color initialColor = warningRangeRenderer.color;
        initialColor.a = 0.3f;
        warningRangeRenderer.color = initialColor;
        
        // 펄스 애니메이션 시작
        StartWarningPulseAnimation(duration, pulseCurve);
        
        Debug.Log($"VFPulse Warning Phase started - Duration: {duration}s, Using original scale: {originalWarningScale}");
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
        
        // 투명도 깜빡임 효과 (0.3 → 1.0 → 0.3)
        warningSequence.Append(
            warningRangeRenderer.DOFade(1f, warningPulseSpeed / 2f)
                .SetEase(Ease.InOutSine)
        );
        warningSequence.Append(
            warningRangeRenderer.DOFade(0.3f, warningPulseSpeed / 2f)
                .SetEase(Ease.InOutSine)
        );
        
        // 무한 반복 설정 (duration 기간 동안 계속 깜빡임)
        warningSequence.SetLoops(-1);
        
        // duration 후에 시퀀스 정지하고 페이드 아웃
        DOTween.Sequence()
            .AppendInterval(duration - 0.2f)
            .AppendCallback(() => {
                warningSequence?.Kill(); // 깜빡임 정지
                warningRangeRenderer.DOFade(0f, 0.2f); // 페이드 아웃
            });
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
        if (chargeParticles != null)
        {
            chargeParticles.gameObject.SetActive(false);
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
        
        // 폭발 효과: 파티클 재생 후 fade out
        StartCoroutine(AnimateParticleExplosion(duration));
    }
    
    /// <summary>
    /// 파티클 폭발 효과: 파티클 재생 후 천천히 fade out
    /// </summary>
    private IEnumerator AnimateParticleExplosion(float duration)
    {
        if (explosionParticles1 == null && explosionParticles2 == null) yield break;
        
        // fade out 시작 시점 설정 (30% 지점부터)
        float fadeStartTime = duration * 0.3f;
        yield return new WaitForSeconds(fadeStartTime);
        
        // 파티클 시스템에 fade out 효과 적용
        SetupParticleFadeOut(explosionParticles1, duration - fadeStartTime);
        SetupParticleFadeOut(explosionParticles2, duration - fadeStartTime);
        
        // 나머지 시간 대기
        yield return new WaitForSeconds(duration - fadeStartTime);
    }
    
    /// <summary>
    /// 파티클 시스템에 fade out 효과를 적용합니다.
    /// </summary>
    private void SetupParticleFadeOut(ParticleSystem particles, float fadeDuration)
    {
        if (particles == null) return;
        
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        // 알파 값을 1에서 0으로 fade out하는 그라디언트 생성
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        
        colorOverLifetime.color = gradient;
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
        
        
        // 파티클 시스템 초기화
        if (explosionParticles1 != null)
        {
            explosionParticles1.gameObject.SetActive(false);
        }
        
        if (explosionParticles2 != null)
        {
            explosionParticles2.gameObject.SetActive(false);
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