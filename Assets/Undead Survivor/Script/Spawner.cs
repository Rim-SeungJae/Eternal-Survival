using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적을 주기적으로 생성하는 스포너 클래스입니다.
/// 게임 시간에 따라 레벨이 오르며, 해당 레벨에 맞는 적을 생성합니다.
/// 스폰 시 장애물과의 충돌을 방지하는 로직을 포함합니다.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("적 생성 위치 배열. Spawner 오브젝트의 자식으로 생성 위치를 지정합니다.")]
    public Transform[] spawnPoint;
    [Tooltip("레벨별 적 생성 데이터 (ScriptableObject 참조)")]
    public SpawnDataSO[] spawnData;
    [Tooltip("레벨업에 걸리는 시간")]
    public float levelTime;

    [Header("Collision Avoidance")]
    [Tooltip("장애물로 간주할 레이어 마스크")]
    public LayerMask obstacleLayerMask;
    [Tooltip("스폰 위치를 검사할 반경 (적 크기 고려)")]
    public float spawnCheckRadius = 0.5f;
    [Tooltip("안전한 스폰 위치를 찾기 위한 최대 시도 횟수")]
    public int maxSpawnAttempts = 10;

    private float timer; // 스폰 타이머
    private int level;   // 현재 레벨

    void Awake()
    {
        // 자식 오브젝트들의 Transform을 스폰 위치로 사용합니다.
        spawnPoint = GetComponentsInChildren<Transform>();
        // 전체 게임 시간을 스폰 데이터 개수로 나누어 레벨업 시간을 계산합니다.
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool positionFound = false;

        // 1. 안전한 스폰 위치를 찾기 위해 여러 번 시도합니다.
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // 2. 랜덤한 스폰 위치를 선택합니다.
            Transform randomPoint = spawnPoint[Random.Range(1, spawnPoint.Length)];
            
            // 3. 해당 위치에 장애물이 있는지 검사합니다.
            Collider2D hit = Physics2D.OverlapCircle(randomPoint.position, spawnCheckRadius, obstacleLayerMask);

            // 4. 장애물이 없다면(hit == null), 이 위치를 사용하고 반복을 중단합니다.
            if (hit == null)
            {
                spawnPosition = randomPoint.position;
                positionFound = true;
                break;
            }
        }

        // 5. 만약 최대 시도 횟수 동안 안전한 위치를 찾지 못했다면, 경고를 남기고 스폰을 중단합니다.
        if (!positionFound)
        {
            Debug.LogWarning("안전한 스폰 위치를 찾지 못했습니다. 스폰 포인트나 장애물 레이어를 확인하세요.");
            return;
        }

        // 6. 찾은 안전한 위치에 적을 스폰합니다.
        GameObject enemy = GameManager.instance.pool.Get("Enemy");
        if (enemy == null) return;

        enemy.transform.position = spawnPosition;
        enemy.GetComponent<Enemy>().Init(spawnData[level]);
    }
}
