using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class BladeofTruthWeapon : WeaponBase
{
    private float timer; // 공격 쿨타임 타이머
    private BladeofTruthWeaponData bladeData;
    private Coroutine hasteCoroutine; // 현재 적용 중인 이동 속도 버프 코루틴

    public override void Init(ItemData data)
    {
        base.Init(data);
        this.bladeData = data as BladeofTruthWeaponData;
        if (this.bladeData == null)
        {
            Debug.LogError("BladeofTruthWeapon에 할당된 ItemData가 BladeofTruthWeaponData 타입이 아닙니다!");
        }
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        if (timer > cooldown.Value)
        {
            timer = 0f;
            Attack();
        }
    }

    /// <summary>
    /// 플레이어 주변에 칼날 이펙트를 소환합니다.
    /// </summary> 
    private void Attack()
    {
        if (bladeData == null) return; // 데이터가 없으면 공격 중단

        GameObject effect = GameManager.instance.pool.Get(bladeData.projectileTag);
        if (effect == null) return;

        effect.transform.position = player.transform.position;

        BladeofTruthEffect effectLogic = effect.GetComponent<BladeofTruthEffect>();
        if (effectLogic != null)
        {
            effectLogic.Init(damage.Value, bladeData.damageDelay, attackArea.Value, bladeData.targetLayer, this);
            effect.SetActive(true);
            effectLogic.StartEffect(); // 이펙트 시작
        }

        // TODO: 적절한 공격 효과음 재생
    }

    /// <summary>
    /// 적중한 적의 수에 따라 플레이어에게 이동 속도 버프를 적용합니다.
    /// </summary>
    public void ApplyHasteEffect(int hitCount)
    {
        if (hitCount == 0) return; // 맞춘 적이 없으면 버프 없음

        // 이전에 적용된 버프가 있다면 중지하고 제거합니다.
        if (hasteCoroutine != null)
        {
            StopCoroutine(hasteCoroutine);
        }

        // 새로운 버프 적용 코루틴을 시작합니다.
        hasteCoroutine = StartCoroutine(HasteRoutine(hitCount));
    }

    private IEnumerator HasteRoutine(int hitCount)
    {
        // 1. 이동 속도 증가량 계산
        float hasteToAdd = bladeData.bonusHasteAmount + (bladeData.additionalHastePerHit * (hitCount - 1));
        hasteToAdd = Mathf.Min(hasteToAdd, bladeData.maxHasteAmount); // 최대 증가량 제한

        // 2. StatModifier 생성 및 적용
        StatModifier hasteModifier = new StatModifier(hasteToAdd, StatModifierType.Additive, this);
        player.speed.AddModifier(hasteModifier);

        // 3. 지정된 시간만큼 대기
        yield return new WaitForSeconds(bladeData.bonusHasteDuration);

        // 4. 시간 종료 후 모디파이어 제거
        player.speed.RemoveAllModifiersFromSource(this);
        hasteCoroutine = null; // 코루틴 참조 초기화
    }
}
