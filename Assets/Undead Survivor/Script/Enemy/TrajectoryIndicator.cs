using System.Collections;
using UnityEngine;

/// <summary>
/// 투사체 궤적을 미리 표시하는 클래스입니다.
/// PoolManager와 호환되며, SpriteRenderer를 사용해 사각형 형태로 궤적을 표시합니다.
/// </summary>
public class TrajectoryIndicator : MonoBehaviour
{
    [Header("Sprite Renderer Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer; // 궤적 스프라이트 렌더러
    [SerializeField] private Sprite trajectorySprite; // 궤적용 사각형 스프라이트
    
    [Header("Visual Effects")]
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 알파 변화 커브
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.2f); // 스케일 변화 커브
    
    
    // 표시 상태
    private bool isDisplaying = false;
    private Coroutine displayCoroutine;
    
    // 궤적 설정
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float baseWidth = 0.5f; // 사각형 스프라이트의 기본 너비 (스케일 기준)
    private float displayDuration;
    
    // 원본 스케일 저장
    private Vector3 originalScale;
    
    void Awake()
    {
        // SpriteRenderer 자동 할당 및 설정
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // SpriteRenderer가 없으면 생성
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // 기본 SpriteRenderer 설정
        SetupSpriteRenderer();
        
        // 원본 스케일 저장
        originalScale = transform.localScale;
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        isDisplaying = false;
        
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        transform.localScale = originalScale;
    }
    
    void OnDisable()
    {
        // 비활성화 시 상태 정리
        isDisplaying = false;
        
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
        
        // 스케일 복원
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// SpriteRenderer의 기본 설정을 구성합니다.
    /// </summary>
    private void SetupSpriteRenderer()
    {
        if (spriteRenderer == null) return;
        
        // 스프라이트 설정
        if (trajectorySprite != null)
        {
            spriteRenderer.sprite = trajectorySprite;
        }
        
        
        // 정렬 순서 설정 (UI 위에 표시)
        spriteRenderer.sortingOrder = 10;
        
        // 머티리얼 설정 (기본 Sprites-Default 사용)
        if (spriteRenderer.material == null)
        {
            spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
    
    /// <summary>
    /// 궤적 표시를 설정합니다.
    /// </summary>
    /// <param name="start">시작 위치</param>
    /// <param name="end">끝 위치</param>
    /// <param name="color">스프라이트 색상</param>
    /// <param name="width">궤적 너비</param>
    public void SetupTrajectory(Vector3 start, Vector3 end, float width)
    {
        startPosition = start;
        endPosition = end;
        baseWidth = width;
        
        // SpriteRenderer 업데이트
        UpdateSpriteRenderer();
    }
    
    /// <summary>
    /// SpriteRenderer를 현재 설정에 맞게 업데이트합니다.
    /// </summary>
    private void UpdateSpriteRenderer()
    {
        if (spriteRenderer == null) return;
        
        // 궤적의 중심 위치 계산
        Vector3 centerPosition = (startPosition + endPosition) * 0.5f;
        transform.position = centerPosition;
        
        // 궤적의 방향과 길이 계산
        Vector3 direction = endPosition - startPosition;
        float distance = direction.magnitude;
        
        // 회전 설정 (궤적 방향으로 회전)
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // 스케일 설정 (길이에 맞게 X축 스케일 조정, Y축은 너비)
        Vector3 newScale = originalScale;
        newScale.x = distance; // X축은 궤적 길이
        newScale.y = baseWidth; // Y축은 궤적 너비
        transform.localScale = newScale;
    
        
        // SpriteRenderer 활성화
        spriteRenderer.enabled = true;
    }
    
    /// <summary>
    /// 궤적 표시를 시작합니다.
    /// </summary>
    /// <param name="duration">표시 지속 시간</param>
    public void StartDisplay(float duration)
    {
        if (isDisplaying) return;
        
        displayDuration = duration;
        isDisplaying = true;
        
        // 오브젝트 활성화
        gameObject.SetActive(true);
        
        // 표시 코루틴 시작
        displayCoroutine = StartCoroutine(DisplayCoroutine());
    }
    
    /// <summary>
    /// 궤적 표시 코루틴입니다.
    /// </summary>
    private IEnumerator DisplayCoroutine()
    {
        if (spriteRenderer == null) yield break;
        
        float elapsed = 0f;
        Vector3 baseScale = transform.localScale;
        
        // SpriteRenderer 활성화
        spriteRenderer.enabled = true;
        
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / displayDuration;
            
            // 알파 값 애니메이션
            float alpha = alphaCurve.Evaluate(progress);
            
            // 스케일 애니메이션 (Y축만 - 너비 변화)
            float scaleMultiplier = scaleCurve.Evaluate(progress);
            Vector3 animatedScale = baseScale;
            animatedScale.y = baseScale.y * scaleMultiplier;
            transform.localScale = animatedScale;
            
            
            yield return null;
        }
        
        // 표시 완료 후 정리
        CompleteDisplay();
    }
    
    /// <summary>
    /// 표시를 완료하고 정리합니다.
    /// </summary>
    private void CompleteDisplay()
    {
        isDisplaying = false;
        displayCoroutine = null;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // 스케일 복원
        transform.localScale = originalScale;
        
        // 풀로 반환
        ReturnToPool();
    }
    
    /// <summary>
    /// 표시를 강제로 중단하고 정리합니다.
    /// </summary>
    public void ForceCleanup()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
        
        isDisplaying = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // 스케일 복원
        transform.localScale = originalScale;
        
        ReturnToPool();
    }
    
    /// <summary>
    /// 궤적의 색상을 변경합니다.
    /// </summary>
    public void SetColor(Color newColor)
    {
        if (spriteRenderer != null && isDisplaying)
        {
            Color currentColor = newColor;
            currentColor.a = spriteRenderer.color.a; // 현재 알파값 유지
            spriteRenderer.color = currentColor;
        }
    }
    
    /// <summary>
    /// 궤적의 너비를 변경합니다.
    /// </summary>
    public void SetWidth(float newWidth)
    {
        baseWidth = newWidth;
        if (isDisplaying)
        {
            // 현재 스케일 업데이트
            Vector3 currentScale = transform.localScale;
            currentScale.y = newWidth;
            transform.localScale = currentScale;
        }
    }
    
    /// <summary>
    /// 궤적용 스프라이트를 변경합니다.
    /// </summary>
    public void SetSprite(Sprite newSprite)
    {
        trajectorySprite = newSprite;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
    
    /// <summary>
    /// 현재 표시 중인지 확인합니다.
    /// </summary>
    public bool IsDisplaying()
    {
        return isDisplaying;
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
    /// 에디터에서 궤적을 미리보기합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (startPosition != Vector3.zero && endPosition != Vector3.zero)
        {
            // 궤적 라인 표시
            Gizmos.DrawLine(startPosition, endPosition);
            
            // 사각형 영역 표시
            Vector3 center = (startPosition + endPosition) * 0.5f;
            Vector3 direction = endPosition - startPosition;
            float distance = direction.magnitude;
            
            
            // 궤적의 사각형 영역을 근사치로 표시
            Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.forward) * baseWidth * 0.5f;
            Vector3[] corners = new Vector3[4]
            {
                startPosition + perpendicular,
                startPosition - perpendicular,
                endPosition - perpendicular,
                endPosition + perpendicular
            };
            
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
            
            // 시작점과 끝점 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPosition, 0.2f);
        }
    }
}