using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터별 고유 능력치를 제공하는 static 클래스입니다.
/// 이 클래스는 MonoBehaviour를 상속하지만, 인스턴스화되지 않고 static 속성만 제공합니다.
/// 더 많은 캐릭터와 능력치가 생긴다면 ScriptableObject 기반의 데이터 중심으로 전환하는 것이 좋습니다.
/// </summary>
public class Character : MonoBehaviour
{
    /// <summary>
    /// 캐릭터의 이동 속도 보정값입니다.
    /// </summary>
    public static float Speed
    {
        get
        {
            // 플레이어 ID가 0일 경우 이동 속도를 10% 증가시킵니다.
            return GameManager.instance.playerId == 0 ? 1.1f : 1f;
        }
    }

    /// <summary>
    /// 근접 무기(ID:0)의 회전 속도 보정값입니다.
    /// </summary>
    public static float WeaponSpeed
    {
        get
        {
            // 플레이어 ID가 1일 경우 무기 속도를 10% 증가시킵니다.
            return GameManager.instance.playerId == 1 ? 1.1f : 1f;
        }
    }

    /// <summary>
    /// 원거리 무기의 공격 속도(쿨타임) 보정값입니다.
    /// </summary>
    public static float WeaponRate
    {
        get
        {
            // 플레이어 ID가 1일 경우 공격 속도를 10% 감소(쿨타임 감소)시킵니다.
            return GameManager.instance.playerId == 1 ? 0.9f : 1f;
        }
    }
}