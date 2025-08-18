using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 중심점 계산 방식을 정의하는 열거형
/// </summary>
public enum CenterPointType
{
    Manual,        // 수동 좌표 지정
    SpritePivot,   // 스프라이트의 pivot 사용
    TextureCenter, // 텍스처 중심 (0.5, 0.5)
    BottomCenter,  // 하단 중심 (0.5, 0)
    TopCenter,     // 상단 중심 (0.5, 1)
    LeftCenter,    // 좌측 중심 (0, 0.5)
    RightCenter    // 우측 중심 (1, 0.5)
}

/// <summary>
/// RadialFill 머티리얼을 사용하는 다양한 효과들을 위한 Static Utility 클래스입니다.
/// Unity의 일반적인 패턴에 따라 정적 메서드로 구현되었습니다.
/// </summary>
public static class RadialFillUtils
{
    /// <summary>
    /// 스프라이트의 pivot을 UV 좌표계로 변환합니다.
    /// </summary>
    /// <param name="sprite">변환할 스프라이트</param>
    /// <returns>UV 좌표계의 pivot 위치</returns>
    public static Vector2 GetPivotAsUV(Sprite sprite)
    {
        if (sprite == null) return new Vector2(0.5f, 0.5f);
        
        Vector2 pivot = sprite.pivot;
        Rect spriteRect = sprite.rect;
        
        // pivot을 스프라이트 rect 내에서의 상대적 위치로 계산
        Vector2 relativePivot = new Vector2(
            (pivot.x - spriteRect.x) / spriteRect.width,
            (pivot.y - spriteRect.y) / spriteRect.height
        );
        
        return relativePivot;
    }
    
    /// <summary>
    /// 지정된 타입에 따라 중심점을 계산합니다.
    /// </summary>
    /// <param name="centerType">중심점 계산 방식</param>
    /// <param name="sprite">스프라이트 (SpritePivot 타입일 때 필요)</param>
    /// <param name="manualCenter">수동 중심점 (Manual 타입일 때 사용)</param>
    /// <returns>계산된 중심점 UV 좌표</returns>
    public static Vector2 CalculateCenterPoint(CenterPointType centerType, Sprite sprite = null, Vector2? manualCenter = null)
    {
        switch (centerType)
        {
            case CenterPointType.Manual:
                return manualCenter ?? new Vector2(0.5f, 0.5f);
                
            case CenterPointType.SpritePivot:
                if (sprite != null)
                    return GetPivotAsUV(sprite);
                return new Vector2(0.5f, 0.5f);
                
            case CenterPointType.TextureCenter:
                return new Vector2(0.5f, 0.5f);
                
            case CenterPointType.BottomCenter:
                return new Vector2(0.5f, 0f);
                
            case CenterPointType.TopCenter:
                return new Vector2(0.5f, 1f);
                
            case CenterPointType.LeftCenter:
                return new Vector2(0f, 0.5f);
                
            case CenterPointType.RightCenter:
                return new Vector2(1f, 0.5f);
                
            default:
                return new Vector2(0.5f, 0f);
        }
    }
    
    /// <summary>
    /// RadialFill 머티리얼의 기본 설정을 초기화합니다.
    /// </summary>
    /// <param name="material">설정할 머티리얼</param>
    /// <param name="centerType">중심점 타입</param>
    /// <param name="sprite">스프라이트 (pivot 계산용)</param>
    /// <param name="manualCenter">수동 중심점</param>
    /// <param name="clockwise">시계방향 여부</param>
    /// <param name="startAngle">시작 각도</param>
    public static void SetupRadialFill(Material material, CenterPointType centerType = CenterPointType.BottomCenter, 
        Sprite sprite = null, Vector2? manualCenter = null, bool clockwise = true, float startAngle = 0f)
    {
        if (material == null) return;
        
        Vector2 calculatedCenter = CalculateCenterPoint(centerType, sprite, manualCenter);
        
        if (material.HasProperty("_FillAmount"))
            material.SetFloat("_FillAmount", 0f);
        if (material.HasProperty("_Clockwise"))
            material.SetFloat("_Clockwise", clockwise ? 1f : 0f);
        if (material.HasProperty("_StartAngle"))
            material.SetFloat("_StartAngle", startAngle);
        if (material.HasProperty("_CenterPoint"))
            material.SetVector("_CenterPoint", calculatedCenter);
    }
    
