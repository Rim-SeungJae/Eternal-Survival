using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 게임의 낮/밤 사이클에 따른 조명 효과를 제어하는 클래스입니다.
/// 밤이 되면 전역 조명을 어둡게 하고 플레이어 주변에만 시야를 제공합니다.
/// </summary>
public class DayNightController : MonoBehaviour
{
    [Header("Light References")]
    [Tooltip("플레이어 주변을 밝히는 2D 조명")]
    public Light2D playerLight;
    [Tooltip("전역 2D 조명")]
    public Light2D globalLight;

    [Header("Game Logic Reference")]
    [Tooltip("게임 매니저 참조")]
    public GameManager gameManager;

    [Header("Night Vision Settings")]
    [Tooltip("밤 시야의 최소/최대 직경")]
    public float minDiameter;
    public float maxDiameter;
    [Tooltip("밤일 때의 전역 조명 강도")]
    public float outsideIntensity;
    [Tooltip("밤일 때의 플레이어 조명 강도")]
    public float insideIntensity;
    [Tooltip("시간에 따른 시야 변화를 제어하는 애니메이션 커브")]
    public AnimationCurve visionCurve;

    /// <summary>
    /// 현재 플레이어 조명의 스케일 값입니다.
    /// </summary>
    public float CurrentScale { get; private set; }

    /// <summary>
    /// 플레이어 조명의 스프라이트 (반경 계산용)
    /// </summary>
    public Sprite playerLightSprite;

    void Update()
    {
        // 게임 매니저를 통해 현재 밤인지 확인합니다.
        if (gameManager.IsNight())
        {
            // 밤으로 전환되는 첫 프레임에 조명을 설정합니다.
            if (!playerLight.enabled)
            {
                playerLight.enabled = true;
                globalLight.intensity = outsideIntensity;
                playerLight.intensity = insideIntensity;
            }

            // 밤이 얼마나 진행되었는지 0~1 사이의 값으로 가져옵니다.
            float t = gameManager.GetNightFactor();
            // 애니메이션 커브를 사용하여 시야 변화에 비선형적인 효과를 줍니다.
            float factor = visionCurve.Evaluate(t);

            // 커브 값을 기반으로 현재 시야(조명 스케일)를 계산합니다.
            float scale = Mathf.Lerp(minDiameter, maxDiameter, factor);
            playerLight.transform.localScale = new Vector3(scale, scale, 1);
            CurrentScale = scale;
        }
        // 낮으로 전환되는 첫 프레임에 조명을 원래대로 되돌립니다.
        else if (!gameManager.IsNight() && playerLight.enabled)
        {
            playerLight.enabled = false;
            globalLight.intensity = 1f;
            CurrentScale = 0f;
        }
    }

    /// <summary>
    /// 현재 플레이어 조명의 유효 반경을 계산하여 반환합니다.
    /// </summary>
    public float CurrentLightRadius
    {
        get
        {
            if (playerLight == null || playerLight.lightCookieSprite == null)
            {
                return 0f;
            }
            // 조명 쿠키 스프라이트의 크기와 현재 조명 스케일을 곱하여 실제 반경을 계산합니다.
            // 스프라이트의 bounds.size.x는 스프라이트의 원본 너비를 나타냅니다.
            return playerLight.lightCookieSprite.bounds.size.x * playerLight.transform.localScale.x;
        }
    }
}