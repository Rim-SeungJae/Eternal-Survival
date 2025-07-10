using UnityEngine;

/// <summary>
/// 씬(Scene) 간에 데이터를 공유하기 위한 static 클래스입니다.
/// 여기서는 캐릭터 선택 씬에서 게임 씬으로 선택된 캐릭터의 ID를 전달하는 데 사용됩니다.
/// </summary>
public static class GlobalData
{
    /// <summary>
    /// 선택된 CharacterDataSO를 저장하는 static 변수입니다.
    /// 게임 시작 시 플레이어의 능력치 초기화에 사용됩니다.
    /// </summary>
    public static CharacterDataSO selectedCharacterDataSO; // CharacterDataSO 타입으로 변경
}
