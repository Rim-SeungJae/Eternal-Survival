using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화면 밖으로 나간 오브젝트(배경, 적)를 플레이어 주변으로 재배치하는 역할을 합니다.
/// 무한히 반복되는 맵과 화면 밖으로 나간 적의 재등장을 구현합니다.
/// </summary>
public class EnemyController : MonoBehaviour
{
    private Collider2D coll;

    [Header("Ground Relocation Settings")]
    [Tooltip("배경 타일이 이동할 거리")]
    public float groundTranslateDistance = 40f;

    [Header("Enemy Relocation Settings")]
    [Tooltip("적 재배치 시 랜덤 범위 (X, Y)")]
    public Vector2 enemyRandomRange = new Vector2(-3f, 3f);
    [Tooltip("적 재배치 시 플레이어와의 거리 배율")]
    public float enemyPlayerDistanceMultiplier = 2f;


    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    // 'Area' 콜라이더를 벗어났을 때 호출됩니다.
    void OnTriggerExit2D(Collider2D collision)
    {
        // 'Area' 태그가 없는 다른 콜라이더와의 충돌은 무시합니다.
        if (!collision.CompareTag(GameTags.AREA))
        {
            return;
        }

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;

        // 플레이어와 오브젝트의 상대적인 위치 계산
        float dirX = playerPos.x - myPos.x;
        float dirY = playerPos.y - myPos.y;

        float diffx = Mathf.Abs(dirX);
        float diffy = Mathf.Abs(dirY);

        // 플레이어가 멀어진 방향을 단위 벡터로 저장
        dirX = dirX > 0 ? 1 : -1;
        dirY = dirY > 0 ? 1 : -1;

        // 오브젝트의 태그에 따라 다른 재배치 로직을 수행합니다.
        switch (transform.tag)
        {
            case "Ground": // 배경 타일 재배치
                // 플레이어와 더 멀리 떨어진 축 방향으로 타일을 이동시켜 무한 맵처럼 보이게 합니다.
                if (diffx > diffy)
                {
                    transform.Translate(Vector3.right * dirX * groundTranslateDistance);
                }
                else
                {
                    transform.Translate(Vector3.up * dirY * groundTranslateDistance);
                }
                break;
            case GameTags.ENEMY: // 적 재배치
                // 콜라이더가 활성화된 적(살아있는 적)만 재배치합니다.
                if (coll.enabled)
                {
                    // 플레이어 방향으로 이동시키되, 약간의 랜덤성을 더해줍니다.
                    Vector3 dist = playerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(enemyRandomRange.x, enemyRandomRange.y), Random.Range(enemyRandomRange.x, enemyRandomRange.y), 0);
                    transform.Translate(ran + dist * enemyPlayerDistanceMultiplier);
                }
                break;
        }
    }
}
