using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 총알의 로직을 관리하는 클래스입니다.
/// 데미지, 관통 횟수, 이동 등을 처리합니다.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("총알의 공격력")]
    public float damage;
    [Tooltip("관통 가능 횟수. -100은 무한 관통을 의미합니다.")]
    public int per;

    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 총알을 초기화하고 발사합니다.
    /// </summary>
    /// <param name="damage">데미지</param>
    /// <param name="per">관통 횟수</param>
    /// <param name="dir">발사 방향</param>
    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;

        // 관통 횟수가 0 이상일 때만(원거리 무기일 때) 물리적인 힘을 가합니다.
        if (per >= 0)
        {
            rigid.linearVelocity = dir * 15f; // 속도 대신 velocity를 사용하여 일정한 속도로 이동
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 적과 충돌하지 않았거나, 무한 관통 상태(-100)가 아닐 때만 로직을 실행합니다.
        if (!collision.CompareTag("Enemy") || per == -100)
        {
            return;
        }

        // 관통 횟수를 1 감소시킵니다.
        per--;

        // 관통 횟수를 모두 소진하면
        if (per < 0)
        {
            // 총알을 멈추고 비활성화하여 풀에 반환합니다.
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 화면 밖으로 나갔을 때 총알을 비활성화하는 함수입니다.
    /// (원래 코드의 OTriggerExit2D는 오타로 보이며, OnTriggerExit2D로 수정했습니다)
    /// </summary>
    void OnTriggerExit2D(Collider2D collision)
    {
        // 'Area' 태그를 가진 경계 영역을 벗어났고, 무한 관통이 아닐 때
        if (!collision.CompareTag("Area") || per == -100)
        {
            return;
        }

        // 총알을 멈추고 비활성화합니다.
        rigid.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}