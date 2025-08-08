using System.Collections;
using UnityEngine;

/// <summary>
/// Alpha 보스의 반원 모양 차징 공격 이펙트를 관리하는 클래스입니다.
/// PoolManager와 호환됩니다.
/// </summary>
public class AlphaChargeEffect : MonoBehaviour
{
    [Header("Charge Effect Settings")]
    [SerializeField] private SpriteRenderer baseSprite; // 기본 반원 스프라이트
    [SerializeField] private SpriteRenderer fillSprite; // 채워지는 반원 스프라이트
    
    [Header("Material Settings")]
    public Material radialFillMaterial; // 라디얼 필 머티리얼 (public으로 변경)
    public Sprite baseSemiCircleSprite; // 기본 반원 스프라이트
    public Sprite fillSemiCircleSprite; // 채움용 반원 스프라이트
    
    [Header("Effect Parameters")]
    [SerializeField] private float chargeEffectScale = 3f; // 이펙트 크기
    [SerializeField] private Color baseColor = Color.red; // 기본 색상
    [SerializeField] private Color fillColor = Color.darkRed; // 채워지는 색상
    
    private Material materialInstance;
    private bool isCharging = false;
    
    void Awake()
    {
        // 기본 설정
        if (baseSprite == null) baseSprite = GetComponent<SpriteRenderer>();
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        // 풀에서 재사용될 때마다 상태 초기화
        isCharging = false;
        
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
            materialInstance = null;
        }
    }
    
    /// <summary>
    /// 차징 이펙트를 시작합니다.
    /// </summary>
    /// <param name="chargeDuration">차징 시간</param>
    /// <param name="direction">공격 방향</param>
    public void StartCharging(float chargeDuration, Vector2 direction)
    {
        if (isCharging) return;
        
        // 오브젝트 활성화
        gameObject.SetActive(true);
        
        // 방향에 따른 회전 설정 (스프라이트가 위를 향하고 있으므로 -90도 보정)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward); // 위쪽 기준이므로 -90도 보정
        
        // 크기 설정
        transform.localScale = Vector3.one * chargeEffectScale;
        
        // 스프라이트 설정
        SetupSprites();
        
        // 차징 시작
        StartCoroutine(ChargingCoroutine(chargeDuration));
    }
    
    /// <summary>
    /// 스프라이트들을 설정합니다.
    /// </summary>
    private void SetupSprites()
    {
        // 스프라이트 할당
        if (baseSprite != null && baseSemiCircleSprite != null)
        {
            baseSprite.sprite = baseSemiCircleSprite;
            baseSprite.color = baseColor;
            baseSprite.sortingOrder = 5;
        }
        
        if (fillSprite != null && fillSemiCircleSprite != null)
        {
            fillSprite.sprite = fillSemiCircleSprite;
            fillSprite.color = fillColor;
            fillSprite.sortingOrder = 6;
            
            // 라디얼 필 머티리얼 설정
            if (radialFillMaterial != null)
            {
                materialInstance = new Material(radialFillMaterial);
                fillSprite.material = materialInstance;
                
                // 초기 fill 값 설정
                materialInstance.SetFloat("_FillAmount", 0f);
                
                // 중심점을 스프라이트 아래쪽으로 설정 (반원의 중심)
                if (fillSprite.sprite != null)
                {
                    Vector2 pivot = GetPivotAsUV(fillSprite.sprite);
                    materialInstance.SetVector("_CenterPoint", pivot);
                }
                
                // 시계방향 설정
                materialInstance.SetFloat("_Clockwise", 1f);
                
                // 머티리얼에 색상 직접 설정
                if (materialInstance.HasProperty("_Color"))
                {
                    materialInstance.SetColor("_Color", fillColor);
                }
                else if (materialInstance.HasProperty("_MainColor"))
                {
                    materialInstance.SetColor("_MainColor", fillColor);
                }
                else if (materialInstance.HasProperty("_TintColor"))
                {
                    materialInstance.SetColor("_TintColor", fillColor);
                }
            }
        }
    }
    
    /// <summary>
    /// 스프라이트의 피벗을 UV 좌표로 변환합니다.
    /// </summary>
    private Vector2 GetPivotAsUV(Sprite sprite)
    {
        Vector2 pivot = sprite.pivot;
        Rect spriteRect = sprite.rect;
        Vector2 relativePivot = new Vector2(
            (pivot.x - spriteRect.x) / spriteRect.width,
            (pivot.y - spriteRect.y) / spriteRect.height
        );
        return relativePivot;
    }
    
    /// <summary>
    /// 차징 코루틴
    /// </summary>
    private IEnumerator ChargingCoroutine(float duration)
    {
        isCharging = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 라디얼 필 업데이트
            if (materialInstance != null)
            {
                materialInstance.SetFloat("_FillAmount", progress);
            }
            
            yield return null;
        }
        
        // 차징 완료
        isCharging = false;
        
        // 이펙트 종료 - Pool로 반환
        ReturnToPool();
    }
    
    /// <summary>
    /// 차징을 중단합니다.
    /// </summary>
    public void StopCharging()
    {
        if (isCharging)
        {
            StopAllCoroutines();
            isCharging = false;
        }
        
        ReturnToPool();
    }
    
    /// <summary>
    /// 풀로 오브젝트를 반환합니다.
    /// </summary>
    private void ReturnToPool()
    {
        // 머티리얼 정리
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
            materialInstance = null;
        }
        
        // Pool로 반환 (Poolable 컴포넌트 사용)
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
    
    void OnDisable()
    {
        // 코루틴 정리
        StopAllCoroutines();
        isCharging = false;
        
        // 머티리얼 정리
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
            materialInstance = null;
        }
    }
}