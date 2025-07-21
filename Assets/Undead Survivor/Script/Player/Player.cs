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

    [Tooltip("획득한 아이템 목록")]
    public List<Item> items;

    [Tooltip("플레이어의 입력 벡터")]
    public Vector2 inputVec;
    [Tooltip("플레이어의 이동 속도")]
    public ModifiableStat speed; // float에서 ModifiableStat으로 변경
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

    // CharacterDataSO에서 가져온 무기 관련 배율을 외부에 노출합니다.
    public float WeaponSpeedMultiplier => currentCharacterData.weaponSpeedMultiplier;
    public float WeaponRateMultiplier => currentCharacterData.weaponRateMultiplier;

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
        items = new List<Item>(); // 아이템 리스트 초기화
    }

    /// <summary>
    /// 현재 보유한 무기의 개수를 반환합니다.
    /// </summary>
    public int WeaponCount
    {
        get { return items.Count(item => item.data.itemAction is Action_Weapon); }
    }

    /// <summary>
    /// 현재 보유한 장비의 개수를 반환합니다.
    /// </summary>
    public int GearCount
    {
        get { return items.Count(item => item.data.itemAction is Action_StatBoostGear); }
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
        if (GameManager.instance.isTimeStopped) return;

        // 충돌한 오브젝트가 "Enemy" 태그를 가지고 있는지 확인합니다.
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Enemy 컴포넌트를 가져와서 해당 적의 contactDamage를 사용합니다.
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 적과 충돌하고 있는 동안 지속적으로 체력이 감소합니다.
                // Time.deltaTime을 곱하여 프레임 속도에 관계없이 일정한 피해를 받도록 합니다.
                GameManager.instance.health -= enemy.contactDamage * Time.deltaTime;
            }
        }

        // 체력이 0 미만으로 떨어지면 사망 처리 로직을 실행합니다.
        if (GameManager.instance.health < 0)
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
}

