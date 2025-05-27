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
            playerLight.lightType = Light2D.LightType.Sprite;
            float t = gameManager.GetNightFactor(); // 0~1
            float factor = visionCurve.Evaluate(t);

            float scale = Mathf.Lerp(minRadius, maxRadius, factor);
            playerLight.transform.localScale = new Vector3(scale, scale, 1);
            //playerLight.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, factor);
        }
        else if (!gameManager.IsNight() && playerLight.lightType != Light2D.LightType.Global)
        {
            playerLight.lightType = Light2D.LightType.Global;
        }
    }
}
