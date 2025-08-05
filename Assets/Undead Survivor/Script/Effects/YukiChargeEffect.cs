using UnityEngine;
using System.Collections;

/// <summary>
/// 유키 무기의 차오름 효과를 시계방향으로 점진적으로 채워지는 방식으로 구현하는 컴포넌트입니다.
/// 어두운 스프라이트 위에 밝은 스프라이트가 시계방향으로 점진적으로 드러나는 효과를 제공합니다.
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
    
    [Tooltip("스프라이트 pivot을 중심점으로 사용")]
    public bool usePivotAsCenter = true;
    
    [Tooltip("수동 중심점 (usePivotAsCenter가 false일 때 사용)")]
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
        darkSprite.sortingOrder = 0;
        brightSprite.sortingOrder = 1;
    }
    
    /// <summary>
    /// 스프라이트의 pivot을 UV 좌표계로 변환합니다.
    /// </summary>
    /// <param name="sprite">변환할 스프라이트</param>
    /// <returns>UV 좌표계의 pivot 위치</returns>
    private Vector2 GetPivotAsUV(Sprite sprite)
    {
        if (sprite == null) return new Vector2(0.5f, 0.5f);
        
        // 스프라이트의 pivot을 텍스처 크기로 나누어 0~1 범위의 UV 좌표로 변환
        Vector2 pivot = sprite.pivot;
        Vector2 textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
        
        // 스프라이트 rect 고려
        Rect spriteRect = sprite.rect;
        
        // pivot을 스프라이트 rect 내에서의 상대적 위치로 계산
        Vector2 relativePivot = new Vector2(
            (pivot.x - spriteRect.x) / spriteRect.width,
            (pivot.y - spriteRect.y) / spriteRect.height
        );
        
        return relativePivot;
    }
    
    /// <summary>
    /// 현재 설정에 따라 중심점을 가져옵니다.
    /// </summary>
    /// <returns>사용할 중심점</returns>
    private Vector2 GetCenterPoint()
    {
        if (usePivotAsCenter && brightSprite != null && brightSprite.sprite != null)
        {
            Vector2 pivotUV = GetPivotAsUV(brightSprite.sprite);
        
            
            return pivotUV;
        }
        else
        {
            return manualCenterPoint;
        }
    }
    
    /// <summary>
    /// 에디터에서 할당한 머티리얼을 설정합니다.
    /// </summary>
    private void SetupMaterial()
    {
        
        // 에디터에서 할당한 머티리얼이 있으면 사용
        if (brightSpriteMaterial != null)
        {
            brightSprite.material = brightSpriteMaterial;
            Debug.Log("에디터에서 할당한 RadialFill 머티리얼을 사용합니다.");
        }
        else
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
        }
        
        StartCoroutine(ChargeSequence());
    }
    
    /// <summary>
    /// 차오름 효과를 즉시 중단합니다.
    /// </summary>
    public void StopCharging()
    {
        StopAllCoroutines();
        isCharging = false;
        SetFillAmount(0f);
    }
    
    /// <summary>
    /// 차오름 시퀀스를 관리합니다.
    /// </summary>
    private IEnumerator ChargeSequence()
    {
        isCharging = true;
        
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
        
        // 완료 후 유지
        if (holdDuration > 0)
        {
            yield return new WaitForSeconds(holdDuration);
        }
        
        isCharging = false;
    }
    
    /// <summary>
    /// 채우기 진행도를 설정합니다.
    /// </summary>
    /// <param name="fillAmount">0~1 사이의 채우기 진행도</param>
    private void SetFillAmount(float fillAmount)
    {
        currentFillAmount = Mathf.Clamp01(fillAmount);
        
        if (brightSpriteMaterial != null)
        {
            // 방사형 채우기 셰이더 사용
            if (brightSpriteMaterial.HasProperty("_FillAmount"))
            {
                Vector2 centerPoint = GetCenterPoint(); // 동적으로 중심점 계산
                
                brightSpriteMaterial.SetFloat("_FillAmount", currentFillAmount);
                brightSpriteMaterial.SetFloat("_StartAngle", startAngle);
                brightSpriteMaterial.SetFloat("_Clockwise", clockwise ? 1f : 0f);
                brightSpriteMaterial.SetVector("_CenterPoint", new Vector4(centerPoint.x, centerPoint.y, 0, 0));
            }
            else
            {
                // 셰이더가 없는 경우 간단한 알파 페이드 사용
                Color color = brightSprite.color;
                color.a = currentFillAmount;
                brightSprite.color = color;
            }
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
        // 에디터에서 할당한 머티리얼은 정리하지 않음
        // Unity가 자동으로 관리함
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
            if (brightSpriteMaterial.HasProperty("_CenterPoint"))
            {
                Vector2 centerPoint = GetCenterPoint(); // 동적으로 중심점 계산
                
                brightSpriteMaterial.SetVector("_CenterPoint", new Vector4(centerPoint.x, centerPoint.y, 0, 0));
                brightSpriteMaterial.SetFloat("_StartAngle", startAngle);
                brightSpriteMaterial.SetFloat("_Clockwise", clockwise ? 1f : 0f);
            }
        }
    }
}