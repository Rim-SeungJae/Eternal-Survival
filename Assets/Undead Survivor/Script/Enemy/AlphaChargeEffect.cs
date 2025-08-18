using System.Collections;
using UnityEngine;
using DG.Tweening;

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
    
    [Header("Range Settings")]
    public Transform rangeIndicator; // 공격 범위 지시자 (에디터에서 연결)
    
    [Header("Swirl Effect Settings")]
    [SerializeField] private float swirlDuration = 0.1f; // swirl 지속 시간
    
    private bool isCharging = false;
    
    // DOTween 애니메이션 참조
    private Tween currentChargeTween;
    private Sequence currentSwirlSequence;
    
    
    /// <summary>
    /// RangeIndicator를 기준으로 실제 공격 범위를 계산합니다. (Scale 고려)
    /// </summary>
    public float GetActualAttackRange()
    {
        if (rangeIndicator == null) return 5f; // 기본값
        
        // 로컬 거리 계산
        float localDistance = rangeIndicator.localPosition.magnitude;
        
        // 현재 스케일 적용 (chargeEffectScale)
        float actualRange = localDistance * chargeEffectScale;
        
        return actualRange;
    }
    
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
                
                // RadialFillUtils로 중심점 설정
                RadialFillUtils.SetupRadialFill(fillMaterial, CenterPointType.SpritePivot, fillSprite.sprite);
                
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
    /// 차징 코루틴
    /// </summary>
    private IEnumerator ChargingCoroutine(float duration)
    {
        isCharging = true;
        
        // RadialFillUtils를 사용한 차징 효과
        if (fillSprite != null && fillSprite.material != null && fillSprite.material.HasProperty("_FillAmount"))
        {
            // RadialFill 설정
            RadialFillUtils.SetupRadialFill(
                fillSprite.material,
                CenterPointType.SpritePivot,
                fillSprite.sprite,
                null,
                true, // 시계방향
                0f
            );
            
            // DOTween 애니메이션 실행
            currentChargeTween = RadialFillUtils.PlayChargingEffect(fillSprite.material, duration, () =>
            {
                isCharging = false;
                // Swirl 효과 시작 (공격 순간)
                StartCoroutine(PlaySwirlEffectWithUtils());
            });
        }
        else
        {
            // 기존 방식 fallback
            yield return StartCoroutine(LegacyChargingCoroutine(duration));
        }
        
        yield return new WaitUntil(() => !isCharging);
    }
    
    /// <summary>
    /// 레거시 차징 방식 (RadialFillController가 없을 때)
    /// </summary>
    private IEnumerator LegacyChargingCoroutine(float duration)
    {
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
        
        // 이펙트 종료 - Pool로 반환
        ReturnToPool();
    }
    
    /// <summary>
    /// RadialFillUtils를 사용한 Swirl 효과를 재생합니다.
    /// </summary>
    private IEnumerator PlaySwirlEffectWithUtils()
    {
        if (swirlSprite == null) 
        {
            ReturnToPool();
            yield break;
        }
        
        // 차징 스프라이트들 숨김
        if (baseSprite != null) baseSprite.gameObject.SetActive(false);
        if (fillSprite != null) fillSprite.gameObject.SetActive(false);
        
        // Swirl 활성화
        swirlSprite.gameObject.SetActive(true);
        
        // RadialFillUtils를 사용한 Swirl 효과
        if (swirlSprite.material != null && swirlSprite.material.HasProperty("_FillAmount"))
        {
            // RadialFill 설정
            RadialFillUtils.SetupRadialFill(
                swirlSprite.material,
                CenterPointType.BottomCenter,
                swirlSprite.sprite,
                null,
                false, // 반시계방향
                0f
            );
            
            // 색상 설정
            if (swirlSprite.material.HasProperty("_Color"))
            {
                swirlSprite.material.SetColor("_Color", fillColor);
            }
            
            // DOTween Swirl 애니메이션
            currentSwirlSequence = RadialFillUtils.PlaySwirlEffect(
                swirlSprite.material, 
                swirlSprite, 
                swirlDuration, 
                0.3f, 
                () =>
                {
                    // Swirl 완료 후 정리
                    swirlSprite.gameObject.SetActive(false);
                    if (baseSprite != null) baseSprite.gameObject.SetActive(true);
                    if (fillSprite != null) fillSprite.gameObject.SetActive(true);
                    
                    // 이펙트 종료 - Pool로 반환
                    ReturnToPool();
                }
            );
        }
        else
        {
            // 기존 방식 fallback
            yield return StartCoroutine(PlaySwirlEffect());
        }
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