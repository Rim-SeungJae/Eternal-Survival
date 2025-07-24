using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
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
    
    // 다중 swirl 레이어 참조 (프리팹에 미리 설정됨)
    private SpriteRenderer[] swirlLayers;

    private float damage;
    private float damageDelay;
    private float attackRadius;
    private LayerMask targetLayer;
    private BladeofTruthWeapon weaponInstance;

    void Awake()
    {
        // 프리팹이 인스턴스화될 때, 이름으로 하위 오브젝트를 찾아 참조를 설정합니다.
        bladeSprite = transform.Find("Blade")?.GetComponent<SpriteRenderer>();
        circleSprite = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        swirlSprite = transform.Find("Swirl_Layer1")?.GetComponent<SpriteRenderer>(); // 이름 변경됨
        
        // 모든 swirl 레이어들을 찾아서 배열에 저장
        InitializeSwirlLayers();
    }
    
    /// <summary>
    /// 프리팹에 미리 설정된 swirl 레이어들을 찾아서 배열에 저장합니다.
    /// </summary>
    private void InitializeSwirlLayers()
    {
        // 프리팹에 있는 모든 swirl 레이어들을 찾습니다
        List<SpriteRenderer> foundLayers = new List<SpriteRenderer>();
        
        // Swirl_Layer1, Swirl_Layer2, Swirl_Layer3를 순서대로 찾습니다
        for (int i = 1; i <= 3; i++)
        {
            Transform layerTransform = transform.Find($"Swirl_Layer{i}");
            if (layerTransform != null)
            {
                SpriteRenderer layerRenderer = layerTransform.GetComponent<SpriteRenderer>();
                if (layerRenderer != null)
                {
                    foundLayers.Add(layerRenderer);
                }
            }
        }
        
        swirlLayers = foundLayers.ToArray();
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

        // 모든 swirl 레이어에 대해 크기 조정
        if(swirlSprite != null && swirlLayers != null)
        {
            // swirl은 중앙 pivot 사용 가정, 지름을 기준으로 스케일 계산
            float swirlDiameter = swirlSprite.sprite.bounds.size.x;
            float baseSwirScale = (attackRadius * 2) / swirlDiameter;
            
            // 각 레이어의 기본 스케일 비율 정의 (프리팹에서 설정된 비율)
            float[] layerScaleRatios = { 1.0f, 0.8f, 0.6f }; // Layer1: 100%, Layer2: 80%, Layer3: 60%
            
            for (int i = 0; i < swirlLayers.Length; i++)
            {
                if (swirlLayers[i] != null)
                {
                    // 각 레이어마다 정의된 비율을 적용하여 attackRadius에 맞게 조정
                    float layerRatio = i < layerScaleRatios.Length ? layerScaleRatios[i] : 1.0f;
                    float layerScale = baseSwirScale * layerRatio;
                    
                    swirlLayers[i].transform.localScale = new Vector3(layerScale, layerScale, 1f);
                    swirlLayers[i].transform.localPosition = Vector3.zero; // 중앙에 위치
                }
            }
        }
    }

    public void StartEffect()
    {
        StartCoroutine(EffectRoutine());
    }

    private IEnumerator EffectRoutine()
    {
        // 회전 시작과 동시에 모든 swirl 레이어 활성화
        if (swirlLayers != null)
        {
            for (int i = 0; i < swirlLayers.Length; i++)
            {
                if (swirlLayers[i] != null)
                {
                    swirlLayers[i].gameObject.SetActive(true);
                    Color layerColor = swirlLayers[i].color;
                    layerColor.a = 0; // 투명하게 시작
                    swirlLayers[i].color = layerColor;
                }
            }
        }

        // 칼날 회전 애니메이션 시작 (2바퀴 = 720도)
        if (bladeSprite != null)
        {
            transform.DOLocalRotate(new Vector3(0, 0, -720), damageDelay * 2, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuad);
        }

        // 회전 진행도에 따른 swirl 강도 조절 및 데미지 타이밍 관리
        float startTime = Time.time;
        float rotationDuration = damageDelay * 2;
        bool damageApplied = false; // 데미지가 이미 적용되었는지 확인
        
        while (Time.time - startTime < rotationDuration)
        {
            float progress = (Time.time - startTime) / rotationDuration;
            
            // 모든 swirl 레이어의 강도 조절
            if (swirlLayers != null)
            {
                // 회전 중반까지 강도 증가, 이후 감소 (0.5에서 최대값)
                float baseIntensity;
                if (progress <= 0.5f)
                {
                    // 0~0.5 구간: 0에서 1로 증가
                    baseIntensity = progress * 2f;
                }
                else
                {
                    // 0.5~1.0 구간: 1에서 0으로 감소
                    baseIntensity = (1f - progress) * 2f;
                }
                
                for (int i = 0; i < swirlLayers.Length; i++)
                {
                    if (swirlLayers[i] != null)
                    {
                        Color layerColor = swirlLayers[i].color;
                        // 각 레이어마다 기본 투명도에 맞춰 강도 적용
                        float originalAlpha = i == 0 ? 1f : (i == 1 ? 0.8f : 0.6f); // 원래 투명도
                        layerColor.a = baseIntensity * originalAlpha;
                        swirlLayers[i].color = layerColor;
                    }
                }
            }
            
            // 회전 중반(가장 빠른 시점)에서 데미지 적용
            if (!damageApplied && progress >= 0.5f)
            {
                // 이펙트 위치를 중심으로 원형 범위 내의 모든 적을 찾습니다.
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

                // 피해 처리가 끝난 후, 부모 무기에 적중한 적의 수를 알립니다.
                if (weaponInstance != null)
                {
                    weaponInstance.ApplyHasteEffect(hitCount);
                }
                
                damageApplied = true; // 데미지 적용 완료 표시
            }
            
            yield return null;
        }

        // 회전 완료 후 모든 swirl 레이어 비활성화
        if (swirlLayers != null)
        {
            for (int i = 0; i < swirlLayers.Length; i++)
            {
                if (swirlLayers[i] != null)
                {
                    swirlLayers[i].gameObject.SetActive(false);
                }
            }
        }

        // 이펙트 오브젝트를 풀에 반환합니다.
        yield return new WaitForSeconds(0.1f); // 이펙트가 자연스럽게 사라질 시간
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
