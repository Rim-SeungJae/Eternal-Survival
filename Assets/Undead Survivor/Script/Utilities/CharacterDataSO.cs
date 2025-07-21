using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Scriptable Objects/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    [TextArea]
    public string description;
    public Sprite portrait;
    public Sprite skillImage;
    public int characterId; // Unique ID for this character

    [Header("Stats Multipliers")]
    [Tooltip("플레이어 이동 속도 배율 (기본 1.0)")]
    public float speedMultiplier = 1.0f;
    [Tooltip("무기 공격 속도 배율 (기본 1.0)")]
    public float weaponSpeedMultiplier = 1.0f;
    [Tooltip("무기 발사 속도 배율 (기본 1.0)")]
    public float weaponRateMultiplier = 1.0f;
    [Tooltip("기본 체력")]
    public float baseHealth = 100f;

    [Header("Visuals")]
    [Tooltip("캐릭터 애니메이터 컨트롤러")]
    public RuntimeAnimatorController animatorController;
}
