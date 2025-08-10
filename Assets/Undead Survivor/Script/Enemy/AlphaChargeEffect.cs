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
    public SpriteRenderer swirlSprite;
    
    [Header("Effect Parameters")]
    [SerializeField] private float chargeEffectScale = 3f; // 이펙트 크기
    [SerializeField] private Color baseColor = Color.red; // 기본 색상
    [SerializeField] private Color fillColor = Color.darkRed; // 채워지는 색상
    
    [Header("Swirl Effect Settings")]
    [SerializeField] private float swirlRotationSpeed = 720f; // swirl 회전 속도 (도/초)
    [SerializeField] private float swirlDuration = 0.1f; // swirl 지속 시간
    
    [Header("Debug Settings (Editor Only)")]
    [SerializeField] private bool enableEditorDebug = false; // 에디터 디버깅 활성화
    [Range(0f, 1f)]
    [SerializeField] private float debugFillAmount = 0f; // 디버그용 fill 값
    [SerializeField] private bool debugAutoPlay = false; // 자동 재생
    [SerializeField] private float debugChargeDuration = 3f; // 디버그 차징 시간
    [SerializeField] private Vector2 debugDirection = Vector2.up; // 디버그 방향
    
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
    }
    
    void Update()
    {
#if UNITY_EDITOR
        // 에디터 디버깅 기능
        if (enableEditorDebug && Application.isPlaying)
        {
            HandleEditorDebug();
        }
#endif
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 에디터 디버깅 기능 처리
    /// </summary>
    private void HandleEditorDebug()
    {
        // 자동 재생 기능
        if (debugAutoPlay && !isCharging)
        {
            StartCharging(debugChargeDuration, debugDirection.normalized);
        }
        
        // 수동 fill 값 조정
        if (!isCharging && fillSprite != null && fillSprite.material != null)
        {
            fillSprite.material.SetFloat("_FillAmount", debugFillAmount * 0.5f); // 반원이므로 0.5 곱함
        }
        
        // 에디터에서 gameObject가 비활성화된 상태라면 강제로 활성화하고 설정
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            SetupSprites();
            
            // 디버그 방향에 따른 회전
            float angle = Mathf.Atan2(debugDirection.y, debugDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            
            // 크기 설정
            transform.localScale = Vector3.one * chargeEffectScale;
        }
    }
    
    /// <summary>
    /// 에디터에서 값이 변경될 때 호출
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying || !enableEditorDebug) return;
        
        // 디버그 방향이 변경되면 회전 업데이트
        if (debugDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(debugDirection.y, debugDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
        
        // Fill Amount가 변경되면 머티리얼 업데이트
        if (fillSprite != null && fillSprite.material != null && !isCharging)
        {
            fillSprite.material.SetFloat("_FillAmount", debugFillAmount * 0.5f);
        }
    }
    
    /// <summary>
    /// 에디터 컨텍스트 메뉴: 차징 테스트
    /// </summary>
    [ContextMenu("Test Charging")]
    private void TestCharging()
    {
        if (Application.isPlaying)
        {
            StartCharging(debugChargeDuration, debugDirection.normalized);
        }
    }
    
    /// <summary>
    /// 에디터 컨텍스트 메뉴: 차징 중단
    /// </summary>
    [ContextMenu("Stop Charging")]
    private void TestStopCharging()
    {
        if (Application.isPlaying)
        {
            StopCharging();
        }
    }
    
    /// <summary>
    /// 에디터 컨텍스트 메뉴: Fill Amount 리셋
    /// </summary>
    [ContextMenu("Reset Fill Amount")]
    private void ResetFillAmount()
    {
        debugFillAmount = 0f;
        if (fillSprite != null && fillSprite.material != null)
        {
            fillSprite.material.SetFloat("_FillAmount", 0f);
        }
    }
    
    /// <summary>
    /// 에디터 컨텍스트 메뉴: Swirl 테스트
    /// </summary>
    [ContextMenu("Test Swirl Effect")]
    private void TestSwirlEffect()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(PlaySwirlEffect());
        }
    }
