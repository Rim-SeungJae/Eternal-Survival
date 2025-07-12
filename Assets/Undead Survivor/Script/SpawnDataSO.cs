using UnityEngine;

/// <summary>
/// 적 생성을 위한 데이터 클래스입니다. ScriptableObject로 관리되어 에디터에서 유연하게 설정할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "NewSpawnData", menuName = "Scriptable Objects/Spawn Data")]
public class SpawnDataSO : ScriptableObject
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
    public float contactDamage;
}
