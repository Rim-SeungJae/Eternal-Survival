using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 몬스터의 등장 조건을 관리하고 적절한 타이밍에 보스를 스폰하는 매니저 클래스입니다.
/// </summary>
public class BossSpawnManager : MonoBehaviour
{
    [Header("Boss Spawn Settings")]
    [SerializeField] private BossSpawnData[] bossSpawnData;
    [SerializeField] private Transform[] bossSpawnPoints;
    [SerializeField] private float spawnDistanceFromPlayer = 15f;
    
    private HashSet<int> spawnedBosses = new HashSet<int>();
    private bool isGameActive = true;
    
    [System.Serializable]
    public class BossSpawnData
    {
        [Tooltip("보스 프리팹")]
        public GameObject bossPrefab;
        [Tooltip("보스 데이터")]
        public BossDataSO bossData;
        [Tooltip("이 보스의 고유 ID")]
        public int bossId;
        [Tooltip("한 게임에서 여러 번 스폰 가능한지")]
        public bool canRespawn = false;
        [Tooltip("리스폰 간격 (초)")]
        public float respawnInterval = 300f; // 5분
        
        [HideInInspector] public float lastSpawnTime = -1f;
    }
    
    void Start()
    {
        // 스폰 포인트가 없다면 기본 생성
        if (bossSpawnPoints == null || bossSpawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }
    }
    
    void Update()
    {
        if (!isGameActive || !GameManager.instance.isLive) return;
        
        CheckBossSpawnConditions();
    }
    
    /// <summary>
    /// 모든 보스의 스폰 조건을 확인합니다.
    /// </summary>
    private void CheckBossSpawnConditions()
    {
        foreach (var spawnData in bossSpawnData)
        {
            if (ShouldSpawnBoss(spawnData))
            {
                SpawnBoss(spawnData);
            }
        }
    }
    
    /// <summary>
    /// 보스를 스폰해야 하는지 판단합니다.
    /// </summary>
    private bool ShouldSpawnBoss(BossSpawnData spawnData)
    {
        // 이미 스폰된 보스이고 리스폰 불가능한 경우
        if (spawnedBosses.Contains(spawnData.bossId) && !spawnData.canRespawn)
        {
            return false;
        }
        
        // 리스폰 가능하지만 아직 리스폰 시간이 되지 않은 경우
        if (spawnData.canRespawn && spawnData.lastSpawnTime >= 0)
        {
            if (Time.time - spawnData.lastSpawnTime < spawnData.respawnInterval)
            {
                return false;
            }
        }
        
        // 등장 조건 확인
        return CheckSpawnConditions(spawnData.bossData);
    }
    
    /// <summary>
    /// 보스 데이터의 등장 조건을 확인합니다.
    /// </summary>
    private bool CheckSpawnConditions(BossDataSO bossData)
    {
        // 시간 조건 확인
        if (GameManager.instance.gameTime < bossData.spawnTime)
        {
            return false;
        }
        
        // 킬 수 조건 확인
        if (GameManager.instance.kill < bossData.requiredKills)
        {
            return false;
        }
        
        // 레벨 조건 확인
        if (GameManager.instance.level < bossData.requiredLevel)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 보스를 스폰합니다.
    /// </summary>
    private void SpawnBoss(BossSpawnData spawnData)
    {
        Vector3 spawnPosition = GetSafeSpawnPosition();
        
        // 보스 생성
        GameObject bossObject = Instantiate(spawnData.bossPrefab, spawnPosition, Quaternion.identity);
        BossBase bossComponent = bossObject.GetComponent<BossBase>();
        
        if (bossComponent != null)
        {
            bossComponent.InitializeBoss(spawnData.bossData);
        }
        
        // 스폰 기록 업데이트
        spawnedBosses.Add(spawnData.bossId);
        spawnData.lastSpawnTime = Time.time;
        
        // GameManager에 보스 등록
        if (GameManager.instance != null)
        {
            Enemy enemyComponent = bossObject.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                GameManager.instance.RegisterEnemy(enemyComponent);
            }
        }
        
        Debug.Log($"보스 '{spawnData.bossData.bossName}' 스폰 완료!");
    }
    
