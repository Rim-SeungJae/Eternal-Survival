using UnityEngine;
using UnityEngine.UI; // Slider를 사용하기 위해 필요

public class Campfire : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("최대 회복까지 걸리는 시간 (초)")]
    public float maxTimer = 5f;

    [Tooltip("최대 회복 가능 횟수")]
    public int maxHealCount = 3;

    [Tooltip("플레이어 감지 거리")]
    public float detectionRadius = 5f;

    [Tooltip("스프라이트가 투명해지기 시작하는 거리")]
    public float fadeStartDistance = 4f;

    [Tooltip("스프라이트가 완전히 투명해지는 거리")]
    public float fadeEndDistance = 6f;

    [Tooltip("플레이어가 가장 가까이 있을 때 스프라이트의 최소 투명도 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.5f; // 변수명 정정

    [Header("연결된 오브젝트")]
    [Tooltip("솥단지 스프라이트 렌더러")]
    public SpriteRenderer potJar;

    [Tooltip("회복 진행도를 표시할 HUD 컴포넌트 (InfoType.Progress)")]
    public HUD progressHUD;

    [Tooltip("진행바에 나타날 설명")]
    public string description;

    // 내부 변수
    private int currentHealCount;
    private float currentTimer;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform; // 플레이어의 Transform을 캐싱하여 매번 찾는 비용을 줄임

    void Start()
    {
        // 초기화
        currentHealCount = maxHealCount;
        currentTimer = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 플레이어 Transform 캐싱
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            playerTransform = GameManager.instance.player.transform;
        }

        // 초기 상태 설정
        UpdateVisuals();
    }

    void Update()
    {
        // 플레이어가 없거나, 회복 횟수를 다 썼으면 아무것도 하지 않음
        if (playerTransform == null || currentHealCount <= 0)
        {
            return;
        }

        // 플레이어와의 거리에 따라 회복존 스프라이트의 투명도를 조절
        HandleSpriteVisibility();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 게임 진행 중이 아니거나, 플레이어가 아니거나, 회복 횟수를 다 썼으면 무시
        if (!GameManager.instance.isLive || !other.CompareTag("Player") || currentHealCount <= 0)
        {
            return;
        }

        // 진행바 활성화
        if (progressHUD != null && !progressHUD.gameObject.activeSelf)
        {
            progressHUD.gameObject.SetActive(true);
        }

        // 타이머를 증가시키고, 최대 시간에 도달하면 체력을 회복
        currentTimer += Time.deltaTime;
        if (currentTimer >= maxTimer)
        {
            HealPlayer();
            currentTimer = 0; // 타이머 초기화
        }
        
        // 진행바 업데이트
        if (progressHUD != null)
        {
            progressHUD.setProgress(maxTimer, currentTimer,description);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어가 범위를 벗어나면 타이머 초기화 및 진행바 비활성화
        if (other.gameObject.CompareTag("Player"))
        {
            currentTimer = 0;
            if (progressHUD != null)
            {
                progressHUD.setProgress(maxTimer, 0,description); // 진행바 0으로 리셋
                progressHUD.gameObject.SetActive(false); // 진행바 비활성화
            }
        }
    }

    // 플레이어의 체력을 회복시키는 메서드
    private void HealPlayer()
    {
        GameManager.instance.health = GameManager.instance.maxHealth;
        currentHealCount--;

        // AudioManager.instance.PlaySfx(AudioManager.Sfx.Heal); // 회복 효과음 업데이트 예정

        // 회복 후 상태 업데이트
        UpdateVisuals();

        // 회복 횟수를 모두 소진했을 경우 진행바 비활성화
        if (currentHealCount <= 0 && progressHUD != null)
        {
            progressHUD.setProgress(maxTimer, 0,description); // 진행바 0으로 리셋
            progressHUD.gameObject.SetActive(false); // 진행바 비활성화
        }
    }

    // 캠프파이어의 시각적 상태를 업데이트하는 메서드
    private void UpdateVisuals()
    {
        bool isActive = currentHealCount > 0;
        gameObject.SetActive(isActive);
        if (potJar != null)
        {
            potJar.enabled = isActive;
        }
    }

    // 플레이어와의 거리에 따라 회복존 스프라이트의 표시 여부를 처리하는 메서드
    private void HandleSpriteVisibility()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 거리에 따라 알파 값을 계산합니다.
        // fadeEndDistance에서 fadeStartDistance로 갈수록 알파 값이 0에서 1로 변합니다.
        float alpha = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, distance);
        alpha = Mathf.Clamp01(alpha); // 알파 값을 0과 1 사이로 제한합니다.

        // 계산된 알파 값을 0 (완전 투명)과 minAlpha (가까이 있을 때의 최대 불투명도) 사이로 보간합니다.
        // 이렇게 하면 가장 가까이 있을 때도 minAlpha 값 이상으로 불투명해지지 않습니다.
        alpha = Mathf.Lerp(0f, minAlpha, alpha);

        Color currentColor = spriteRenderer.color;
        currentColor.a = alpha;
        spriteRenderer.color = currentColor;

        // 스프라이트 렌더러 자체는 항상 활성화 상태로 유지합니다.
        if (!spriteRenderer.enabled)
        {
            spriteRenderer.enabled = true;
        }
    }
}