using UnityEngine;
using System.Collections;

/// <summary>
/// Star Fragment 메테오 충돌 예정 위치를 표시하는 경고 이펙트
/// 원형 웨이브가 성장하면서 플레이어에게 위험을 알립니다.
/// </summary>
public class StarFragmentWarning : MonoBehaviour
{
    [Header("경고 표시 설정")]
    [Tooltip("웨이브 성장 곡선 (0에서 1까지)")]
    public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private SpriteRenderer mainSpriteRenderer; // 상위 컴포넌트 (고정된 원)
    private SpriteRenderer waveRenderer; // 하위 컴포넌트 (성장하는 웨이브)
    private GameObject waveObject;
    private Coroutine waveGrowthCoroutine;
    
    private float warningDuration; // 웨이브 성장에 걸리는 시간

    void Awake()
    {
        mainSpriteRenderer = GetComponent<SpriteRenderer>();
        
        // 하위 오브젝트에서 웨이브 렌더러 찾기
        Transform waveTransform = transform.Find("Wave");
        waveObject = waveTransform.gameObject;
        waveRenderer = waveObject.GetComponent<SpriteRenderer>();
    }

    public void Init(float radius)
    {
        // 메인 스프라이트 크기 설정
        if (mainSpriteRenderer != null)
        {
            mainSpriteRenderer.transform.localScale = Vector3.one;
            float mainDiameter = mainSpriteRenderer.sprite.bounds.size.x;
            if (mainDiameter > 0)
            {
                float mainScale = (radius * 2) / mainDiameter;
                mainSpriteRenderer.transform.localScale = new Vector3(mainScale, mainScale, 1f);
            }
        }
        
        // 웨이브 스프라이트 초기 설정 (크기 0)
        if (waveRenderer != null)
        {
            waveRenderer.transform.localScale = Vector3.zero;
            waveObject.SetActive(true);
        }
    }

    public void StartWaveGrowth(float duration)
    {
        warningDuration = duration;
        
        if (waveGrowthCoroutine != null)
        {
            StopCoroutine(waveGrowthCoroutine);
        }
        waveGrowthCoroutine = StartCoroutine(WaveGrowthEffect());
    }

    void OnEnable()
    {
        // 활성화될 때 웨이브 오브젝트 초기화
        if (waveObject != null)
        {
            waveObject.SetActive(true);
            waveObject.transform.localScale = Vector3.zero;
        }
    }

    void OnDisable()
    {
        // 비활성화될 때 웨이브 성장 효과 중지
        if (waveGrowthCoroutine != null)
        {
            StopCoroutine(waveGrowthCoroutine);
            waveGrowthCoroutine = null;
        }
        
        if (waveObject != null)
        {
            waveObject.SetActive(false);
        }
    }

    private IEnumerator WaveGrowthEffect()
    {
        if (waveRenderer == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < warningDuration && gameObject.activeSelf)
        {
            float progress = elapsedTime / warningDuration;
            float curveValue = growthCurve.Evaluate(progress);
            
            // 웨이브 크기를 0에서 1(상위 오브젝트와 같은 크기)까지 성장
            waveRenderer.transform.localScale = new Vector3(curveValue, curveValue, 1f);
            
            // 성장하면서 점점 투명해지는 효과
            Color waveColor = waveRenderer.color;
            waveColor.a = Mathf.Lerp(0.8f, 0.2f, progress);
            waveRenderer.color = waveColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 애니메이션 완료 후 풀에 반환
        DeactivateWarning();
    }

    /// <summary>
    /// 경고 표시를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void DeactivateWarning()
    {
        // Poolable 컴포넌트를 사용하여 풀에 반납합니다.
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
} 