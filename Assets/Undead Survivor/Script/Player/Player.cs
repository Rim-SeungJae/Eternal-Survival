using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linq 네임스페이스 추가
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 움직임, 애니메이션, 충돌 처리를 담당하는 클래스입니다.
/// Unity의 Input System을 사용하여 입력을 받습니다.
/// </summary>
public class Player : MonoBehaviour
{
    public const int MAX_WEAPONS = 6; // 최대 무기 수
    public const int MAX_GEARS = 6;   // 최대 장비 수
    
    // 상수들
    private const float REVIVE_INVINCIBLE_TIME = 2f; // 부활 후 무적 시간
    private const float REVIVE_HEALTH_RATIO = 0.5f; // 부활 시 체력 회복 비율

    [Tooltip("획득한 아이템 목록")]
    public List<Item> items;

    [Tooltip("플레이어의 입력 벡터")]
    public Vector2 inputVec;
    [Tooltip("플레이어의 이동 속도")]
    public ModifiableStat speed;
    [Tooltip("플레이어의 방어력")]
    public ModifiableStat defense;
    [Tooltip("플레이어의 부활 횟수")]
    public ModifiableStat revive;
    [Tooltip("주변 적을 탐지하는 스캐너")]
    public Scanner scanner;
    [Tooltip("플레이어의 손 오브젝트 (무기 장착 위치)")]
    public Hand[] hands;
    [Tooltip("플레이어의 시간정지 효과 파티클 시스템")]
    public ParticleSystem particle;

    // 컴포넌트 참조
    Rigidbody2D rg;
    SpriteRenderer sp;
    Animator an;

    private CharacterDataSO currentCharacterData; // 현재 플레이어의 CharacterDataSO
    private bool isInvincible = false; // 무적 상태 여부

    void Awake()
    {
        // 필수 컴포넌트들을 미리 캐싱하여 성능을 최적화합니다.
        rg = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        an = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        // 비활성화된 자식 오브젝트도 포함하여 Hand 컴포넌트를 찾습니다.
        hands = GetComponentsInChildren<Hand>(true);
        // ParticleSystem 컴포넌트를 찾아 캐싱합니다.
        particle = GetComponentInChildren<ParticleSystem>(true);

        // ModifiableStat 인스턴스 초기화
        speed = new ModifiableStat(0); 
        defense = new ModifiableStat(0);
        revive = new ModifiableStat(0);
        items = new List<Item>(); // 아이템 리스트 초기화
    }

    /// <summary>
    /// 현재 보유한 무기의 개수를 반환합니다.
    /// </summary>
    public int WeaponCount
    {
        get { return items.Count(item => item.data.itemType == ItemData.ItemType.Weapon); }
    }

    /// <summary>
    /// 현재 보유한 장비의 개수를 반환합니다.
    /// </summary>
    public int GearCount
    {
        get { return items.Count(item => item.data.itemType == ItemData.ItemType.Gear); }
    }

    /// <summary>
    /// 획득한 아이템을 목록에 추가합니다.
    /// </summary>
    public void AddItem(Item newItem)
    {
        if (!items.Contains(newItem))
        {
            items.Add(newItem);
        }
    }

    void OnEnable()
    {
        // GlobalData에서 선택된 캐릭터 데이터를 가져옵니다.
        currentCharacterData = GlobalData.selectedCharacterDataSO;

        // CharacterDataSO에서 플레이어의 초기 능력치를 설정합니다.
        speed.BaseValue = currentCharacterData.speedMultiplier; // speed.BaseValue에 할당
        GameManager.instance.maxHealth = currentCharacterData.baseHealth; // GameManager의 최대 체력 설정
        GameManager.instance.health = GameManager.instance.maxHealth; // 현재 체력도 최대 체력으로 설정

        // 선택된 캐릭터에 맞는 애니메이터 컨트롤러를 설정합니다.
        an.runtimeAnimatorController = currentCharacterData.animatorController;

        // GameManager의 playerId도 CharacterDataSO에서 가져온 ID로 설정합니다.
        GameManager.instance.playerId = currentCharacterData.characterId;
    }

