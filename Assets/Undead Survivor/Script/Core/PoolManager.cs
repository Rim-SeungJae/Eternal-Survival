using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링을 관리하는 클래스입니다.
/// 비활성화된 오브젝트만 큐에 보관하는 정석적인 방식으로, 빠르고 효율적인 Get/Return을 지원합니다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    // 성능 최적화 상수
    private const int DYNAMIC_EXPANSION_SIZE = 5; // 동적 확장 시 한 번에 생성할 오브젝트 수
    
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Tooltip("풀링할 오브젝트 목록")]
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;

    private void Awake()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                // Poolable 컴포넌트를 추가하여 자신의 태그를 알도록 합니다.
                obj.AddComponent<Poolable>().poolTag = pool.tag;
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
            prefabDictionary.Add(pool.tag, pool.prefab);
        }
    }

    /// <summary>
    /// 지정된 태그에 해당하는 오브젝트를 풀에서 가져옵니다.
    /// 풀에 사용 가능한 오브젝트가 없으면 새로 생성합니다.
    /// 반환되는 오브젝트는 항상 비활성화된 상태입니다. 사용 후 명시적으로 활성화해야 합니다.
    /// </summary>
    public GameObject Get(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[tag];
        GameObject objToReturn;

        // 큐에 사용 가능한 오브젝트가 있으면 꺼내서 사용합니다.
        if (objectQueue.Count > 0)
        {
            objToReturn = objectQueue.Dequeue();
            // objToReturn.SetActive(true); // 즉시 활성화하지 않음
        }
        // 큐가 비어있으면 여러 개를 한 번에 생성하여 GC 압박을 줄입니다.
        else
        {
            // 동적 확장: 한 번에 여러 개 생성
            for (int i = 0; i < DYNAMIC_EXPANSION_SIZE; i++)
            {
                GameObject newObj = Instantiate(prefabDictionary[tag], transform);
                newObj.AddComponent<Poolable>().poolTag = tag;
                newObj.SetActive(false);
                
                // 첫 번째 오브젝트는 반환용, 나머지는 플에 추가
                if (i == 0)
                {
                    objToReturn = newObj;
                }
                else
                {
                    objectQueue.Enqueue(newObj);
                }
            }
        }
        
        // 항상 비활성화된 상태로 반환
        objToReturn.SetActive(false);
        return objToReturn;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 다시 풀에 반환합니다.
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            Destroy(obj); // 풀이 없으면 그냥 파괴
            return;
        }
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}

/// <summary>
/// 풀링되는 모든 오브젝트에 부착되어, 자신의 풀 태그를 저장하는 역할을 합니다.
/// </summary>
public class Poolable : MonoBehaviour
{
    public string poolTag;
}
