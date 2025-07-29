using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무기 진화 UI를 관리하는 클래스입니다.
/// 진화 가능한 무기들을 표시하고 진화를 실행합니다.
/// </summary>
public class EvolutionUI : MonoBehaviour
{
    [Header("# UI References")]
    [Tooltip("진화 후보 무기들을 표시할 컨테이너")]
    public Transform evolutionContainer;
    
    [Tooltip("진화 후보 무기 UI 프리팹")]
    public GameObject evolutionCandidatePrefab;
    
    [Tooltip("닫기 버튼")]
    public Button closeButton;
    
    [Tooltip("배경 패널")]
    public Image backgroundPanel;
    
    private List<EvolutionCandidate> candidates;
    private List<GameObject> candidateUIs = new List<GameObject>();
    
    void Awake()
    {
        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUI);
        }
        
        // 배경 클릭으로 닫기
        if (backgroundPanel != null)
        {
            backgroundPanel.GetComponent<Button>().onClick.AddListener(CloseUI);
        }
    }
    
    /// <summary>
    /// 진화 UI를 초기화합니다.
    /// </summary>
    /// <param name="evolutionCandidates">진화 가능한 무기 목록</param>
    public void Initialize(List<EvolutionCandidate> evolutionCandidates)
    {
        this.candidates = evolutionCandidates;
        
        // 기존 UI 요소들 정리
        ClearCandidateUIs();
        
        // 진화 후보 무기들 UI 생성
        CreateCandidateUIs();
        
        // UI 표시
        gameObject.SetActive(true);
        
        // 게임 일시정지
        GameManager.instance.Stop();
        
        // 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
    }
    
    /// <summary>
    /// 진화 후보 무기들의 UI를 생성합니다.
    /// </summary>
    private void CreateCandidateUIs()
    {
        if (evolutionContainer == null || evolutionCandidatePrefab == null)
        {
            Debug.LogError("진화 UI에 필요한 참조가 설정되지 않았습니다.");
            return;
        }
        
        foreach (EvolutionCandidate candidate in candidates)
        {
            GameObject candidateUI = Instantiate(evolutionCandidatePrefab, evolutionContainer);
            EvolutionCandidateUI candidateComponent = candidateUI.GetComponent<EvolutionCandidateUI>();
            
            if (candidateComponent != null)
            {
                candidateComponent.Initialize(candidate, this);
            }
            
            candidateUIs.Add(candidateUI);
        }
    }
    
    /// <summary>
    /// 기존 진화 후보 UI들을 정리합니다.
    /// </summary>
    private void ClearCandidateUIs()
    {
        foreach (GameObject ui in candidateUIs)
        {
            if (ui != null)
            {
                Destroy(ui);
            }
        }
        candidateUIs.Clear();
    }
    
    /// <summary>
    /// 진화를 실행합니다.
    /// </summary>
    /// <param name="candidate">진화할 후보</param>
    public void ExecuteEvolution(EvolutionCandidate candidate)
    {
        bool success = WeaponEvolutionManager.Instance.EvolveWeapon(
            candidate.originalItem, 
            candidate.evolutionData
        );
        
        if (success)
        {
            CloseUI();
        }
    }
    
    /// <summary>
    /// 진화 UI를 닫습니다.
    /// </summary>
    public void CloseUI()
    {
        ClearCandidateUIs();
        gameObject.SetActive(false);
        GameManager.instance.Resume();
    }
    
    void OnDestroy()
    {
        // 이벤트 리스너 정리
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseUI);
        }
        
        if (backgroundPanel != null)
        {
            backgroundPanel.GetComponent<Button>().onClick.RemoveListener(CloseUI);
        }
    }
}

/// <summary>
/// 개별 진화 후보 무기의 UI를 관리하는 클래스입니다.
/// </summary>
public class EvolutionCandidateUI : MonoBehaviour
{
    [Header("# UI References")]
    [Tooltip("원본 무기 아이콘")]
    public Image originalWeaponIcon;
    
    [Tooltip("진화 화살표 아이콘")]
    public Image evolutionArrowIcon;
    
    [Tooltip("진화된 무기 아이콘")]
    public Image evolvedWeaponIcon;
    
