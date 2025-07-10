using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 내 Heads-Up Display (HUD)의 각 UI 요소를 관리하는 클래스입니다.
/// Inspector에서 지정한 InfoType에 따라 각각 다른 정보를 표시합니다.
/// </summary>
public class HUD : MonoBehaviour
{
    // 표시할 정보의 종류를 정의하는 열거형
    public enum InfoType { Exp, Level, Kill, Time, Health, Progress }

    [Tooltip("이 HUD 요소가 표시할 정보의 종류")]
    public InfoType type;

    private Text myText;
    private Slider mySlider;
    private float totalTime; // 전체 시간 (Progress 타입에서 사용)
    private float remainTime; // 남은 시간 (Progress 타입에서 사용)
    private Slider progressSlider; // 진행바 (Progress 타입에서 사용)
    private TextMeshProUGUI TMP; // 진행바 설명 (Progress 타입에서 사용)

    void Awake()
    {
        // 이 스크립트가 붙은 게임 오브젝트에서 Text와 Slider 컴포넌트를 찾아 캐싱합니다.
        // 해당 컴포넌트가 없을 경우 null이 되므로, 실제 사용 시에는 null 체크를 해주는 것이 안전합니다.
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
        progressSlider = GetComponentInChildren<Slider>();
        TMP = GetComponentInChildren<TextMeshProUGUI>();
        totalTime = 0;
    }

    // Update 대신 LateUpdate를 사용하여 모든 게임 로직 계산이 끝난 후 UI를 갱신합니다.
    void LateUpdate()
    {
        // InfoType에 따라 다른 UI 갱신 로직을 수행합니다.
        switch (type)
        {
            case InfoType.Exp:
                // 경험치 바(Slider)를 갱신합니다.
                float curExp = GameManager.instance.exp;
                // 다음 레벨업에 필요한 경험치를 가져옵니다. 레벨이 최대일 경우를 대비해 Min 함수를 사용합니다.
                float maxExp = GameManager.instance.nextExp[Mathf.Min(GameManager.instance.level, GameManager.instance.nextExp.Length - 1)];
                mySlider.value = curExp / maxExp;
                break;
            case InfoType.Health:
                // 체력 바(Slider)를 갱신합니다.
                float curHealth = GameManager.instance.health;
                float maxHealth = GameManager.instance.maxHealth;
                mySlider.value = curHealth / maxHealth;
                break;
            case InfoType.Kill:
                // 처치한 적의 수를 텍스트로 표시합니다.
                myText.text = string.Format("{0:F0}", GameManager.instance.kill);
                break;
            case InfoType.Level:
                // 현재 레벨을 텍스트로 표시합니다.
                myText.text = string.Format("Lv.{0:F0}", GameManager.instance.level);
                break;
            case InfoType.Time:
                // 남은 시간을 분:초 형태로 표시합니다.
                float remainGameTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
                int min = Mathf.FloorToInt(remainGameTime / 60);
                int sec = Mathf.FloorToInt(remainGameTime % 60);
                // D2 포맷을 사용하여 항상 두 자리로 표시합니다. (예: 01:05)
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                break;
            case InfoType.Progress:
                // 진행바의 값은 남은 시간을 전체 시간으로 나눈 비율로 설정합니다.
                // totalTime이 0보다 클 경우에만 슬라이더를 갱신합니다.
                if (totalTime > 0)
                {
                    progressSlider.value = remainTime / totalTime;
                }
                break;
        }
    }

    public void setProgress(float totalTime, float remainTime, string desc)
    {
        this.totalTime = totalTime;
        this.remainTime = remainTime;
        if (TMP != null) // null 체크 추가
        {
            TMP.text = desc;
        }
    }
}