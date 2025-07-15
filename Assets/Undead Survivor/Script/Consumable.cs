using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

/// <summary>
/// 플레이어가 획득할 수 있는 소모성 아이템의 로직을 관리하는 클래스입니다.
/// DOTween을 활용한 연출과 거리 기반의 획득 판정을 사용합니다.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Consumable : MonoBehaviour
{
    public enum ConsumableType { Heal, TimeStop, WipeEnemies, Credit, Gold }

    public ConsumableType type;
    public float value;

    [Header("Absorption Settings")]
    [Tooltip("도달할 최대 속도")]
    public float maxSpeed = 12f;
    [Tooltip("초당 증가하는 속도 (가속도)")]
    public float acceleration = 0.5f;
    [Tooltip("획득으로 판정할 최소 거리")]
    public float minDistanceToAcquire = 0.1f;

    [Header("Bounce Effect Settings")]
    [Tooltip("튕겨나가는 거리")]
    public float bounceDistance = 0.5f;
    [Tooltip("튕겨나가는 데 걸리는 시간")]
    public float bounceDuration = 0.2f;

    private bool isAbsorbing = false;
    private Transform playerTransform;
    private float currentSpeed = 0f; // 현재 이동 속도
    private Tween bounceTween; // 튕겨나가는 트윈을 제어하기 위한 변수

    void OnEnable()
    {
        isAbsorbing = false;
        currentSpeed = 0f; // 활성화될 때 현재 속도 초기화
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    void Update()
    {
        if (!isAbsorbing || playerTransform == null) return;

        // 튕겨나가는 트윈이 진행 중일 때는 따라가지 않습니다.
        if (bounceTween != null && bounceTween.IsActive()) return;

        // 현재 속도를 가속도에 따라 점차 증가시킵니다.
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // 최대 속도 제한

        // 계산된 현재 속도로 플레이어를 향해 이동합니다.
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, playerTransform.position) < minDistanceToAcquire)
        {
            ApplyEffect();
            transform.DOKill();
            // gameObject.SetActive(false) 대신 풀에 반납하는 로직으로 변경
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

    /// <summary>
    /// ItemAbsorber에 의해 호출되어 흡수 상태를 시작합니다.
    /// </summary>
    public void StartAbsorb(Transform player)
    { 
        if (isAbsorbing || player == null) return;

        this.playerTransform = player;
        this.isAbsorbing = true;

        // 1. 플레이어로부터 아이템으로의 방향 벡터를 계산합니다.
        Vector3 directionToPlayer = (transform.position - player.position).normalized;
        // 방향 벡터가 (0,0,0)인 경우 (위치가 같은 경우)를 대비해 기본 방향을 설정합니다.
        if (directionToPlayer == Vector3.zero) directionToPlayer = Vector3.up;

        // 2. 플레이어 반대 방향으로 튕겨나갈 목표 위치를 계산합니다.
        Vector3 bounceTargetPosition = transform.position + directionToPlayer * bounceDistance;

        // 3. DOMove를 사용하여 해당 위치로 짧게 이동하는 트윈을 실행하고, 트윈을 변수에 저장합니다.
        //    Ease.OutQuad는 부드럽게 감속하는 효과를 줍니다.
        bounceTween = transform.DOMove(bounceTargetPosition, bounceDuration).SetEase(Ease.OutQuad);
    }

    private void ApplyEffect()
    {
        switch (type)
        {
            case ConsumableType.Heal:
                GameManager.instance.health = Mathf.Min(GameManager.instance.maxHealth, GameManager.instance.health + value);
                break;
            case ConsumableType.Credit:
                GameManager.instance.GetExp((int)value);
                break;
            case ConsumableType.TimeStop:
                GameManager.instance.StartTimeStop(value); // GameManager의 시간 정지 함수 호출
                break;
                // TODO: 아이템 획득 효과음 재생
        }
    }
}
