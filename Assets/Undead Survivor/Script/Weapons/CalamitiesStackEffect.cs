using UnityEngine;
using System.Collections;

/// <summary>
/// Three Calamities 스택 효과 프리팹을 관리하는 컴포넌트입니다.
/// </summary>
public class CalamitiesStackEffect : MonoBehaviour
{
    [Header("# Stack Effect Settings")]
    [Tooltip("스택 효과 스프라이트")]
    public SpriteRenderer stackSpriteRenderer;
    
    [Tooltip("스택별 스프라이트 배열 (0: 1스택, 1: 2스택, 2: 3스택)")]
    public Sprite[] stackSprites = new Sprite[3];
    
    [Tooltip("스택 효과 크기")]
    public float stackScale = 1f;

    
    private ThreeCalamitiesStack parentStack;
    private int currentStackLevel = 0;

    void Awake()
    {
        // SpriteRenderer가 설정되지 않은 경우 자동으로 찾기
        if (stackSpriteRenderer == null)
        {
            stackSpriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 초기 설정
        if (stackSpriteRenderer != null)
        {
            stackSpriteRenderer.sortingLayerName = "Default";
            stackSpriteRenderer.sortingOrder = 10;
        }
        
        // 크기 설정
        transform.localScale = Vector3.one * stackScale;
    }

    /// <summary>
    /// 스택 효과를 초기화합니다.
    /// </summary>
    /// <param name="parentStack">부모 스택 컴포넌트</param>
    /// <param name="stackLevel">스택 레벨 (1-3)</param>
    public void Init(ThreeCalamitiesStack parentStack, int stackLevel)
    {
        this.parentStack = parentStack;
        SetStackLevel(stackLevel);
    }

    /// <summary>
    /// 스택 레벨에 따라 스프라이트를 변경합니다.
    /// </summary>
    /// <param name="stackLevel">스택 레벨 (1-3)</param>
    public void SetStackLevel(int stackLevel)
    {
        currentStackLevel = stackLevel;
        
        if (stackSpriteRenderer != null && stackLevel >= 1 && stackLevel <= 3)
        {
            int spriteIndex = stackLevel - 1; // 1스택 = 인덱스 0
            
            if (spriteIndex < stackSprites.Length && stackSprites[spriteIndex] != null)
            {
                stackSpriteRenderer.sprite = stackSprites[spriteIndex];
            }
            else
            {
                Debug.LogWarning($"스택 레벨 {stackLevel}에 해당하는 스프라이트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// 현재 스택 레벨을 반환합니다.
    /// </summary>
    public int GetStackLevel()
    {
        return currentStackLevel;
    }

    void OnDisable()
    {
        // 참조 정리
        parentStack = null;
        currentStackLevel = 0;
    }
} 