using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// AirSupply 보상 상자 획득 시 재생되는 UI 애니메이션을 관리하는 클래스입니다.
/// </summary>
public class AirSupplyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject rewardPanel; // 보상 UI 캔버스
    [SerializeField] private Image boxImage; // 상자 이미지 (BoxImage)
    [SerializeField] private Image itemIcon; // 획득한 아이템 아이콘 (ItemIcon)
    [SerializeField] private Image lightOverlay; // 빛 효과 오버레이
    
    [Header("Animation Settings")]
    [SerializeField] private float shakeIntensity = 30f; // 흔들림 강도
    [SerializeField] private float shakeDuration = 3f; // 흔들림 지속 시간
    [SerializeField] private float lightFadeDuration = 1f; // 빛 효과 지속 시간
    [SerializeField] private float itemShowDelay = 0.5f; // 아이템 표시 지연 시간
    
    [Header("Box Animation")]
    [SerializeField] private Sprite closedBoxSprite; // 닫힌 상자 스프라이트
    [SerializeField] private Sprite openBoxSprite; // 열린 상자 스프라이트
    
    private static AirSupplyUI instance;
    public static AirSupplyUI Instance => instance;
    
    private bool isPlaying = false;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 초기 설정
        if (rewardPanel == null) rewardPanel = GetComponent<GameObject>();
        
        // 초기에는 UI 비활성화
        if (rewardPanel != null)
        {
            rewardPanel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// AirSupply 보상 획득 애니메이션을 재생합니다.
    /// </summary>
    /// <param name="rewardSprite">획득한 아이템의 스프라이트</param>
    /// <param name="onComplete">애니메이션 완료 후 실행할 콜백</param>
    public void PlayRewardAnimation(Sprite rewardSprite, System.Action onComplete = null)
    {
        if (isPlaying) return;
        
        StartCoroutine(RewardAnimationSequence(rewardSprite, onComplete));
    }
    
    /// <summary>
    /// 보상 애니메이션 시퀀스를 실행합니다.
    /// </summary>
    private IEnumerator RewardAnimationSequence(Sprite rewardSprite, System.Action onComplete)
    {
        isPlaying = true;
        
        // 게임 일시정지
        GameManager.instance.Stop();
        
        // UI 활성화 및 초기 설정
        SetupUI(rewardSprite);
        
        // 1단계: 상자 흔들기 (3초)
        yield return StartCoroutine(ShakeBoxAnimation());
        
        // 2단계: 상자 열기
        OpenBox();
        
        // 3단계: 빛 효과 (화면을 가림)
        yield return StartCoroutine(LightEffectAnimation());
        
        // 4단계: 아이템 아이콘 표시
        yield return StartCoroutine(ShowItemIcon());
        
        // 5단계: 잠시 대기 후 종료
        yield return new WaitForSecondsRealtime(2f);
        
        // UI 정리 및 게임 재개
        CleanupUI();
        GameManager.instance.Resume();
        
        // 완료 콜백 실행
        onComplete?.Invoke();
        
        isPlaying = false;
    }
    
    /// <summary>
    /// UI 초기 설정을 수행합니다.
    /// </summary>
    private void SetupUI(Sprite rewardSprite)
    {
        // 캔버스 활성화
        rewardPanel.gameObject.SetActive(true);
        
        // 상자 이미지 설정 (닫힌 상태)
        if (boxImage != null && closedBoxSprite != null)
        {
            boxImage.sprite = closedBoxSprite;
            boxImage.transform.localScale = Vector3.one;
            boxImage.transform.rotation = Quaternion.identity;
        }
        
        // 아이템 아이콘 설정 (투명하게 시작)
        if (itemIcon != null)
        {
            itemIcon.sprite = rewardSprite;
            itemIcon.color = new Color(1f, 1f, 1f, 0f);
            itemIcon.transform.localScale = Vector3.zero;
        }
        
        // 빛 오버레이 설정 (투명하게 시작)
        if (lightOverlay != null)
        {
            lightOverlay.color = new Color(1f, 1f, 1f, 0f);
        }
    }
    
    /// <summary>
    /// 상자 흔들기 애니메이션을 실행합니다.
    /// </summary>
    private IEnumerator ShakeBoxAnimation()
    {
        if (boxImage == null) yield break;
        
        Vector3 originalPosition = boxImage.transform.localPosition;
        
        // DOTween을 사용한 흔들기 애니메이션
        boxImage.transform.DOShakePosition(shakeDuration, shakeIntensity, 10, 90f, false, true)
            .SetUpdate(true); // Time.timeScale에 영향받지 않도록 설정
        
        yield return new WaitForSecondsRealtime(shakeDuration);
        
        // 원래 위치로 복귀
        boxImage.transform.localPosition = originalPosition;
    }
    
    /// <summary>
    /// 상자를 열린 상태로 변경합니다.
    /// </summary>
    private void OpenBox()
    {
        if (boxImage != null && openBoxSprite != null)
        {
            boxImage.sprite = openBoxSprite;
        }
    }
    
    /// <summary>
    /// 빛 효과 애니메이션을 실행합니다.
    /// </summary>
    private IEnumerator LightEffectAnimation()
    {
        if (lightOverlay == null) yield break;
        
        // 빛이 화면을 가리도록 페이드인
        lightOverlay.DOFade(1f, lightFadeDuration * 0.3f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(lightFadeDuration * 0.3f);
        
        // 잠시 대기 (완전히 가려진 상태)
        yield return new WaitForSecondsRealtime(lightFadeDuration * 0.4f);
        
        // 빛이 사라지도록 페이드아웃
        lightOverlay.DOFade(0f, lightFadeDuration * 0.3f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(lightFadeDuration * 0.3f);
    }
    
    /// <summary>
    /// 아이템 아이콘을 표시합니다.
    /// </summary>
    private IEnumerator ShowItemIcon()
    {
        if (itemIcon == null) yield break;
        
        yield return new WaitForSecondsRealtime(itemShowDelay);
        
        // 아이템 아이콘 등장 애니메이션
        itemIcon.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        itemIcon.DOFade(1f, 0.3f).SetUpdate(true);
        
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    /// <summary>
    /// UI를 정리하고 비활성화합니다.
    /// </summary>
    private void CleanupUI()
    {
        // 모든 DOTween 애니메이션 정리
        if (boxImage != null) boxImage.transform.DOKill();
        if (itemIcon != null) 
        {
            itemIcon.transform.DOKill();
            itemIcon.DOKill();
        }
        if (lightOverlay != null) lightOverlay.DOKill();
        
        // 캔버스 비활성화
        rewardPanel.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 강제로 애니메이션을 중단합니다.
    /// </summary>
    public void ForceStop()
    {
        if (!isPlaying) return;
        
        StopAllCoroutines();
        CleanupUI();
        GameManager.instance.Resume();
        isPlaying = false;
    }
    
    void OnDestroy()
    {
        // DOTween 정리
        DOTween.KillAll();
    }
}