    [Tooltip("원본 무기 이름")]
    public Text originalWeaponName;
    
    [Tooltip("진화된 무기 이름")]
    public Text evolvedWeaponName;
    
    [Tooltip("진화 설명")]
    public Text evolutionDescription;
    
    [Tooltip("진화 버튼")]
    public Button evolveButton;
    
    [Tooltip("필요한 Gear 아이템들 표시")]
    public Transform requiredGearsContainer;
    
    [Tooltip("Gear 아이템 UI 프리팹")]
    public GameObject gearItemPrefab;
    
    private EvolutionCandidate candidate;
    private EvolutionUI parentUI;
    
    /// <summary>
    /// 진화 후보 UI를 초기화합니다.
    /// </summary>
    /// <param name="evolutionCandidate">진화 후보 데이터</param>
    /// <param name="ui">부모 UI 참조</param>
    public void Initialize(EvolutionCandidate evolutionCandidate, EvolutionUI ui)
    {
        this.candidate = evolutionCandidate;
        this.parentUI = ui;
        
        // UI 요소들 설정
        SetupWeaponIcons();
        SetupWeaponNames();
        SetupEvolutionDescription();
        SetupRequiredGears();
        SetupEvolveButton();
    }
    
    /// <summary>
    /// 무기 아이콘들을 설정합니다.
    /// </summary>
    private void SetupWeaponIcons()
    {
        if (originalWeaponIcon != null)
        {
            originalWeaponIcon.sprite = candidate.originalItem.data.itemIcon;
        }
        
        if (evolvedWeaponIcon != null)
        {
            evolvedWeaponIcon.sprite = candidate.evolutionData.evolvedWeapon.itemIcon;
        }
    }
    
    /// <summary>
    /// 무기 이름들을 설정합니다.
    /// </summary>
    private void SetupWeaponNames()
    {
        if (originalWeaponName != null)
        {
            originalWeaponName.text = candidate.originalItem.data.itemName;
        }
        
        if (evolvedWeaponName != null)
        {
            evolvedWeaponName.text = candidate.evolutionData.evolvedWeapon.itemName;
        }
    }
    
    /// <summary>
    /// 진화 설명을 설정합니다.
    /// </summary>
    private void SetupEvolutionDescription()
    {
        if (evolutionDescription != null)
        {
            evolutionDescription.text = candidate.evolutionData.evolutionDescription;
        }
    }
    
    /// <summary>
    /// 필요한 Gear 아이템들을 표시합니다.
    /// </summary>
    private void SetupRequiredGears()
    {
        if (requiredGearsContainer == null || gearItemPrefab == null)
        {
            return;
        }
        
        // 기존 Gear UI들 정리
        foreach (Transform child in requiredGearsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 필요한 Gear 아이템들 UI 생성
        foreach (GearData requiredGear in candidate.evolutionData.requiredGears)
        {
            GameObject gearUI = Instantiate(gearItemPrefab, requiredGearsContainer);
            Image gearIcon = gearUI.GetComponent<Image>();
            
            if (gearIcon != null)
            {
                gearIcon.sprite = requiredGear.itemIcon;
                
                // 보유 여부에 따라 투명도 조절
                Player player = GameManager.instance.player;
                Item gearItem = player.items.Find(item => item.data == requiredGear);
                bool isOwned = gearItem != null && gearItem.level > 0;
                
                Color iconColor = gearIcon.color;
                iconColor.a = isOwned ? 1f : 0.3f;
                gearIcon.color = iconColor;
            }
        }
    }
    
    /// <summary>
    /// 진화 버튼을 설정합니다.
    /// </summary>
    private void SetupEvolveButton()
    {
        if (evolveButton != null)
        {
            evolveButton.onClick.RemoveAllListeners();
            evolveButton.onClick.AddListener(OnEvolveButtonClick);
        }
    }
    
    /// <summary>
    /// 진화 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnEvolveButtonClick()
    {
        if (parentUI != null)
        {
            parentUI.ExecuteEvolution(candidate);
        }
    }
    
    void OnDestroy()
    {
        // 이벤트 리스너 정리
        if (evolveButton != null)
        {
            evolveButton.onClick.RemoveAllListeners();
        }
    }
} 