using UnityEngine;
using System.Collections;

/// <summary>
/// Star Fragment 메테오 충돌 예정 위치를 표시하는 경고 이펙트
/// 깜빡이는 효과로 플레이어에게 위험을 알립니다.
/// </summary>
public class StarFragmentWarning : MonoBehaviour
{
    [Header("경고 표시 설정")]
    [Tooltip("경고 표시 깜빡임 간격")]
    public float blinkInterval = 0.2f;
    
    [Tooltip("경고 표시 최소 투명도")]
    [Range(0f, 1f)]
    public float minAlpha = 0.3f;
    
    [Tooltip("경고 표시 최대 투명도")]
    [Range(0f, 1f)]
    public float maxAlpha = 1.0f;

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(float radius)
    {
        // 오브젝트 풀에서 재활용될 때마다 스케일을 초기화합니다.
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = Vector3.one; // 스케일 초기화
            // 지름을 기준으로 스케일 계산
            float circleDiameter = spriteRenderer.sprite.bounds.size.x;
            if (circleDiameter > 0)
            {
                float circleScale = (radius * 2) / circleDiameter;
                spriteRenderer.transform.localScale = new Vector3(circleScale, circleScale, 1f);
            }
        }
    }

    void OnEnable()
    {
        // 활성화될 때마다 깜빡임 효과 시작
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    void OnDisable()
    {
        // 비활성화될 때 깜빡임 효과 중지
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    private IEnumerator BlinkEffect()
    {
        if (spriteRenderer == null) yield break;
        
        while (gameObject.activeSelf)
        {
            // 투명도를 minAlpha에서 maxAlpha 사이로 변경하여 깜빡임 효과
            Color color = spriteRenderer.color;
            color.a = minAlpha;
            spriteRenderer.color = color;
            
            yield return new WaitForSeconds(blinkInterval);
            
            color.a = maxAlpha;
            spriteRenderer.color = color;
            
            yield return new WaitForSeconds(blinkInterval);
        }
    }
} 