using UnityEngine;

/// <summary>
/// 플레이어가 지나간 자리에 독장판을 생성하는 '유독성 발자국' 무기 로직을 처리합니다.
/// WeaponBase를 상속받아 공통 로직을 재사용합니다.
/// </summary>
public class NoxiousAftermathWeapon : WeaponBase
{
    private float timer;

    public override void Awake()
    {
        base.Awake(); // 부모 클래스의 Awake()를 먼저 호출합니다.
    }

    void Start()
    {
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        if (timer > cooldown.Value)
        {
            timer = 0f;
            // 플레이어가 이동한 위치에 독장판을 생성합니다.
            SpawnPuddle(player.transform.position);
        }
    }

    /// <summary>
    /// 독장판을 생성하고 초기화합니다.
    /// </summary>
    /// <param name="spawnPosition">독장판이 생성될 위치</param>
    private void SpawnPuddle(Vector3 spawnPosition)
    {
        GameObject puddle = GameManager.instance.pool.Get(itemData.projectileTag);
        if (puddle == null)
        {
            Debug.LogWarning($"PoolManager에서 태그 '{itemData.projectileTag}'에 해당하는 독장판 프리팹을 가져오지 못했습니다. PoolManager 설정을 확인하세요.");
            return;
        }

        puddle.transform.position = spawnPosition;
        // attackArea.Value를 독장판의 크기에 적용합니다.
        puddle.transform.localScale = Vector3.one * attackArea.Value;

        NoxiousAftermathEffect puddleLogic = puddle.GetComponent<NoxiousAftermathEffect>();
        if (puddleLogic != null)
        {
            // damage, duration과 함께 count를 피해 간격(cooldown)으로 전달합니다.
            puddleLogic.Init(damage.Value, duration.Value);
        }
        puddle.SetActive(true); // 오브젝트 활성화

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Melee); // 효과음 업데이트 예정
    }
}
