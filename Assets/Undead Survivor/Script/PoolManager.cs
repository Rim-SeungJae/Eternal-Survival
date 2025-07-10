using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링을 관리하는 클래스입니다.
/// 자주 사용되는 게임 오브젝트(총알, 적 등)를 미리 생성하고 재활용하여
/// 게임 실행 중의 성능 저하(특히 GC Spike)를 방지합니다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    [Tooltip("풀링할 프리팹 배열. 인덱스가 ID로 사용됩니다.")]
    public GameObject[] prefabs;

    // 각 프리팹 종류별로 오브젝트 리스트를 관리합니다.
    private List<GameObject>[] pools;

    private void Awake()
    {
        // 프리팹 배열 크기에 맞춰 풀 리스트를 초기화합니다.
        pools = new List<GameObject>[prefabs.Length];

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }
    }

    /// <summary>
    /// 지정된 인덱스에 해당하는 오브젝트를 풀에서 가져옵니다.
    /// </summary>
    /// <param name="index">가져올 프리팹의 인덱스 (ID)</param>
    /// <returns>활성화된 게임 오브젝트</returns>
    public GameObject Get(int index)
    {
        GameObject select = null;

        // 해당 풀에서 비활성화 상태인(사용 가능한) 오브젝트를 찾습니다.
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true); // 찾았으면 활성화하고
                break;                  // 반복문을 빠져나옵니다.
            }
        }

        // 만약 사용 가능한 오브젝트가 풀에 없다면
        if (!select)
        {
            // 새로 생성하고 풀에 추가합니다.
            select = Instantiate(prefabs[index], transform); // PoolManager 하위에 생성
            pools[index].Add(select);
        }

        return select;
    }
}