using UnityEngine;

/// <summary>
/// MagicLampWeapon에서 발사되는 주먹 투사체의 로직을 관리합니다.
/// 플레이어 중심에서 적을 향해 짧은 거리를 이동하며 피해를 줍니다.
/// </summary>
public class MagicLampEffect : MonoBehaviour
{
    public float waitTime = 0.5f; // 주먹이 발사되기 전 대기 시간
    public float flightTime = 0.2f; // 주먹이 날아가는 시간
    public float totalTime = 1f; // 전체 지속 시간
    private float damage;
    private float duration; // 투사체 지속 시간 (이동 거리와 연관)
    private float speed;    // 투사체 이동 속도
    private Vector3 direction; // 투사체 이동 방향
    private float timer; // 지속 시간 타이머

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        // 투사체 이동
        if (timer >= waitTime && timer < waitTime+flightTime) transform.position += direction * speed * Time.deltaTime;

        // 지속 시간 체크
        if (timer > totalTime)
        {
            Deactivate();
        }
    }

    /// <summary>
    /// 주먹 투사체를 초기화합니다.
    /// </summary>
    /// <param name="dmg">피해량</param>
    /// <param name="dur">지속 시간 (이동 거리와 연관)</param>
    /// <param name="spd">이동 속도</param>
    /// <param name="dir">이동 방향</param>
    public void Init(float dmg, float dur, float spd, Vector3 dir)
    {
        damage = dmg;
        duration = dur;
        speed = spd;
        direction = dir;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 투사체를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void Deactivate()
    {
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
