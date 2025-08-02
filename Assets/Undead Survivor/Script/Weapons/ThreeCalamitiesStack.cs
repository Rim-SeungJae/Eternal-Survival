using UnityEngine;
using System.Collections;

/// <summary>
/// 적에게 Three Calamities 스택을 관리하는 컴포넌트입니다.
/// </summary>
public class ThreeCalamitiesStack : MonoBehaviour
{
    private int currentStacks = 0;
    private float stackTimer = 0f;
    private HyejinWeaponEvo weaponRef;
    private Enemy enemy;
    
    [Header("# Visual Effects")]
    [Tooltip("현재 표시 중인 스택 시각 효과")]
    private GameObject currentStackEffect;
    
    [Tooltip("스택 표시 위치 오프셋")]
    public Vector3 stackOffset = new Vector3(0, 0, 0);

    [Tooltip("풀스택 초기화 대기 시간")]
    public float resetDelay = 1f;

    private string stackEffectTag;
    
    private Coroutine resetCoroutine; // 코루틴 참조 저장

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public void Init(string stackEffectTag)
    {
        this.stackEffectTag = stackEffectTag;
    }

    void Update()
    {
        // 스택 타이머 업데이트
        if (stackTimer > 0)
        {
            stackTimer -= Time.deltaTime;
            if (stackTimer <= 0)
            {
                // 스택 시간 만료 시 모든 스택 제거
                ClearAllStacks();
            }
        }
        
        // 스택 표시 위치 업데이트
        UpdateStackVisualPosition();
    }

    /// <summary>
    /// 스택 표시 위치를 업데이트합니다.
    /// </summary>
    private void UpdateStackVisualPosition()
    {
        if (currentStackEffect != null)
        {
            currentStackEffect.transform.position = transform.position + stackOffset;
        }
    }

    /// <summary>
    /// Three Calamities 스택을 추가합니다.
    /// </summary>
    /// <param name="weapon">무기 참조</param>
    /// <param name="duration">스택 지속 시간</param>
    public void AddStack(HyejinWeaponEvo weapon, float duration)
    {
        weaponRef = weapon;
        
        // 진행 중인 리셋 코루틴이 있으면 취소
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
            currentStacks = 0;
            stackTimer = 0f;
        }
        
        currentStacks++;
        stackTimer = duration;

        
        // 스택 시각 효과 업데이트
        UpdateStackVisual();
        
        // 3회 스택 달성 시 광역 공격 발동
        if (currentStacks >= 3)
        {
            TriggerCalamitiesExplosion();
            resetCoroutine = StartCoroutine(WaitAndResetStacks(resetDelay));
        }
    }

    /// <summary>
    /// 스택 시각 효과를 업데이트합니다.
    /// </summary>
    private void UpdateStackVisual()
    {
        // 기존 스택 효과가 있으면 스프라이트만 변경
        if (currentStackEffect != null)
        {
            CalamitiesStackEffect stackEffectComponent = currentStackEffect.GetComponent<CalamitiesStackEffect>();
            if (stackEffectComponent != null)
            {
                stackEffectComponent.SetStackLevel(currentStacks);
                return;
            }
        }
        
        // 기존 스택 효과가 없으면
        
        if (currentStacks > 0 && currentStacks <= 3)
        {
            
            // 오브젝트 풀에서 스택 효과 가져오기
            currentStackEffect = GameManager.instance.pool.Get(stackEffectTag);
            if (currentStackEffect != null)
            {
                currentStackEffect.transform.position = transform.position + stackOffset;
                currentStackEffect.SetActive(true);
                
                // 스택 효과 컴포넌트 초기화 및 스프라이트 설정
                CalamitiesStackEffect stackEffectComponent = currentStackEffect.GetComponent<CalamitiesStackEffect>();
                if (stackEffectComponent != null)
                {
                    stackEffectComponent.Init(this, currentStacks);
                }
            }
            else
            {
                Debug.LogWarning($"스택 효과 프리팹을 찾을 수 없습니다: {stackEffectTag}");
            }
        }
    }

    /// <summary>
    /// 현재 스택 효과를 제거합니다.
    /// </summary>
    private void RemoveCurrentStackEffect()
    {
        if (currentStackEffect != null)
        {
            Poolable poolable = currentStackEffect.GetComponent<Poolable>();
            if (poolable != null)
            {
                GameManager.instance.pool.ReturnToPool(poolable.poolTag, currentStackEffect);
            }
            else
            {
                currentStackEffect.SetActive(false);
            }
            currentStackEffect = null;
        }
        
    }

    /// <summary>
    /// 모든 스택을 제거합니다.
    /// </summary>
    private void ClearAllStacks()
    {
        currentStacks = 0;
        stackTimer = 0f;
        
        // 시각 효과 제거
        RemoveCurrentStackEffect();
    }

    private IEnumerator WaitAndResetStacks(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearAllStacks();
        resetCoroutine = null; // 코루틴 완료 후 참조 제거
    }

    /// <summary>
    /// Three Calamities 광역 공격을 발동합니다.
    /// </summary>
    private void TriggerCalamitiesExplosion()
    {
        if (weaponRef != null)
        {
            weaponRef.TriggerCalamitiesExplosion(transform.position);
        }
    }
    
    /// <summary>
    /// 컴포넌트가 비활성화 될 때 시각 효과도 정리합니다.
    /// </summary>
    void OnDisable()
    {
        ClearAllStacks();
    }
} 
