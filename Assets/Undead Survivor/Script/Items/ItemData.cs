using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 아이템 데이터의 기반이 되는 추상 ScriptableObject 클래스입니다.
/// </summary>
public abstract class ItemData : ScriptableObject
{
    public enum ItemType { Weapon, Gear, Consumable }

    [Header("# Main Info")]
    [Tooltip("아이템 종류 (무기, 장비, 소모품)")]
    public ItemType itemType;
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
}