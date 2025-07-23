using UnityEngine;
using System.Collections;
using DG.Tweening; // DOTween 네임스페이스 추가

/// <summary>
/// Blade of Truth 이펙트의 시각 효과와 지연된 피해 처리를 담당합니다.
/// </summary>
public class BladeofTruthEffect : MonoBehaviour
{
    // 하위 오브젝트에 대한 참조는 Awake에서 동적으로 찾습니다.
    private SpriteRenderer bladeSprite;
    private SpriteRenderer circleSprite;
    private SpriteRenderer swirlSprite;

    private float damage;
    private float damageDelay;
    private float attackRadius;
    private LayerMask targetLayer;
    private BladeofTruthWeapon weaponInstance;

    void Awake()
    {
        // 프리팹이 인스턴스화될 때, 이름으로 하위 오브젝트를 찾아 참조를 설정합니다.
        // 이 로직은 스크립트가 활성화될 때 한 번만 실행되어야 합니다.
        bladeSprite = transform.Find("Blade")?.GetComponent<SpriteRenderer>();
        circleSprite = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        swirlSprite = transform.Find("Swirl")?.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position = GameManager.instance.player.transform.position;        
    }

    /// <summary>
    /// 이펙트를 초기화하고 애니메이션 및 피해 처리 코루틴을 시작합니다.
    /// </summary>
    public void Init(float dmg, float delay, float radius, LayerMask layer, BladeofTruthWeapon weapon)
    {
        this.damage = dmg;
        this.damageDelay = delay;
        this.attackRadius = radius;
        this.targetLayer = layer;
        this.weaponInstance = weapon;

        // 각 스프라이트의 크기를 공격 범위에 맞게 조절
        if (bladeSprite != null)
        {
            float bladeHeight = bladeSprite.sprite.bounds.size.y;
            float bladeScale = attackRadius / bladeHeight;
            bladeSprite.transform.localScale = new Vector3(bladeScale, bladeScale, 1f);

        }

        if (circleSprite != null)
        {
            // circle은 중앙 pivot 사용 가정, 지름을 기준으로 스케일 계산
            float circleDiameter = circleSprite.sprite.bounds.size.x;
            float circleScale = (attackRadius * 2) / circleDiameter;
            circleSprite.transform.localScale = new Vector3(circleScale, circleScale, 1f);
            circleSprite.transform.localPosition = Vector3.zero; // 중앙에 위치
        }

        if(swirlSprite != null)
        {
            // swirl은 중앙 pivot 사용 가정, 지름을 기준으로 스케일 계산
            float swirlDiameter = swirlSprite.sprite.bounds.size.x;
            float swirlScale = (attackRadius * 2) / swirlDiameter;
            swirlSprite.transform.localScale = new Vector3(swirlScale, swirlScale, 1f);
            swirlSprite.transform.localPosition = Vector3.zero; // 중앙에 위치
        }
    }

    public void StartEffect()
    {
        StartCoroutine(EffectRoutine());
    }

    private IEnumerator EffectRoutine()
    {
        // 칼날 회전 애니메이션 시작 (2바퀴 = 720도)
        // damageDelay 시간 동안 회전하도록 설정합니다.
        if (bladeSprite != null)
        {
            // 부모(이펙트 루트)를 중심으로 회전해야 하므로, 이펙트 루트를 회전시킵니다.
            // 칼날 자체는 이미 localPosition으로 위치가 잡혀있습니다.
            transform.DOLocalRotate(new Vector3(0, 0, -720), damageDelay * 2, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuad);
        }

        // 이펙트 애니메이션(회전)이 끝날 때까지 대기합니다.
        yield return new WaitForSeconds(damageDelay);

        // 3. 지연 후, 이펙트 위치를 중심으로 원형 범위 내의 모든 적을 찾습니다.
        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(transform.position, attackRadius, targetLayer);
        int hitCount = 0;

        foreach (var enemyCollider in enemiesToDamage)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitCount++;
            }
        }

        // 4. 피해 처리가 끝난 후, 부모 무기에 적중한 적의 수를 알립니다.
        if (weaponInstance != null)
        {
            weaponInstance.ApplyHasteEffect(hitCount);
        }

        // 5. 이펙트 오브젝트를 풀에 반환합니다.
        // 잠시 후 비활성화하여 이펙트가 자연스럽게 사라질 시간을 줍니다.
        yield return new WaitForSeconds(damageDelay*2 + 0.1f); // 회전 애니메이션과 이펙트 지속 시간 고려
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
