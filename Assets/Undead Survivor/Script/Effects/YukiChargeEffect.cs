using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 유키 무기의 차오름 효과를 시계방향으로 점진적으로 채워지는 방식으로 구현하는 컴포넌트입니다.
/// 어두운 스프라이트 위에 밝은 스프라이트가 시계방향으로 점진적으로 드러나는 효과를 제공합니다.
/// Static RadialFillUtils를 사용하여 구현되었습니다.
/// </summary>
public class YukiChargeEffect : MonoBehaviour
{
    [Header("# Sprite Settings")]
    [Tooltip("어두운 버전 스프라이트 (배경)")]
    public SpriteRenderer darkSprite;
    
    [Tooltip("밝은 버전 스프라이트 (전경)")]
    public SpriteRenderer brightSprite;
    
    [Header("# Fill Settings")]
    [Tooltip("채워짐 시작 각도 (반원의 bottom center 기준)")]
    public float startAngle = 0f; // 반원의 오른쪽 끝에서 시작
    
    [Tooltip("채워짐 방향 (true = 시계방향, false = 반시계방향)")]
    public bool clockwise = true;
    
    [Tooltip("중심점 계산 방식")]
    public CenterPointType centerPointType = CenterPointType.SpritePivot;
    
    [Tooltip("수동 중심점 (centerPointType이 Manual일 때 사용)")]
    public Vector2 manualCenterPoint = new Vector2(0.5f, 0.5f);
    
    [Header("# Animation Settings")]
    [Tooltip("차오름 지속 시간")]
    public float chargeDuration = 2f;
    
    [Tooltip("차오름 완료 후 유지 시간")]
    public float holdDuration = 0.5f;
    
    [Header("# Material")]
    public Material brightSpriteMaterial;
    
    private float currentFillAmount = 0f;
    private bool isCharging = false;
    
    // DOTween 애니메이션 참조
    private Tween currentChargeTween;

    void Awake()
    {
        InitializeComponents();
        SetupMaterial();
    }
    
    /// <summary>
    /// 컴포넌트들을 초기화합니다.
    /// </summary>
    private void InitializeComponents()
    {
        // SpriteRenderer가 설정되지 않은 경우 자동으로 찾기
        if (darkSprite == null)
        {
            darkSprite = transform.Find("DarkSprite")?.GetComponent<SpriteRenderer>();
        }
        
        if (brightSprite == null)
        {
            brightSprite = transform.Find("BrightSprite")?.GetComponent<SpriteRenderer>();
        }
    
        
        // 렌더링 순서 설정
        if (darkSprite != null && brightSprite != null)
        {
            darkSprite.sortingOrder = 0;
            brightSprite.sortingOrder = 1;
        }
    }
    
    /// <summary>
    /// 에디터에서 할당한 머티리얼을 설정합니다.
    /// </summary>
    private void SetupMaterial()
    {
        
        // 에디터에서 할당한 머티리얼이 있으면 사용
        if (brightSpriteMaterial != null && brightSprite != null)
        {
            brightSprite.material = brightSpriteMaterial;
            Debug.Log("에디터에서 할당한 RadialFill 머티리얼을 사용합니다.");
        }
        else if (brightSprite != null)
        {
            // 머티리얼이 할당되지 않은 경우 기본 머티리얼 사용
            brightSpriteMaterial = brightSprite.material;
            Debug.LogWarning("RadialFill 머티리얼이 할당되지 않았습니다. 기본 페이드 효과를 사용합니다.");
        }
    }
    
    /// <summary>
    /// 차오름 효과를 시작합니다.
    /// </summary>
    /// <param name="duration">차오름 지속 시간 (선택사항)</param>
    public void StartCharging(float duration = -1f)
    {
        if (duration > 0)
        {
            chargeDuration = duration;
        }
        
        if (isCharging)
        {
            StopAllCoroutines();
            currentChargeTween?.Kill();
        }
        
        StartCoroutine(ChargeSequence());
    }
    
    /// <summary>
    /// 차오름 효과를 즉시 중단합니다.
    /// </summary>
    public void StopCharging()
    {
        StopAllCoroutines();
        currentChargeTween?.Kill();
        isCharging = false;
        
        // Fill Amount 리셋
        if (brightSpriteMaterial != null)
        {
            RadialFillUtils.SetFillAmount(brightSpriteMaterial, 0f);
        }
    }
    
