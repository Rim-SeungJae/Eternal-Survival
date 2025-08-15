using UnityEngine;

/// <summary>
/// 보스 몬스터 생성을 위한 데이터 클래스입니다. ScriptableObject로 관리되어 에디터에서 유연하게 설정할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "NewBossData", menuName = "Scriptable Objects/Boss Data")]
public class BossDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("보스 이름")]
    public string bossName;
    [Tooltip("보스 스프라이트 타입 (애니메이터 컨트롤러 인덱스)")]
    public int spriteType;
    
    [Header("스탯")]
    [Tooltip("체력")]
    public int health;
    [Tooltip("이동 속도")]
    public float speed;
    [Tooltip("플레이어에게 입히는 접촉 데미지")]
    public float contactDamage;
    
    [Header("등장 조건")]
    [Tooltip("등장 시간 (초 단위)")]
    public float spawnTime;
    [Tooltip("등장에 필요한 최소 킬 수")]
    public int requiredKills;
    [Tooltip("등장에 필요한 최소 플레이어 레벨")]
    public int requiredLevel;
    
    [Header("특수 공격")]
    [Tooltip("특수 공격 사용 간격 (초)")]
    public float specialAttackCooldown = 10f;
    [Tooltip("특수 공격 범위")]
    public float specialAttackRange = 5f;
    [Tooltip("특수 공격 데미지")]
    public float specialAttackDamage = 50f;
    
    [Header("보상")]
    [Tooltip("처치 시 획득 경험치")]
    public int expReward = 100;
    [Tooltip("드롭 가능한 아이템 목록과 각 아이템의 드롭 확률")]
    public LootItem[] lootTable;
    [Tooltip("보스 아이콘")]
    public Sprite bossIcon;
}