using UnityEngine;
using System.Collections;

/// <summary>
/// 혜진 진화 무기의 Three Calamities 스택을 관리하는 통합 컴포넌트입니다.
/// 기존의 ThreeCalamitiesStack과 CalamitiesStackEffect 기능을 모두 포함합니다.
/// </summary>
public class HyejinWeaponEvoMark : MonoBehaviour
{
    [Header("# Stack Settings")]
    private int currentStacks = 0;
    private float stackTimer = 0f;
    private float stackDuration;
    private float explosionDamage;
    private HyejinWeaponEvo weaponRef;
    private Enemy targetEnemy;
    
    [Header("# Visual Settings")]
    [Tooltip("스택별 스프라이트 배열 (0: 1스택, 1: 2스택, 2: 3스택)")]
    public Sprite[] stackSprites = new Sprite[3];
    
    [Tooltip("스택 효과 크기")]
    public float stackScale = 1f;
    
    [Tooltip("스택 표시 위치 오프셋")]
    public Vector3 stackOffset = new Vector3(0, 0.5f, 0);
    
    [Tooltip("풀스택 후 초기화 대기 시간")]
    public float resetDelay = 1f;
    
    private SpriteRenderer stackSpriteRenderer;
    private Coroutine resetCoroutine;
    
    void Awake()
    {
        // SpriteRenderer 컴포넌트 가져오기
        stackSpriteRenderer = GetComponent<SpriteRenderer>();
        if (stackSpriteRenderer == null)
        {
            stackSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 렌더링 설정
        stackSpriteRenderer.sortingLayerName = "Default";
        stackSpriteRenderer.sortingOrder = 10;
        
        // 크기 설정
        transform.localScale = Vector3.one * stackScale;
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Three Calamities 마크를 초기화합니다.
    /// </summary>
    /// <param name="weapon">무기 참조</param>
    /// <param name="damage">폭발 피해량</param>
    /// <param name="duration">스택 지속 시간</param>
    public void InitializeMark(HyejinWeaponEvo weapon, float damage, float duration)
    {
        weaponRef = weapon;
        explosionDamage = damage;
        stackDuration = duration;
        
        // 부모 오브젝트에서 Enemy 컴포넌트 찾기
        targetEnemy = GetComponentInParent<Enemy>();
        
        if (targetEnemy == null)
        {
            Debug.LogError("HyejinWeaponEvoMark: Enemy 컴포넌트를 찾을 수 없습니다!");
            RemoveMark();
            return;
        }
        
        // 기존 마크가 있는지 확인하고 스택 추가
        HyejinWeaponEvoMark[] existingMarks = targetEnemy.GetComponentsInChildren<HyejinWeaponEvoMark>();
        foreach (HyejinWeaponEvoMark existingMark in existingMarks)
        {
            if (existingMark != this && existingMark != null)
            {
                // 기존 마크에 스택 추가
                existingMark.AddStack();
                RemoveMark();
                return;
            }
        }
        
        // 새로운 마크 시작
        AddStack();
    }
    
    /// <summary>
    /// 스택을 추가합니다.
    /// </summary>
    public void AddStack()
    {
        // 기존 리셋 코루틴이 있으면 중단
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
        
        // 3스택 상태에서 추가 스택이 오면 즉시 폭발
        if (currentStacks >= 3)
        {
            TriggerCalamitiesExplosion();
            return;
        }
        
        currentStacks++;
        stackTimer = stackDuration;
        
        // 시각적 효과 업데이트
        UpdateStackVisual();
        
        // 3스택 달성 시 폭발 발동
        if (currentStacks >= 3)
        {
            TriggerCalamitiesExplosion();
            resetCoroutine = StartCoroutine(WaitAndResetStacks(resetDelay));
        }
        
        Debug.Log($"적 {targetEnemy.name}에게 Three Calamities 스택 추가! 현재 스택: {currentStacks}");
    }
    
    /// <summary>
    /// 스택 시각적 효과를 업데이트합니다.
    /// </summary>
    private void UpdateStackVisual()
    {
        if (currentStacks >= 1 && currentStacks <= 3)
        {
            int spriteIndex = currentStacks - 1; // 1스택 = 인덱스 0
            
            if (spriteIndex < stackSprites.Length && stackSprites[spriteIndex] != null)
            {
                stackSpriteRenderer.sprite = stackSprites[spriteIndex];
                
                // 스택 수에 따라 색상 변경 (선택사항)
                Color stackColor = Color.white;
                switch (currentStacks)
                {
                    case 1:
                        stackColor = new Color(1f, 1f, 0f, 0.8f); // 노란색
                        break;
                    case 2:
                        stackColor = new Color(1f, 0.5f, 0f, 0.8f); // 주황색
                        break;
                    case 3:
                        stackColor = new Color(1f, 0f, 0f, 0.8f); // 빨간색
                        break;
                }
                stackSpriteRenderer.color = stackColor;
            }
            else
            {
                Debug.LogWarning($"스택 레벨 {currentStacks}에 해당하는 스프라이트가 없습니다.");
            }
        }
    }
    
    /// <summary>
    /// Three Calamities 폭발을 발동합니다.
    /// </summary>
    private void TriggerCalamitiesExplosion()
    {
        if (weaponRef != null && targetEnemy != null)
        {
            weaponRef.TriggerCalamitiesExplosion(targetEnemy.transform.position);
            
            // 폭발 효과음 재생
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
            
            Debug.Log($"Three Calamities 폭발 발동! 위치: {targetEnemy.transform.position}");
        }
    }
    
    /// <summary>
    /// 스택 지속시간 관리 및 자동 초기화
    /// </summary>
    void Update()
    {
        if (stackTimer > 0)
        {
            stackTimer -= Time.deltaTime;
            if (stackTimer <= 0)
            {
                // 스택 시간 만료 시 모든 스택 제거
                ClearAllStacks();
            }
        }
        
        // 적이 사라졌으면 마크도 제거
        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
        {
            RemoveMark();
            return;
        }
        
        // 스택 표시 위치 업데이트
        UpdateStackPosition();
    }
    
    /// <summary>
    /// 스택 표시 위치를 업데이트합니다.
    /// </summary>
    private void UpdateStackPosition()
    {
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.transform.position + stackOffset;
        }
    }
    
    /// <summary>
    /// 모든 스택을 제거합니다.
    /// </summary>
    private void ClearAllStacks()
    {
        currentStacks = 0;
        stackTimer = 0f;
        stackSpriteRenderer.sprite = null;
        
        RemoveMark();
    }
    
    /// <summary>
    /// 스택 지속시간 관리 코루틴
    /// </summary>
    private IEnumerator WaitAndResetStacks(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearAllStacks();
        resetCoroutine = null;
    }
    
    /// <summary>
    /// 마크를 제거하고 정리합니다.
    /// </summary>
    public void RemoveMark()
    {
        // 진행 중인 코루틴 정리
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
        
        // 풀에 반환
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
    
    /// <summary>
    /// 현재 스택 수를 반환합니다.
    /// </summary>
    public int GetCurrentStacks()
    {
        return currentStacks;
    }
    
    void OnDisable()
    {
        // 정리 작업
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
        
        currentStacks = 0;
        stackTimer = 0f;
        targetEnemy = null;
        weaponRef = null;
    }
}