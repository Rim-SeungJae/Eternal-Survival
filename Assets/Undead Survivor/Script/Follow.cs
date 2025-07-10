using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 요소가 특정 게임 오브젝트(플레이어)를 따라다니게 하는 클래스입니다.
/// 주로 플레이어 위에 표시되는 UI(체력 바 등)에 사용될 수 있습니다.
/// </summary>
public class Follow : MonoBehaviour
{
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // 물리 업데이트 주기에 맞춰 실행하여 떨림 현상을 줄일 수 있습니다.
    void FixedUpdate()
    {
        // 월드 좌표계의 플레이어 위치를 스크린 좌표계로 변환하여 UI 위치를 설정합니다.
        rect.position = Camera.main.WorldToScreenPoint(GameManager.instance.player.transform.position);
    }
}