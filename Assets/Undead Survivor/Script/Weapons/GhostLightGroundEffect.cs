using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ghost Light의 화염 장판 효과를 관리합니다.
/// </summary>
public class GhostLightGroundEffect : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;
    private float tickRate;
    private float radius;

    private float durationTimer;
    private float tickTimer;
    private SpriteRenderer circleSprite;

    private List<GameObject> internalFlames = new List<GameObject>();

    [Header("Internal Flame Settings")]
    [Tooltip("내부 불꽃에 사용할 프리팹의 풀 태그")]
    public string internalFlamePoolTag = "GhostLightStaticFlame";
    [Tooltip("장판 면적당 생성될 불꽃의 밀도 (높을수록 더 많은 불꽃 생성)")]
    public float flameDensityFactor = 5f; // 새로운 변수
    [Tooltip("내부 불꽃이 장판의 가장자리로부터 떨어져야 할 최소 거리")]
    public float internalFlameSafeMargin = 0.2f; // 내부 불꽃의 대략적인 반지름을 고려

    void Awake()
    {
        // SpriteRenderer 컴포넌트를 Awake에서 한 번만 가져옵니다.
        circleSprite = GetComponent<SpriteRenderer>();
        if (circleSprite == null)
        {
            Debug.LogError("GhostLightGroundEffect: SpriteRenderer component not found on this GameObject.");
        }
    }

    public void Init(float dps, float dur, float tick, float rad)
    {
        this.damagePerSecond = dps;
        this.duration = dur;
        this.tickRate = tick;
        this.radius = rad;

        // 오브젝트 풀에서 재활용될 때마다 스케일을 초기화합니다.
        if (circleSprite != null)
        {
            circleSprite.transform.localScale = Vector3.one; // 스케일 초기화
            // 지름을 기준으로 스케일 계산
            float circleDiameter = circleSprite.sprite.bounds.size.x;
            if (circleDiameter > 0)
            {
                float circleScale = (radius * 2) / circleDiameter;
                circleSprite.transform.localScale = new Vector3(circleScale, circleScale, 1f);
            }
        }
    }

    void OnEnable()
    {
        durationTimer = 0f;
        tickTimer = 0f;
        SpawnInternalFlames();
    }

    void Update()
    {
        durationTimer += Time.deltaTime;
        if (durationTimer > duration)
        {
            CleanUpAndDeactivate();
            return;
        }

        tickTimer += Time.deltaTime;
        if (tickTimer > tickRate)
        {
            tickTimer = 0f;
            DealDamage();
        }
    }

    void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));
        foreach (var enemyCollider in enemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 피해량은 (초당 피해량 * 틱 간격)으로 계산
                enemy.TakeDamage(damagePerSecond * tickRate);
            }
        }
    }

    void SpawnInternalFlames()
    {
        // 내부 불꽃이 생성될 수 있는 유효 반지름을 계산합니다.
        float effectiveRadius = radius - internalFlameSafeMargin;
        if (effectiveRadius < 0) effectiveRadius = 0; // 반지름이 음수가 되지 않도록 방지

        // 장판의 면적에 비례하여 불꽃 개수를 동적으로 계산합니다.
        int calculatedFlameCount = Mathf.RoundToInt(Mathf.PI * effectiveRadius * effectiveRadius * flameDensityFactor);
        calculatedFlameCount = Mathf.Max(1, calculatedFlameCount); // 최소 1개는 생성되도록 보장

        for (int i = 0; i < calculatedFlameCount; i++)
        {
            GameObject flame = GameManager.instance.pool.Get(internalFlamePoolTag);
            if (flame != null)
            {
                // 장판 범위 내 랜덤한 위치에 배치 (안전 마진 적용)
                Vector2 randomPoint = Random.insideUnitCircle * effectiveRadius;
                flame.transform.position = transform.position + (Vector3)randomPoint;
                flame.SetActive(true);
                internalFlames.Add(flame);
            }
        }
    }

    void CleanUpAndDeactivate()
    {
        // 내부 불꽃들을 풀에 반환
        foreach (var flame in internalFlames)
        {
            if (flame != null && flame.activeSelf)
            {
                Poolable poolable = flame.GetComponent<Poolable>();
                if (poolable != null)
                {
                    GameManager.instance.pool.ReturnToPool(poolable.poolTag, flame);
                }
                else
                {
                    flame.SetActive(false);
                }
            }
        }
        internalFlames.Clear();

        // 장판 자신을 풀에 반환
        Poolable selfPoolable = GetComponent<Poolable>();
        if (selfPoolable != null)
        {
            GameManager.instance.pool.ReturnToPool(selfPoolable.poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}