#endif
    
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
            
            // 기존 라디얼 필 머티리얼 직접 사용 (이미 에디터에서 설정됨)
            Material fillMaterial = fillSprite.material;
            if (fillMaterial != null)
            {
                // 초기 fill 값 설정
                fillMaterial.SetFloat("_FillAmount", 0f);
                
                // 중심점을 스프라이트 pivot으로 설정
                if (fillSprite.sprite != null)
                {
                    Vector2 pivot = GetPivotAsUV(fillSprite.sprite);
                    fillMaterial.SetVector("_CenterPoint", pivot);
                }
                
                // 시계방향 설정
                fillMaterial.SetFloat("_Clockwise", 1f);
                
                // 머티리얼에 색상 직접 설정
                if (fillMaterial.HasProperty("_Color"))
                {
                    fillMaterial.SetColor("_Color", fillColor);
                }
                else if (fillMaterial.HasProperty("_MainColor"))
                {
                    fillMaterial.SetColor("_MainColor", fillColor);
                }
                else if (fillMaterial.HasProperty("_TintColor"))
                {
                    fillMaterial.SetColor("_TintColor", fillColor);
                }
            }
        }
        
        // Swirl 스프라이트 설정
        if (swirlSprite != null)
        {
            swirlSprite.sortingOrder = 7; // 가장 앞에 표시
            swirlSprite.gameObject.SetActive(false); // 처음에는 비활성화
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
            
            // 라디얼 필 업데이트 (기존 머티리얼 직접 사용)
            if (fillSprite != null && fillSprite.material != null)
            {
                fillSprite.material.SetFloat("_FillAmount", progress);
            }
            
            yield return null;
        }
        
        // 차징 완료
        isCharging = false;
        
        // Swirl 효과 시작 (공격 순간)
        yield return StartCoroutine(PlaySwirlEffect());
        
        // 이펙트 종료 - Pool로 반환
        ReturnToPool();
    }
    
    /// <summary>
    /// Swirl 공격 효과를 재생합니다.
    /// </summary>
    private IEnumerator PlaySwirlEffect()
    {
        if (swirlSprite == null) yield break;
        
        // 차징 스프라이트들 숨김
        if (baseSprite != null) baseSprite.gameObject.SetActive(false);
        if (fillSprite != null) fillSprite.gameObject.SetActive(false);
        
        // Swirl 활성화
        swirlSprite.gameObject.SetActive(true);
        
        // 초기 설정
        float elapsed = 0f;
        float initialRotation = swirlSprite.transform.eulerAngles.z;
        
        // 기존 머티리얼 활용 - radial fill 초기화
        Material swirlMaterial = swirlSprite.material;
        if (swirlMaterial != null)
        {
            swirlMaterial.SetFloat("_FillAmount", 0f);
            swirlMaterial.SetFloat("_Clockwise", 1f); // 반시계방향
            swirlMaterial.SetColor("_Color", fillColor);
        }
        
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
            Color currentColor = swirlMaterial.GetColor("_Color");
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            
            if (swirlMaterial != null && swirlMaterial.HasProperty("_Color"))
            {
                swirlMaterial.SetColor("_Color", currentColor);
            }
            else
            {
                swirlSprite.color = currentColor;
            }
            
            yield return null;
        }
        
        // Swirl 비활성화
        swirlSprite.gameObject.SetActive(false);
        
        // 차징 스프라이트들 다시 활성화 (정리용)
        if (baseSprite != null) baseSprite.gameObject.SetActive(true);
        if (fillSprite != null) fillSprite.gameObject.SetActive(true);
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
        
        // Swirl 정리
        if (swirlSprite != null)
        {
            swirlSprite.gameObject.SetActive(false);
        }
        
        ReturnToPool();
    }
    
    /// <summary>
    /// 풀로 오브젝트를 반환합니다.
    /// </summary>
    private void ReturnToPool()
    {
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
    }

}