using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 도전과제(업적)를 관리하는 클래스입니다.
/// PlayerPrefs를 사용하여 달성 상태를 저장하고, 조건 충족 시 캐릭터를 해금합니다.
/// </summary>
public class AchiveManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("잠금 상태의 캐릭터 UI 오브젝트 배열")]
    public GameObject[] lockCharacter;
    [Tooltip("해금 상태의 캐릭터 UI 오브젝트 배열")]
    public GameObject[] unlockCharacter;
    [Tooltip("업적 달성 알림 UI")]
    public GameObject uiNotice;

    // 도전과제 종류 정의
    private enum Achive { UnlockPotato, UnlockBean }
    private Achive[] achives; // 모든 도전과제 목록
    private WaitForSecondsRealtime wait; // 알림 UI 표시 시간

    void Awake()
    {
        // WaitForSecondsRealtime은 Time.timeScale에 영향을 받지 않아, 게임이 멈춰도 동작합니다.
        wait = new WaitForSecondsRealtime(5);
        // Enum의 모든 값을 배열로 가져와 도전과제 목록을 초기화합니다.
        achives = (Achive[])Enum.GetValues(typeof(Achive));

        // PlayerPrefs에 데이터가 없으면 처음 실행으로 간주하고 초기화합니다.
        if (!PlayerPrefs.HasKey("MyData"))
        {
            Init();
        }
    }

    /// <summary>
    /// PlayerPrefs에 도전과제 데이터를 처음으로 생성하고 초기화합니다.
    /// </summary>
    void Init()
    {
        PlayerPrefs.SetInt("MyData", 1); // 데이터가 생성되었음을 표시

        // 모든 도전과제를 '미달성' 상태(0)로 초기화합니다.
        foreach (Achive achive in achives)
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);
        }
    }

    void Start()
    {
        // 게임 시작 시 도전과제 달성 상태에 따라 캐릭터 UI를 갱신합니다.
        UnlockCharacter();
    }

    /// <summary>
    /// 저장된 도전과제 달성 상태에 따라 캐릭터 선택 UI의 잠금/해금 상태를 설정합니다.
    /// </summary>
    void UnlockCharacter()
    {
        for (int i = 0; i < lockCharacter.Length; i++)
        {
            string achiveName = achives[i].ToString();
            // PlayerPrefs에서 해당 도전과제의 달성 여부를 가져옵니다 (1: 달성, 0: 미달성).
            bool isUnlock = PlayerPrefs.GetInt(achiveName) == 1;
            lockCharacter[i].SetActive(!isUnlock);
            unlockCharacter[i].SetActive(isUnlock);
        }
    }

    // 매 프레임 후반에 도전과제 달성 여부를 확인합니다.
    void LateUpdate()
    {
        // 모든 도전과제에 대해 달성 조건을 확인합니다.
        foreach (Achive achive in achives)
        {
            CheckAchive(achive);
        }
    }

    /// <summary>
    /// 특정 도전과제의 달성 조건을 확인하고, 달성 시 처리 로직을 실행합니다.
    /// </summary>
    /// <param name="achive">확인할 도전과제</param>
    void CheckAchive(Achive achive)
    {
        bool isAchive = false;

        // 도전과제 종류에 따라 달성 조건을 확인합니다.
        switch (achive)
        {
            case Achive.UnlockPotato:
                // 조건: 10킬 이상 달성
                isAchive = GameManager.instance.kill >= 10;
                break;
            case Achive.UnlockBean:
                // 조건: 최대 게임 시간까지 생존
                isAchive = GameManager.instance.gameTime == GameManager.instance.maxGameTime;
                break;
        }

        // 조건을 달성했고, 아직 달성 기록이 없는 경우
        if (isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0)
        {
            // 달성 상태로 저장
            PlayerPrefs.SetInt(achive.ToString(), 1);

            // 알림 UI에서 해당 도전과제에 맞는 텍스트만 활성화합니다.
            for (int i = 0; i < uiNotice.transform.childCount; i++)
            {
                bool isActive = i == (int)achive;
                uiNotice.transform.GetChild(i).gameObject.SetActive(isActive);
            }

            // 알림 코루틴을 시작합니다.
            StartCoroutine(NoticeRoutine());
        }
    }

    /// <summary>
    /// 도전과제 달성 알림 UI를 일정 시간 동안 보여줬다가 사라지게 하는 코루틴입니다.
    /// </summary>
    IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp); // 달성 효과음 재생

        yield return wait; // 5초 대기

        uiNotice.SetActive(false);
    }
}