    /// <summary>
    /// 차오름 시퀀스를 관리합니다.
    /// </summary>
    private IEnumerator ChargeSequence()
    {
        isCharging = true;
        
        // RadialFillUtils를 사용한 차오름 효과
        if (brightSpriteMaterial != null && brightSpriteMaterial.HasProperty("_FillAmount"))
        {
            // RadialFill 설정
            RadialFillUtils.SetupRadialFill(
                brightSpriteMaterial,
                centerPointType,
                brightSprite?.sprite,
                manualCenterPoint,
                clockwise,
                startAngle
            );
            
            // DOTween 애니메이션 실행
            bool chargeCompleted = false;
            currentChargeTween = RadialFillUtils.PlayChargingEffect(brightSpriteMaterial, chargeDuration, () =>
            {
                chargeCompleted = true;
            });
            
            // 차오름 완료까지 대기
            yield return new WaitUntil(() => chargeCompleted);
        }
        else
        {
            // 기존 방식 fallback
            yield return StartCoroutine(LegacyChargeSequence());
        }
        
        // 완료 후 유지
        if (holdDuration > 0)
        {
            yield return new WaitForSeconds(holdDuration);
        }
        
        isCharging = false;
    }
    
    /// <summary>
    /// 레거시 차오름 방식 (RadialFill 셰이더가 없을 때)
    /// </summary>
    private IEnumerator LegacyChargeSequence()
    {
        // 초기화
        SetFillAmount(0f);
        
        // 차오름 단계
        float elapsedTime = 0f;
        while (elapsedTime < chargeDuration)
        {
            float progress = elapsedTime / chargeDuration;
            SetFillAmount(progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 완료 상태로 설정
        SetFillAmount(1f);
    }
    
    /// <summary>
    /// 채우기 진행도를 설정합니다. (레거시 방식)
    /// </summary>
    /// <param name="fillAmount">0~1 사이의 채우기 진행도</param>
    private void SetFillAmount(float fillAmount)
    {
        currentFillAmount = Mathf.Clamp01(fillAmount);
        
        if (brightSpriteMaterial != null && brightSpriteMaterial.HasProperty("_FillAmount"))
        {
            RadialFillUtils.SetFillAmount(brightSpriteMaterial, currentFillAmount);
        }
        else if (brightSpriteMaterial != null)
        {
            // 셰이더가 없는 경우 간단한 알파 페이드 사용
            Color color = brightSprite.color;
            color.a = currentFillAmount;
            brightSprite.color = color;
        }
    }
    
    /// <summary>
    /// 현재 채우기 진행도를 반환합니다.
    /// </summary>
    public float GetFillAmount()
    {
        return currentFillAmount;
    }
    
    /// <summary>
    /// 차오름 중인지 확인합니다.
    /// </summary>
    public bool IsCharging()
    {
        return isCharging;
    }
    
    void OnDestroy()
    {
        // DOTween 정리
        currentChargeTween?.Kill();
    }
    
    /// <summary>
    /// 에디터에서 디버그용으로 사용 (테스트용)
    /// </summary>
    [ContextMenu("Test Charge Effect")]
    private void TestChargeEffect()
    {
        if (Application.isPlaying)
        {
            StartCharging();
        }
    }
    
    /// <summary>
    /// 실시간 디버깅을 위해 Update에서 값 변경사항 적용
    /// </summary>
    void Update()
    {
        // 에디터에서 실시간으로 값을 변경할 수 있도록
        if (Application.isEditor && brightSpriteMaterial != null)
        {
            // 실시간으로 중심점 및 설정 업데이트
            RadialFillUtils.UpdateCenterPoint(brightSpriteMaterial, centerPointType, brightSprite?.sprite, manualCenterPoint);
            
            if (brightSpriteMaterial.HasProperty("_StartAngle"))
                brightSpriteMaterial.SetFloat("_StartAngle", startAngle);
            if (brightSpriteMaterial.HasProperty("_Clockwise"))
                brightSpriteMaterial.SetFloat("_Clockwise", clockwise ? 1f : 0f);
        }
    }
}