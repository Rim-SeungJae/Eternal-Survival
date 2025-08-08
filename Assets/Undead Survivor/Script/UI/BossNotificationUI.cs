using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보스 등장 및 처치 알림을 표시하는 UI 클래스입니다.
/// </summary>
public class BossNotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Image bossIcon;
    [SerializeField] private Animator notificationAnimator;
    
    [Header("Notification Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private AudioClip bossAppearSound;
    [SerializeField] private AudioClip bossDefeatedSound;
    
    [Header("Colors")]
    [SerializeField] private Color appearanceColor = Color.red;
    [SerializeField] private Color defeatedColor = Color.gold;
    
    private static BossNotificationUI instance;
    public static BossNotificationUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<BossNotificationUI>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 초기에는 비활성화
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 보스 등장 알림을 표시합니다.
    /// </summary>
    /// <param name="bossName">보스 이름</param>
    /// <param name="icon">보스 아이콘 (선택사항)</param>
    public void ShowBossAppearance(string bossName, Sprite icon = null)
    {
        ShowNotification("보스 등장!", bossName, appearanceColor, icon);
    }
    
    /// <summary>
    /// 보스 처치 알림을 표시합니다.
    /// </summary>
    /// <param name="bossName">보스 이름</param>
    /// <param name="icon">보스 아이콘 (선택사항)</param>
    public void ShowBossDefeated(string bossName, Sprite icon = null)
    {
        ShowNotification("보스 처치!", bossName, defeatedColor, icon);
    }
    
    /// <summary>
    /// 알림을 표시합니다.
    /// </summary>
    private void ShowNotification(string title, string bossName, Color textColor, Sprite icon)
    {
        // 이미 알림이 표시 중이라면 중단
        StopAllCoroutines();
        
        // UI 설정
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = textColor;
        }
        
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
            bossNameText.color = textColor;
        }
        
        if (bossIcon != null)
        {
            if (icon != null)
            {
                bossIcon.sprite = icon;
                bossIcon.gameObject.SetActive(true);
            }
            else
            {
                bossIcon.gameObject.SetActive(false);
            }
        }
        
        // 알림 표시 코루틴 시작
        StartCoroutine(DisplayNotificationCoroutine());
    }
    
    /// <summary>
    /// 알림 표시 코루틴
    /// </summary>
    private IEnumerator DisplayNotificationCoroutine()
    {
        // 패널 활성화
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }
        
        // 애니메이터가 있다면 등장 애니메이션 재생
        if (notificationAnimator != null)
        {
            notificationAnimator.SetTrigger("Show");
        }
        else
        {
            // 애니메이터가 없다면 간단한 스케일 애니메이션
            StartCoroutine(SimpleScaleAnimation(true));
        }
        
        // 표시 시간만큼 대기
        yield return new WaitForSeconds(displayDuration);
        
        // 애니메이터가 있다면 퇴장 애니메이션 재생
        if (notificationAnimator != null)
        {
            notificationAnimator.SetTrigger("Hide");
            // 애니메이션이 끝날 때까지 대기 (대략 0.5초)
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // 애니메이터가 없다면 간단한 스케일 애니메이션
            yield return StartCoroutine(SimpleScaleAnimation(false));
        }
        
        // 패널 비활성화
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 간단한 스케일 애니메이션 (애니메이터가 없을 때 사용)
    /// </summary>
    private IEnumerator SimpleScaleAnimation(bool scaleUp)
    {
        if (notificationPanel == null) yield break;
        
        Vector3 startScale = scaleUp ? Vector3.zero : Vector3.one;
        Vector3 endScale = scaleUp ? Vector3.one : Vector3.zero;
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease out animation
            t = 1f - (1f - t) * (1f - t);
            
            notificationPanel.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        
        notificationPanel.transform.localScale = endScale;
    }
    
    /// <summary>
    /// 현재 표시 중인 알림을 즉시 숨깁니다.
    /// </summary>
    public void HideNotification()
    {
        StopAllCoroutines();
        
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            notificationPanel.transform.localScale = Vector3.one;
        }
    }
}