    /// <summary>
    /// Fill Amount를 즉시 설정합니다.
    /// </summary>
    /// <param name="material">대상 머티리얼</param>
    /// <param name="fillAmount">0~1 사이의 채우기 진행도</param>
    public static void SetFillAmount(Material material, float fillAmount)
    {
        if (material?.HasProperty("_FillAmount") == true)
        {
            material.SetFloat("_FillAmount", Mathf.Clamp01(fillAmount));
        }
    }
    
    /// <summary>
    /// 중심점을 업데이트합니다. (실시간 변경시 유용)
    /// </summary>
    /// <param name="material">대상 머티리얼</param>
    /// <param name="centerType">중심점 타입</param>
    /// <param name="sprite">스프라이트</param>
    /// <param name="manualCenter">수동 중심점</param>
    public static void UpdateCenterPoint(Material material, CenterPointType centerType, Sprite sprite = null, Vector2? manualCenter = null)
    {
        if (material?.HasProperty("_CenterPoint") == true)
        {
            Vector2 calculatedCenter = CalculateCenterPoint(centerType, sprite, manualCenter);
            material.SetVector("_CenterPoint", calculatedCenter);
        }
    }
    
    #region DOTween 기반 애니메이션 메서드들
    
    /// <summary>
    /// 단순한 fill 애니메이션을 재생합니다. (0에서 1까지)
    /// </summary>
    /// <param name="material">대상 머티리얼</param>
    /// <param name="duration">애니메이션 지속 시간</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Tween PlayFillAnimation(Material material, float duration = 1f, System.Action onComplete = null)
    {
        if (material?.HasProperty("_FillAmount") != true) 
        {
            onComplete?.Invoke();
            return null;
        }
        
        material.SetFloat("_FillAmount", 0f);
        return material.DOFloat(1f, "_FillAmount", duration).OnComplete(() => onComplete?.Invoke());
    }
    
    /// <summary>
    /// 특정 값까지 fill 애니메이션을 재생합니다.
    /// </summary>
    /// <param name="material">대상 머티리얼</param>
    /// <param name="targetFill">목표 fill 값</param>
    /// <param name="duration">애니메이션 지속 시간</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Tween PlayFillToValue(Material material, float targetFill, float duration = 1f, System.Action onComplete = null)
    {
        if (material?.HasProperty("_FillAmount") != true)
        {
            onComplete?.Invoke();
            return null;
        }
        
        float currentFill = material.GetFloat("_FillAmount");
        return material.DOFloat(targetFill, "_FillAmount", duration).OnComplete(() => onComplete?.Invoke());
    }
    
