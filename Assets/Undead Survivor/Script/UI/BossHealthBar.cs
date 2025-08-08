using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보스 몬스터의 체력바를 관리하는 클래스입니다.
/// 보스 머리 위에 표시되며, 체력 변화에 따라 업데이트됩니다.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image fillImage;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private float smoothSpeed = 5f;
    
    private Transform bossTransform;
    private Camera mainCamera;
    private float maxHealth;
    private float currentHealth;
    
    void Awake()
    {
        mainCamera = Camera.main;
        bossTransform = transform.parent;
        
        // World Space Canvas 설정
        if (worldCanvas == null)
        {
            worldCanvas = GetComponent<Canvas>();
        }
        
        if (worldCanvas != null)
        {
            worldCanvas.worldCamera = mainCamera;
            worldCanvas.sortingOrder = 10; // 다른 UI보다 앞에 표시
        }
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (bossTransform != null && gameObject.activeInHierarchy)
        {
            // 보스 위치를 따라다니도록 위치 업데이트
            Vector3 targetPosition = bossTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            
            // 카메라를 항상 바라보도록 회전
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
    
    /// <summary>
    /// 체력바를 초기화합니다.
    /// </summary>
    /// <param name="bossName">보스 이름</param>
    /// <param name="maxHP">최대 체력</param>
    public void InitializeHealthBar(string bossName, float maxHP)
    {
        maxHealth = maxHP;
        currentHealth = maxHP;
        
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = maxHP;
        }
        
        UpdateHealthText();
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 체력을 업데이트합니다.
    /// </summary>
    /// <param name="currentHP">현재 체력</param>
    /// <param name="maxHP">최대 체력</param>
    public void UpdateHealth(float currentHP, float maxHP)
    {
        currentHealth = Mathf.Max(0, currentHP);
        maxHealth = maxHP;
        
        if (healthSlider != null)
        {
            // 부드러운 체력바 애니메이션
            StartCoroutine(SmoothHealthUpdate(currentHealth));
        }
        
        UpdateHealthText();
        UpdateHealthBarColor();
    }
    
    /// <summary>
    /// 체력바를 숨깁니다.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 체력바를 표시합니다.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
        }
    }
    
    private void UpdateHealthBarColor()
    {
        if (fillImage == null) return;
        
        float healthPercentage = currentHealth / maxHealth;
        
        // 체력 비율에 따른 색상 변경
        if (healthPercentage > 0.6f)
        {
            fillImage.color = Color.green;
        }
        else if (healthPercentage > 0.3f)
        {
            fillImage.color = Color.yellow;
        }
        else
        {
            fillImage.color = Color.red;
        }
    }
    
    private System.Collections.IEnumerator SmoothHealthUpdate(float targetHealth)
    {
        float startValue = healthSlider.value;
        float elapsed = 0f;
        float duration = 0.3f; // 애니메이션 지속 시간
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (healthSlider != null)
            {
                healthSlider.value = Mathf.Lerp(startValue, targetHealth, t);
            }
            
            yield return null;
        }
        
        if (healthSlider != null)
        {
            healthSlider.value = targetHealth;
        }
    }
}