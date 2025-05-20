using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightController : MonoBehaviour
{
    public Light2D playerLight;
    public GameManager gameManager;

    public float minRadius;
    public float maxRadius;
    public AnimationCurve visionCurve;

    void Update()
    {
        if (gameManager.IsNight())
        {
            playerLight.lightType = Light2D.LightType.Point;
            float t = gameManager.GetNightFactor(); // 0~1
            float factor = visionCurve.Evaluate(t);
            playerLight.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, factor);
        }
        else
        {
            playerLight.lightType = Light2D.LightType.Global;
            playerLight.pointLightOuterRadius = maxRadius;
        }
    }
}
