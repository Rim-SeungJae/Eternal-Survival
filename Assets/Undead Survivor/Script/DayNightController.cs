using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightController : MonoBehaviour
{
    public Light2D playerLight,globalLight;
    public GameManager gameManager;

    public float minDiameter;
    public float maxDiameter;
    public float outsideIntensity;
    public float insideIntensity;
    public AnimationCurve visionCurve;

    public float CurrentScale { get; private set; } // 추가

    void Update()
    {
        if (gameManager.IsNight())
        {
            if (!playerLight.enabled)
            {
                playerLight.enabled = true;
                globalLight.intensity = outsideIntensity;
                playerLight.intensity = insideIntensity;
            }
            float t = gameManager.GetNightFactor(); // 0~1
            float factor = visionCurve.Evaluate(t);

            float scale = Mathf.Lerp(minDiameter, maxDiameter, factor);
            playerLight.transform.localScale = new Vector3(scale, scale, 1);
            CurrentScale = scale;
            //playerLight.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, factor);
        }
        else if (!gameManager.IsNight() && playerLight.enabled)
        {
            playerLight.enabled = false;
            globalLight.intensity = 1f;
            CurrentScale = 0f;
        }
    }

    public Sprite playerLightSprite; // 에디터에서 연결

    public float CurrentLightRadius
    {
        get
        {
            if (playerLight == null || playerLight.lightCookieSprite == null)
                return 0f;
            // Light2D에 연결된 스프라이트의 x축 크기 * 현재 스케일
            return playerLight.lightCookieSprite.bounds.size.x * playerLight.transform.localScale.x;
        }
    }
}
