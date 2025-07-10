using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템의 모든 데이터를 정의하는 ScriptableObject 클래스입니다.
/// ScriptableObject를 사용하면 코드 변경 없이 에셋 파일 형태로 데이터를 관리할 수 있어 편리합니다.
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    /// <summary>
    /// 아이템의 종류를 정의하는 열거형입니다.
    /// </summary>
    public enum ItemType { Melee, Range, Glove, Shoe, Heal }

    [Header("# Main Info")]
    [Tooltip("아이템 종류")]
    public ItemType itemType;
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
    [Tooltip("기본 공격력")]
    public float baseDamage;
    [Tooltip("기본 개수")]
    public int baseCount;
    [Tooltip("레벨별 공격력 증가량 (또는 효과 수치)")]
    public float[] damages;
    [Tooltip("레벨별 개수 증가량")]
    public int[] counts;

    [Header("# Weapon")]
    [Tooltip("무기일 경우, 사용할 발사체 프리팹")]
    public GameObject projectile;
    [Tooltip("무기를 장착했을 때 손에 표시될 스프라이트 (현재 미사용)")]
    public Sprite hand;
}