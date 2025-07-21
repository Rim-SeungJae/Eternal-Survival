using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 종료 시 결과(승리/패배) UI를 관리하는 클래스입니다.
/// </summary>
public class Result : MonoBehaviour
{
    [Tooltip("결과 타이틀 UI 오브젝트 배열 (0: 패배, 1: 승리)")]
    public GameObject[] titles;

    /// <summary>
    /// 패배 시 호출되어 패배 UI를 활성화합니다.
    /// </summary>
    public void Lose()
    {
        titles[0].SetActive(true);
    }

    /// <summary>
    /// 승리 시 호출되어 승리 UI를 활성화합니다.
    /// </summary>
    public void Win()
    {
        titles[1].SetActive(true);
    }
}