    /// <summary>
    /// Input System에 의해 호출되는 이동 입력 처리 함수입니다.
    /// </summary>
    /// <param name="value">입력 값 (Vector2)</param>
    void OnMove(InputValue value)
    {
        // 입력 값을 Vector2로 변환하여 저장합니다.
        inputVec = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // 게임이 진행 중이 아닐 때는 물리 업데이트를 중단합니다.
        if (!GameManager.instance.isLive) return;
        
        // 입력 벡터와 속도를 기반으로 다음 위치를 계산하고,
        // Rigidbody를 사용하여 물리적으로 안전하게 이동시킵니다.
        Vector2 nextVec = inputVec * speed.Value * Time.fixedDeltaTime; // speed.Value 사용
        rg.MovePosition(rg.position + nextVec);
    }

    void LateUpdate()
    {
        // 게임이 진행 중이 아닐 때는 로직을 중단합니다.
        if (!GameManager.instance.isLive) return;

        // 이동 속도에 따라 애니메이터의 "Speed" 파라미터를 조절하여 걷기/서기 애니메이션을 제어합니다.
        an.SetFloat("Speed", inputVec.magnitude); // speed.Value 대신 inputVec.magnitude 사용

        // x축 입력이 있을 경우, 입력 방향에 따라 스프라이트를 좌우로 뒤집습니다.
        if (inputVec.x != 0)
        {
            sp.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // 게임이 진행 중이 아닐 때는 충돌 처리를 무시합니다.
        if (!GameManager.instance.isLive) return;

        // 시간 정지중에는 충돌 처리를 하지 않습니다.
        if (GameManager.instance.isTimeStopped || isInvincible) return;

        // 충돌한 오브젝트가 "Enemy" 태그를 가지고 있는지 확인합니다.
        if (collision.gameObject.CompareTag(GameTags.ENEMY))
        {
            // Enemy 컴포넌트를 가져와서 해당 적의 contactDamage를 사용합니다.
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 적과 충돌하고 있는 동안 지속적으로 체력이 감소합니다.
                // Time.deltaTime을 곱하여 프레임 속도에 관계없이 일정한 피해를 받도록 합니다.
                float actualDamage = CalculateDamageAfterDefense(enemy.contactDamage);
                GameManager.instance.health -= actualDamage * Time.deltaTime;
            }
        }

        // 체력이 0 미만으로 떨어지면 부활을 시도합니다.
        if (GameManager.instance.health <= 0)
        {
            TryRevive();
        }
    }

    /// <summary>
    /// 부활을 시도하고, 가능하면 부활 코루틴을 시작합니다. 불가능하면 게임 오버를 호출합니다.
    /// </summary>
    void TryRevive()
    {
        if (revive.Value > 0)
        {
            revive.BaseValue--; // 부활 횟수 차감
            StartCoroutine(ReviveRoutine());
        }
        else
        {
            // 플레이어의 자식 오브젝트(무기 등)를 모두 비활성화합니다.
            for (int i = 2; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            // 사망 애니메이션을 재생합니다.
            an.SetTrigger("Dead");
            // GameManager에 게임 오버를 알립니다.
            GameManager.instance.GameOver();
        }
    }

    /// <summary>
    /// 부활 시퀀스 (무적, 체력 회복, 이펙트)를 처리하는 코루틴입니다.
    /// </summary>
    IEnumerator ReviveRoutine()
    {
        // 1. 무적 상태 시작
        isInvincible = true;

        // 2. 부활 이펙트 재생
        GameObject reviveEffect = GameManager.instance.pool.Get("Revive"); // PoolManager에 등록된 태그 사용
        if (reviveEffect != null)
        {
            reviveEffect.transform.position = transform.position;
            reviveEffect.SetActive(true);
        }
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp); // 임시 효과음

        // 3. 체력을 절반 회복
        GameManager.instance.health = GameManager.instance.maxHealth * REVIVE_HEALTH_RATIO;

        // 4. 일정 시간 동안 무적 유지
        yield return new WaitForSeconds(REVIVE_INVINCIBLE_TIME);

        // 5. 무적 상태 종료
        isInvincible = false;
        // TODO: 무적 시각 효과 종료
        GameManager.instance.pool.ReturnToPool("Revive", reviveEffect); // 풀에 반홨
    }
    
    /// <summary>
    /// 방어력을 적용한 최종 데미지를 계산합니다.
    /// </summary>
    /// <param name="baseDamage">기본 데미지</param>
    /// <returns>방어력이 적용된 최종 데미지</returns>
    private float CalculateDamageAfterDefense(float baseDamage)
    {
        return baseDamage * (100f / (100f + defense.Value));
    }
}

