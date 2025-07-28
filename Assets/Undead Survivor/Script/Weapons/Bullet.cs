using UnityEngine;

/// <summary>
/// 총알의 로직을 관리하는 클래스입니다.
/// 데미지, 관통, 속도, 지속시간, 크기 등 무기의 모든 속성을 받아 동작합니다.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float damage;
    public int per; // 관통 횟수 (pierce)
    public float speed;
    public float duration;
    public float scale; // 투사체의 크기

    private Rigidbody2D rigid;
    private float timer; // 지속시간 타이머

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Weapon으로부터 모든 속성을 받아 총알을 초기화하고 발사합니다.
    /// </summary>
    public void Init(float damage, int per, float duration, float speed, float scale, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;
        this.duration = duration;
        this.speed = speed;
        this.scale = scale;

        // 투사체의 크기 적용
        transform.localScale = Vector3.one * scale;

        // 근접 무기가 아닌 경우 (dir이 Vector3.zero가 아님)
        if (dir != Vector3.zero)
        {
            rigid.linearVelocity = dir * speed;
        }
    }

    void OnEnable()
    {
        // 오브젝트 풀에서 재사용될 때마다 타이머를 초기화합니다.
        timer = 0f;
    }

    void Update()
    {
        // 지속시간이 0보다 크고, 타이머가 지속시간을 초과하면 비활성화합니다.
        // 근접 무기처럼 계속 유지되어야 하는 경우는 duration을 0 또는 음수로 설정하여 이 로직을 무시할 수 있습니다.
        if (duration > 0)
        {
            timer += Time.deltaTime;
            if (timer > duration)
            {
                Deactivate();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameTags.DESTRUCTIBLE))
        {
            collision.GetComponent<DestructibleObject>()?.TakeDamage(1);
        }

        // 적과 부딪혔을 때
        if (collision.CompareTag(GameTags.ENEMY))
        {
            // Enemy 스크립트의 TakeDamage 함수를 호출하여 피해를 줍니다.
            collision.GetComponent<Enemy>()?.TakeDamage(damage);

            // 무한 관통이 아닐 때만 관통 횟수를 차감합니다.
            if (per != -100)
            {
                per--;
                if (per < 0)
                {
                    Deactivate();
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // 화면 밖으로 나갔고, 무한 관통이 아닐 때
        if (!collision.CompareTag(GameTags.AREA) || per == -100)
        {
            return;
        }
        Deactivate();
    }

    /// <summary>
    /// 총알을 비활성화하고 풀에 반납합니다.
    /// </summary>
    private void Deactivate()
    {
        rigid.linearVelocity = Vector2.zero;
        
        // Poolable 컴포넌트를 사용하여 풀에 반납하는 로직을 유지합니다.
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

