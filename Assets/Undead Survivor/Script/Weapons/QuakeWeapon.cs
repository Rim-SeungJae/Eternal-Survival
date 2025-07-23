using UnityEngine;

/// <summary>
/// 플레이어의 이동 거리에 따라 주기적으로 광역 피해를 주는 'Quake' 무기 로직을 처리합니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class QuakeWeapon : WeaponBase
{
    private float distanceTraveled = 0f; // 마지막 발동 후 이동한 거리
    private Vector3 lastPosition; // 마지막 위치 저장용
    private bool isQuakeActive = false; // Quake 이펙트가 현재 활성화 중인지 여부

    public override void Awake()
    {
        base.Awake(); // 부모 클래스의 Awake()를 먼저 호출합니다.
    }

    void Start()
    {
        // 플레이어의 초기 위치 설정
        lastPosition = player.transform.position;
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        // Quake 이펙트가 활성화 중이 아닐 때만 이동 거리를 측정합니다.
        if (!isQuakeActive)
        {
            float currentDistance = Vector3.Distance(player.transform.position, lastPosition);
            distanceTraveled += currentDistance;
            
            // 누적 이동 거리가 최종 계산된 발동 거리(cooldown.Value)를 넘어서면 공격을 실행합니다.
            if (distanceTraveled >= cooldown.Value)
            {
                Attack();
                distanceTraveled = 0f; // 이동 거리 초기화
                isQuakeActive = true; // Quake 이펙트 활성화 상태로 설정
            }
        }
        
        // 매 프레임마다 마지막 위치 갱신 (isQuakeActive 상태와 관계없이)
        lastPosition = player.transform.position;
    }

    public override void Init(ItemData data)
    {
        base.Init(data); // 부모 클래스의 Init()을 호출하여 기본 설정을 적용

        // 무기 초기화 시 플레이어의 현재 위치를 저장하여 이동 거리 측정을 시작합니다.
        lastPosition = player.transform.position;
    }

    /// <summary>
    /// 주변의 모든 적에게 광역 피해를 줍니다。
    /// </summary>
    private void Attack()
    {
        WeaponData weaponData = itemData as WeaponData;
        // 이펙트 풀에서 이펙트 오브젝트를 가져옵니다.
        GameObject effect = GameManager.instance.pool.Get(weaponData.projectileTag);
        if (effect == null)
        {
            // 이펙트 생성에 실패하면 isQuakeActive를 즉시 false로 되돌려 다음 발동을 허용합니다.
            isQuakeActive = false;
            Debug.LogWarning($"PoolManager에서 태그 '{weaponData.projectileTag}'에 해당하는 이펙트를 가져오지 못했습니다. PoolManager 설정을 확인하세요.");
            return;
        }

        effect.transform.position = player.transform.position;
        // 최종 계산된 범위(attackArea.Value)를 이펙트의 크기에 적용합니다.
        effect.transform.localScale = Vector3.one * attackArea.Value;

        // 이펙트 스크립트에 최종 계산된 데미지와 지속시간, 그리고 이 QuakeWeapon 인스턴스를 전달합니다.
        QuakeEffect effectLogic = effect.GetComponent<QuakeEffect>();
        if (effectLogic != null)
        {
            effectLogic.Init(damage.Value, duration.Value, this);
        }
        effect.SetActive(true); // 오브젝트 활성화

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Melee); // 적절한 효과음으로 변경 가능
    }

    /// <summary>
    /// Quake 이펙트가 종료되었을 때 QuakeEffect 스크립트로부터 호출됩니다.
    /// </summary>
    public void OnQuakeEffectFinished()
    {
        isQuakeActive = false;
    }
}