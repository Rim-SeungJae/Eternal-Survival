using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 전반적인 흐름과 상태를 관리하는 싱글톤 클래스입니다.
/// 플레이어 데이터, 게임 시간, 레벨, 경험치 등을 관리하며,
/// 게임의 시작, 종료, 재시작 로직을 담당합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager instance;

    [Header("# Game Object")]
    public Player player; // 플레이어 오브젝트 참조
    public PoolManager pool; // 오브젝트 풀링 매니저 참조
    public LevelUp uiLevelUp; // 레벨업 UI 참조
    public Result uiResult; // 결과 UI 참조
    public GameObject enemyCleaner; // 게임 승리 시 적을 정리할 오브젝트
    public DayNightController dayNightController; // 낮/밤 컨트롤러 참조 추가

    [Header("# Game Control")]
    public bool isLive; // 게임 진행 상태 (true: 진행 중, false: 정지)
    public bool isTimeStopped = false; // 시간 정지 상태
    public float gameTime; // 현재 게임 시간
    public float maxGameTime = 2 * 10f; // 최대 게임 시간 (생존 목표 시간)

    [Header("# Player Info")]
    public int playerId; // 현재 플레이어 ID
    public int level; // 현재 레벨
    public float health; // 현재 체력
    public float maxHealth = 100f; // 최대 체력
    public int kill; // 처치한 적 수
    public int exp; // 현재 경험치
    public int[] nextExp = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600 }; // 레벨별 요구 경험치

    [Header("# Day/Night Cycle")]
    public float cycleDuration = 60f; // 낮/밤 사이클 전체 시간
    public float nightStart = 30f; // 밤이 시작되는 시간
    public float nightEnd = 60f; // 밤이 끝나는 시간
    public float nightTimer = 0f; // 현재 사이클 타이머

    private void Awake()
    {
        instance = this; // 싱글톤 인스턴스 할당
        Application.targetFrameRate = 60; // 프레임 속도 60으로 고정

        // 캐릭터 선택 씬에서 선택한 캐릭터 ID를 가져옵니다.
        playerId = GlobalData.selectedCharacterDataSO.characterId;

        // DayNightController 참조를 GameManager에서 캐싱합니다.
        dayNightController = FindFirstObjectByType<DayNightController>();
    }

    void Start()
    {
        // 선택된 캐릭터로 게임을 시작합니다.
        GameStart(playerId);
    }

    /// <summary>
    /// 게임을 시작하는 함수입니다.
    /// </summary>
    /// <param name="id">선택된 캐릭터의 ID</param>
    public void GameStart(int id)
    {
        playerId = id;
        health = maxHealth;

        player.gameObject.SetActive(true);
        // 캐릭터 ID에 따라 초기 무기 선택
        uiLevelUp.Select(playerId % 2);
        Resume(); // 게임 재개

        // BGM 및 효과음 재생
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    /// <summary>
    /// 게임 오버 처리를 시작하는 함수입니다.
    /// </summary>
    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    /// <summary>
    /// 게임 오버 연출 및 UI 표시를 위한 코루틴입니다.
    /// </summary>
    IEnumerator GameOverRoutine()
    {
        isLive = false;
        yield return new WaitForSeconds(0.5f); // 잠시 대기 후 UI 표시

        uiResult.gameObject.SetActive(true);
        uiResult.Lose(); // 패배 UI 활성화
        Stop(); // 게임 정지

        // BGM 및 효과음 변경
        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    /// <summary>
    /// 게임 승리 처리를 시작하는 함수입니다.
    /// </summary>
    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    /// <summary>
    /// 게임 승리 연출 및 UI 표시를 위한 코루틴입니다.
    /// </summary>
    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true); // 화면의 모든 적 제거
        yield return new WaitForSeconds(0.5f); // 잠시 대기 후 UI 표시

        uiResult.gameObject.SetActive(true);
        uiResult.Win(); // 승리 UI 활성화
        Stop(); // 게임 정지

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    /// <summary>
    /// 게임을 재시작하는 함수입니다. (타이틀 씬으로 돌아감)
    /// </summary>
    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (!isLive) return; // 게임이 진행 중이 아니면 업데이트 중단

        // 게임 시간 및 낮/밤 사이클 업데이트
        gameTime += Time.deltaTime;
        nightTimer += Time.deltaTime;
        if (nightTimer > cycleDuration)
        {
            nightTimer = 0f; // 사이클 초기화
        }

        // 최대 게임 시간에 도달하면 승리 처리
        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    /// <summary>
    /// 경험치를 획득하는 함수입니다.
    /// </summary>
    public void GetExp(int amount = 1)
    {
        if (!isLive) return;

        exp += amount;
        // 현재 레벨에 맞는 요구 경험치에 도달했는지 확인
        if (exp >= nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;
            uiLevelUp.Show(); // 레벨업 UI 표시
        }
    }

    /// <summary>
    /// 게임을 일시 정지합니다.
    /// </summary>
    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0; // 게임 시간 흐름 정지
    }

    /// <summary>
    /// 게임을 재개합니다.
    /// </summary>
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1; // 게임 시간 흐름 정상화
    }

    /// <summary>
    /// 현재 시간이 밤인지 확인합니다.
    /// </summary>
    /// <returns>밤이면 true, 낮이면 false</returns>
    public bool IsNight()
    {
        return nightTimer > nightStart && nightTimer < nightEnd;
    }
    
    /// <summary>
    /// 밤이 얼마나 진행되었는지 0과 1 사이의 값으로 반환합니다.
    /// </summary>
    /// <returns>밤 진행도 (0.0 ~ 1.0)</returns>
    public float GetNightFactor()
    {
        if (!IsNight()) return 0f;

        return (nightTimer - nightStart) / (nightEnd - nightStart);
    }

    /// <summary>
    /// 지정된 시간 동안 시간 정지 효과를 발동시키는 코루틴을 시작합니다.
    /// </summary>
    /// <param name="duration">시간 정지 지속 시간</param>
    public void StartTimeStop(float duration)
    {
        StartCoroutine(TimeStopCoroutine(duration));
    }

    IEnumerator TimeStopCoroutine(float duration)
    {
        isTimeStopped = true;
        player.particle.Play(); // 시간 정지 효과 파티클 재생
        // 모든 활성화된 Enemy를 찾아 시간 정지 효과를 적용합니다.
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                enemy.SetTimeStopEffect(true);
            }
        }

        yield return new WaitForSeconds(duration);

        isTimeStopped = false;
        player.particle.Stop(); // 시간 정지 효과 파티클 정지

        // 모든 활성화된 Enemy를 다시 찾아 시간 정지 효과를 해제합니다.
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.SetTimeStopEffect(false);
            }
        }
    }
}