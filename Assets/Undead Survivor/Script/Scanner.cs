using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주변의 특정 레이어(적)를 탐지하고 가장 가까운 대상을 찾는 클래스입니다.
/// 원거리 무기의 타겟팅에 사용됩니다.
/// </summary>
public class Scanner : MonoBehaviour
{
    [Tooltip("탐지 반경")]
    public float scanRange;
    [Tooltip("탐지할 대상의 레이어")]
    public LayerMask targetLayer;
    [Tooltip("탐지된 모든 대상")]
    public RaycastHit2D[] targets;
    [Tooltip("가장 가까운 대상")]
    public Transform nearestTarget;

    // 물리 관련 로직이므로 FixedUpdate에서 실행합니다.
    void FixedUpdate()
    {
        // 지정된 반경 내의 모든 대상 레이어 콜라이더를 탐지합니다.
        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        // 탐지된 대상 중에서 가장 가까운 대상을 찾습니다.
        nearestTarget = GetNearest();
    }

    /// <summary>
    /// 탐지된 대상(targets) 중에서 가장 가까운 대상의 Transform을 반환합니다.
    /// </summary>
    /// <returns>가장 가까운 대상의 Transform. 대상이 없으면 null을 반환합니다.</returns>
    Transform GetNearest()
    {
        Transform result = null;
        float minDiff = float.MaxValue; // 최소 거리를 저장할 변수, 초기값은 최대값으로 설정

        foreach (RaycastHit2D target in targets)
        {
            Vector3 myPos = transform.position;
            Vector3 targetPos = target.transform.position;
            // 현재 대상과의 거리 계산
            float curDiff = Vector3.Distance(myPos, targetPos);

            // 현재 거리가 이전에 찾은 최소 거리보다 작으면
            if (curDiff < minDiff)
            {
                // 최소 거리를 갱신하고, 이 대상을 결과로 저장합니다.
                minDiff = curDiff;
                result = target.transform;
            }
        }

        return result;
    }
}