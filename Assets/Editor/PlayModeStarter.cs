using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이모드 시작 시 자동으로 TitleScene에서 시작하도록 하는 에디터 스크립트입니다.
/// </summary>
[InitializeOnLoad]
public class PlayModeStarter
{
    private static string titleScenePath = "Assets/Scenes/TitleScene.unity";
    private static string lastActiveScene;
    
    static PlayModeStarter()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // 플레이모드 진입 전: 현재 씬 저장하고 TitleScene으로 변경
                lastActiveScene = SceneManager.GetActiveScene().path;
                
                if (lastActiveScene != titleScenePath)
                {
                    // 현재 씬이 TitleScene이 아니라면 TitleScene으로 변경
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(titleScenePath);
                }
                break;
                
            case PlayModeStateChange.ExitingPlayMode:
                // 플레이모드 종료 시: 원래 씬으로 복원
                if (!string.IsNullOrEmpty(lastActiveScene) && lastActiveScene != titleScenePath)
                {
                    EditorApplication.delayCall += () =>
                    {
                        EditorSceneManager.OpenScene(lastActiveScene);
                    };
                }
                break;
        }
    }
}