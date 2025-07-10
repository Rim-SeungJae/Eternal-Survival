using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 손(무기 장착 위치)의 위치와 방향, 렌더링 순서를 관리하는 클래스입니다.
/// 플레이어의 이동 방향에 따라 손의 위치와 스프라이트가 자연스럽게 보이도록 조정합니다.
/// </summary>
public class Hand : MonoBehaviour
{
    [Tooltip("오른손인지 여부")]
    public bool isRight;
    [Tooltip("손의 스프라이트 렌더러")]
    public SpriteRenderer spriter;

    private SpriteRenderer player;

    // 플레이어가 바라보는 방향에 따른 손의 상대 위치 및 회전값
    private Vector3 leftPos = new Vector3(0.35f, -0.15f, 0);
    private Vector3 leftPosReverse = new Vector3(-0.15f, -0.15f, 0);
    private Quaternion rightRot = Quaternion.Euler(0, 0, -35);
    private Quaternion rightRotReverse = Quaternion.Euler(0, 0, -135);

    void Awake()
    {
        // 부모 오브젝트에서 플레이어의 SpriteRenderer를 찾아옵니다.
        // GetComponentsInParent는 자기 자신부터 부모로 올라가며 찾습니다.
        // 순서에 의존하는 방식이므로 구조가 바뀌면 문제가 될 수 있습니다.
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    // 모든 Update가 끝난 후 호출되어, 플레이어의 최종 상태에 따라 위치를 보정합니다.
    void LateUpdate()
    {
        // 플레이어 스프라이트의 flipX 속성을 통해 바라보는 방향을 확인합니다.
        bool isReverse = player.flipX;

        if (isRight)
        {
            // 오른손: 플레이어 방향에 따라 회전값과 렌더링 순서(sortingOrder)를 조정합니다.
            transform.localRotation = isReverse ? rightRotReverse : rightRot;
            spriter.flipY = isReverse;
            spriter.sortingOrder = isReverse ? 4 : 6; // 캐릭터보다 앞에/뒤에 보이도록 설정
        }
        else
        {
            // 왼손: 플레이어 방향에 따라 위치와 렌더링 순서를 조정합니다.
            transform.localPosition = isReverse ? leftPosReverse : leftPos;
            spriter.flipX = isReverse;
            spriter.sortingOrder = isReverse ? 6 : 4;
        }
    }
}