    /// <summary>
    /// 안전한 스폰 위치를 찾습니다.
    /// </summary>
    private Vector3 GetSafeSpawnPosition()
    {
        if (GameManager.instance?.player == null)
        {
            return Vector3.zero;
        }
        
        Vector3 playerPosition = GameManager.instance.player.transform.position;
        
        /*
        // 스폰 포인트가 있다면 플레이어에서 적절한 거리에 있는 것을 선택
        if (bossSpawnPoints != null && bossSpawnPoints.Length > 0)
        {
            Transform bestSpawnPoint = null;
            float bestDistance = 0f;
            
            foreach (var spawnPoint in bossSpawnPoints)
            {
                if (spawnPoint == null) continue;
                
                float distance = Vector3.Distance(playerPosition, spawnPoint.position);
                
                // 적절한 거리 범위에 있는 스폰 포인트 중 가장 적절한 것 선택
                if (distance >= spawnDistanceFromPlayer && distance <= spawnDistanceFromPlayer * 2f)
                {
                    if (bestSpawnPoint == null || distance < bestDistance)
                    {
                        bestSpawnPoint = spawnPoint;
                        bestDistance = distance;
                    }
                }
            }
            
            if (bestSpawnPoint != null)
            {
                return bestSpawnPoint.position;
            }
        }
        */
        
        // 스폰 포인트가 없거나 적절한 것이 없다면 플레이어 주변 랜덤 위치에서 생성
        return GetRandomPositionAroundPlayer(playerPosition);
    }
    
    /// <summary>
    /// 플레이어 주변의 랜덤한 위치를 반환합니다.
    /// </summary>
    private Vector3 GetRandomPositionAroundPlayer(Vector3 playerPosition)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = spawnDistanceFromPlayer;
        
        Vector3 spawnPosition = playerPosition + new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );
        
        return spawnPosition;
    }
    
    /// <summary>
    /// 기본 스폰 포인트들을 생성합니다.
    /// </summary>
    private void CreateDefaultSpawnPoints()
    {
        GameObject spawnPointParent = new GameObject("Boss Spawn Points");
        spawnPointParent.transform.SetParent(transform);
        
        List<Transform> spawnPoints = new List<Transform>();
        
        // 4개의 기본 스폰 포인트 생성 (상, 하, 좌, 우)
        Vector3[] directions = {
            Vector3.up * spawnDistanceFromPlayer,
            Vector3.down * spawnDistanceFromPlayer,
            Vector3.left * spawnDistanceFromPlayer,
            Vector3.right * spawnDistanceFromPlayer
        };
        
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject spawnPoint = new GameObject($"Boss Spawn Point {i + 1}");
            spawnPoint.transform.SetParent(spawnPointParent.transform);
            spawnPoint.transform.position = directions[i];
            spawnPoints.Add(spawnPoint.transform);
        }
        
        bossSpawnPoints = spawnPoints.ToArray();
    }
    
    /// <summary>
    /// 보스가 처치되었을 때 호출됩니다.
    /// </summary>
    public void OnBossDefeated(int bossId)
    {
        // 리스폰 불가능한 보스는 스폰 기록에서 제거하지 않음
        // 리스폰 가능한 보스는 다시 스폰될 수 있도록 함
        Debug.Log($"보스 ID {bossId} 처치됨");
    }
    
    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    public void StopSpawning()
    {
        isGameActive = false;
    }
    
    /// <summary>
    /// 게임 재시작 시 호출
    /// </summary>
    public void ResetSpawning()
    {
        spawnedBosses.Clear();
        isGameActive = true;
        
        foreach (var spawnData in bossSpawnData)
        {
            spawnData.lastSpawnTime = -1f;
        }
    }
}