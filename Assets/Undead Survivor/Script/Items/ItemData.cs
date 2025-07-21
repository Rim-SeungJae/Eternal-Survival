using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템의 모든 데이터를 정의하는 ScriptableObject 클래스입니다.
/// ScriptableObject를 사용하면 코드 변경 없이 에셋 파일 형태로 데이터를 관리할 수 있어 편리합니다.
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("# Main Info")]
    [Tooltip("이 아이템이 수행할 행동을 정의하는 ScriptableObject")]
    public ItemAction itemAction;
    [Tooltip("아이템 고유 ID")]
    public int itemId;
    [Tooltip("아이템 이름")]
    public string itemName;
    [Tooltip("아이템 설명 (여러 줄 입력 가능)")]
    [TextArea]
    public string itemDesc;
    [Tooltip("아이템 아이콘 스프라이트")]
    public Sprite itemIcon;

    [Header("# Level Data")]
    [Tooltip("아이템의 최대 레벨")]
    public int maxLevel = 4;
    [Tooltip("레벨별 피해량")]
    public float[] damages;
    [Tooltip("레벨별 투사체 속도")]
    public float[] projectileSpeeds;
    [Tooltip("레벨별 지속 시간(음수일 경우 무한)")]
    public float[] durations;
    [Tooltip("레벨별 공격 범위")]
    public float[] areas;
    [Tooltip("레벨별 쿨타임")]
    public float[] cooldowns;
    [Tooltip("레벨별 투사체 개수")]
    public int[] counts;

    [Header("# Weapon")]
    [Tooltip("무기일 경우, PoolManager에 등록된 발사체의 태그")]
    [PoolTagSelector] // 이 어트리뷰트를 추가!
    public string projectileTag;
    [Tooltip("무기를 장착했을 때 손에 표시될 스프라이트 (현재 미사용)")]
    public Sprite hand;
}