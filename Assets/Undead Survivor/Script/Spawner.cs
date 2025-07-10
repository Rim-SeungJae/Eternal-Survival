using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적을 주기적으로 생성하는 스포너 클래스입니다.
/// 게임 시간에 따라 레벨이 오르며, 해당 레벨에 맞는 적을 생성합니다.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Tooltip("적 생성 위치 배열. Spawner 오브젝트의 자식으로 생성 위치를 지정합니다.")]
    public Transform[] spawnPoint;
    [Tooltip("레벨별 적 생성 데이터")]
    public SpawnData[] spawnData;
    [Tooltip("레벨업에 걸리는 시간")]
    public float levelTime;

    private float timer; // 스폰 타이머
    private int level;   // 현재 레벨

    void Awake()
    {
        // 자식 오브젝트들의 Transform을 스폰 위치로 사용합니다.
        // GetComponentsInChildren는 부모(자신)도 포함하므로, 실제 사용 시에는 1번 인덱스부터 사용해야 합니다.
        spawnPoint = GetComponentsInChildren<Transform>();
        // 전체 게임 시간을 스폰 데이터 개수로 나누어 레벨업 시간을 계산합니다.
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        // 현재 게임 시간에 따라 레벨을 결정합니다.
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        // 현재 레벨의 스폰 주기가 되면 적을 생성합니다.
        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    /// <summary>
    /// 적을 생성하고 초기화합니다.
    /// </summary>
    void Spawn()
    {
        // 풀 매니저에서 적 오브젝트를 가져옵니다. (Get(0)은 적 프리팹을 의미)
        GameObject enemy = GameManager.instance.pool.Get(0);
        // 지정된 스폰 위치 중 한 곳에 랜덤하게 배치합니다.
        // spawnPoint[0]은 부모(Spawner) 자신이므로 1부터 시작합니다.
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        // 현재 레벨에 맞는 데이터로 적을 초기화합니다.
        enemy.GetComponent<Enemy>().Init(spawnData[level]);
    }
}

/// <summary>
/// 적 생성을 위한 데이터 클래스입니다.
/// ScriptableObject로 만들면 더 유연하게 관리할 수 있습니다.
/// </summary>
[System.Serializable]
public class SpawnData
{
    [Tooltip("적 스프라이트 타입 (애니메이터 컨트롤러 인덱스)")]
    public int spriteType;
    [Tooltip("생성 주기 (초)")]
    public float spawnTime;
    [Tooltip("체력")]
    public int health;
    [Tooltip("이동 속도")]
    public float speed;
    [Tooltip("그림자 위치 오프셋")]
    public Vector2 shadowOffset;
    [Tooltip("그림자 크기")]
    public Vector2 shadowSize;
    [Tooltip("콜라이더 크기")]
    public Vector2 colliderSize;
    [Tooltip("플레이어에게 입히는 접촉 데미지")]
    public float contactDamage; // 새로 추가된 필드
}