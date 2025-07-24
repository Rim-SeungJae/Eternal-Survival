using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

/// <summary>
/// Ghost Light 투사체의 이동 및 도착 시 폭발 로직을 관리합니다.
/// 개선: 각 투사체가 서로 다른 시작 각도에서 공전을 시작합니다.
/// </summary>
public class GhostLightProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float dur;
    private float area;
    private float groundEffectTickRate;
    private Vector3 targetPosition;
    private Transform playerTransform; // 플레이어 Transform 참조

    // 공전 관련 변수 (개선됨)
    [Header("Orbit Settings")]
    [Tooltip("투사체가 플레이어 주위를 공전할 궤도 반지름")]
    public float orbitRadius = 0.5f;
    [Tooltip("투사체가 플레이어 주위를 공전하는 시간")]
    public float orbitDuration = 0.2f;
    [Tooltip("시작 각도 (도)")]
    public float startAngle = 0f;

    private bool isOrbiting = true; // 공전 중인지 여부
    private float orbitTimer = 0f; // 공전 타이머
    private Vector3 orbitCenterOffset; // 플레이어로부터의 초기 상대 위치

    public void Init(float dmg, float dur, float spd, float area, Vector3 trgtPos, float tickRate, Transform playerTrns, float startAng)
    {
        this.damage = dmg;
        this.speed = spd;
        this.dur = dur;
        this.area = area;
        this.targetPosition = trgtPos;
        this.groundEffectTickRate = tickRate;
        this.playerTransform = playerTrns;
        this.startAngle = startAng;

        // 초기 상태 설정
        isOrbiting = true;
        orbitTimer = 0f;
        
        // 개선된 초기 위치 설정: 시작 각도를 고려한 위치
        float startAngleRad = startAngle * Mathf.Deg2Rad;
        orbitCenterOffset = new Vector3(
            Mathf.Cos(startAngleRad) * orbitRadius,
            Mathf.Sin(startAngleRad) * orbitRadius,
            0
        );
        transform.position = playerTransform.position + orbitCenterOffset;

        // 기존에 실행 중인 트윈이 있다면 모두 제거합니다.
        transform.DOKill(true);
    }

    void Update()
    {
        if (isOrbiting)
        {
            orbitTimer += Time.deltaTime;

            // 개선된 공전 로직: 시작 각도를 고려한 공전
            float currentAngle = (startAngle + (orbitTimer / orbitDuration) * 180f) * Mathf.Deg2Rad; // 180도 공전
            Vector3 rotatedOffset = new Vector3(
                Mathf.Cos(currentAngle) * orbitRadius,
                Mathf.Sin(currentAngle) * orbitRadius,
                0
            );
            transform.position = playerTransform.position + rotatedOffset;

            if (orbitTimer >= orbitDuration)
            {
                isOrbiting = false;
                // 공전이 끝난 후 타겟 위치로 이동 시작
            }
        }
        else
        {
            // 타겟 위치를 향해 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // 타겟 위치에 충분히 가까워지면 폭발
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                SpawnGroundEffect();
            }
        }

        // 투사체 지속 시간 체크 (공전 시간 포함)
        dur -= Time.deltaTime;
        if (dur <= 0)
        {
            SpawnGroundEffect(); // 지속 시간 만료 시에도 장판 생성
        }
    }

    /// <summary>
    /// 현재 위치에 화염 장판 이펙트를 생성하고 자신은 풀에 반환됩니다.
    /// </summary>
    void SpawnGroundEffect()
    {
        // 이미 장판이 생성되었거나 비활성화 중이라면 중복 호출 방지
        if (!gameObject.activeSelf) return;

        GameObject effectObj = GameManager.instance.pool.Get("GhostLightGroundEffect"); 
        if (effectObj != null)
        {
            effectObj.transform.position = transform.position;
            GhostLightGroundEffect groundEffect = effectObj.GetComponent<GhostLightGroundEffect>();
            if (groundEffect != null)
            {
                groundEffect.Init(damage, dur, groundEffectTickRate, area);
            }
            effectObj.SetActive(true);
        }

        // 자신을 풀에 반환
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

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 상태를 초기화합니다.
        isOrbiting = true;
        orbitTimer = 0f;
        transform.DOKill(true); // DOTween 트윈 강제 종료
    }
}