    /// <summary>
    /// 유키 무기 스타일의 swirl 효과를 재생합니다. (빠른 채움 + 페이드아웃)
    /// </summary>
    /// <param name="material">대상 머티리얼</param>
    /// <param name="renderer">페이드아웃할 SpriteRenderer</param>
    /// <param name="totalDuration">전체 지속 시간</param>
    /// <param name="fillRatio">채움 단계 비율 (0.3 = 전체 시간의 30%)</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Sequence PlaySwirlEffect(Material material, SpriteRenderer renderer, float totalDuration = 1f, float fillRatio = 0.3f, System.Action onComplete = null)
    {
        if (material?.HasProperty("_FillAmount") != true || renderer == null)
        {
            onComplete?.Invoke();
            return null;
        }
        
        // 초기 설정
        material.SetFloat("_FillAmount", 0f);
        
        // 원본 색상 저장 (머티리얼의 _Color 프로퍼티 우선)
        Color originalColor;
        bool useMaterialColor = material.HasProperty("_Color");
        
        if (useMaterialColor)
        {
            originalColor = material.GetColor("_Color");
        }
        else
        {
            originalColor = renderer.color;
        }
        
        // Swirl 효과용 시작 색상 (약간 투명하게)
        Color swirlStartColor = originalColor;
        swirlStartColor.a = originalColor.a * 0.7f; // 원본 투명도의 70%로 시작
        
        // 초기 색상 설정
        if (useMaterialColor)
        {
            material.SetColor("_Color", swirlStartColor);
        }
        else
        {
            renderer.color = swirlStartColor;
        }
        
        Sequence sequence = DOTween.Sequence();
        
        // 1단계: 빠른 채움
        float fillDuration = totalDuration * fillRatio;
        sequence.Append(material.DOFloat(1f, "_FillAmount", fillDuration));
        
        // 2단계: 페이드아웃
        float fadeDuration = totalDuration * (1f - fillRatio);
        Color fadeColor = originalColor;
        fadeColor.a = 0f;
        
        if (useMaterialColor)
        {
            // RadialFill 머티리얼의 _Color 프로퍼티를 페이드아웃
            sequence.Append(material.DOColor(fadeColor, "_Color", fadeDuration));
        }
        else
        {
            // 일반 SpriteRenderer 색상 페이드아웃
            sequence.Append(renderer.DOColor(fadeColor, fadeDuration));
        }
        
        // 완료 후 색상 복구 및 콜백
        sequence.OnComplete(() =>
        {
            if (useMaterialColor)
            {
                material.SetColor("_Color", originalColor);
            }
            else
            {
                renderer.color = originalColor;
            }
            onComplete?.Invoke();
        });
        
        return sequence;
    }
    
    /// <summary>
    /// 알파 보스 스타일의 charging 효과를 재생합니다. (점진적 채움)
    /// </summary>
    /// <param name="material">대상 머티리얼</param>
    /// <param name="duration">차징 지속 시간</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Tween PlayChargingEffect(Material material, float duration = 3f, System.Action onComplete = null)
    {
        return PlayFillAnimation(material, duration, onComplete);
    }
    
    #endregion
    
    #region 빠른 설정 헬퍼 메서드들
    
    /// <summary>
    /// 빠른 Swirl 효과 실행 (정적 메서드)
    /// </summary>
    /// <param name="renderer">대상 SpriteRenderer</param>
    /// <param name="centerType">중심점 타입</param>
    /// <param name="duration">지속 시간</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Sequence PlayQuickSwirlEffect(SpriteRenderer renderer, CenterPointType centerType = CenterPointType.BottomCenter, float duration = 1f, System.Action onComplete = null)
    {
        if (renderer?.material == null)
        {
            onComplete?.Invoke();
            return null;
        }
        
        SetupRadialFill(renderer.material, centerType, renderer.sprite);
        return PlaySwirlEffect(renderer.material, renderer, duration, 0.3f, onComplete);
    }
    
    /// <summary>
    /// 빠른 Charging 효과 실행
    /// </summary>
    /// <param name="renderer">대상 SpriteRenderer</param>
    /// <param name="centerType">중심점 타입</param>
    /// <param name="duration">지속 시간</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Tween PlayQuickChargingEffect(SpriteRenderer renderer, CenterPointType centerType = CenterPointType.SpritePivot, float duration = 3f, System.Action onComplete = null)
    {
        if (renderer?.material == null)
        {
            onComplete?.Invoke();
            return null;
        }
        
        SetupRadialFill(renderer.material, centerType, renderer.sprite);
        return PlayChargingEffect(renderer.material, duration, onComplete);
    }
    
    #endregion
}