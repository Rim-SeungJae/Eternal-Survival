using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨업 시 아이템 선택 UI를 관리하는 클래스입니다.
/// </summary>
public class LevelUp : MonoBehaviour
{
    private RectTransform rect;
    private Item[] items; // 자식으로 있는 아이템 UI 요소들

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        // 비활성화된 자식도 포함하여 모든 Item 컴포넌트를 가져옵니다.
        items = GetComponentsInChildren<Item>(true);
    }

    /// <summary>
    /// 레벨업 UI를 표시합니다.
    /// </summary>
    public void Show()
    {
        // 선택할 아이템들을 랜덤하게 준비합니다.
        Next();
        // UI를 화면에 보이게 하고 게임을 정지시킵니다.
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        // 관련 효과음을 재생합니다.
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    /// <summary>
    /// 레벨업 UI를 숨깁니다.
    /// </summary>
    public void Hide()
    {
        // UI를 화면에서 보이지 않게 하고 게임을 재개합니다.
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        // 관련 효과음을 재생합니다.
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    /// <summary>
    /// 특정 인덱스의 아이템을 선택합니다.
    /// </summary>
    /// <param name="index">선택한 아이템의 인덱스</param>
    public void Select(int index)
    {
        items[index].OnClick();
    }

    /// <summary>
    /// 다음 레벨업 시 보여줄 아이템 3개를 랜덤하게 선택하여 활성화합니다.
    /// </summary>
    private void Next()
    {
        // 먼저 모든 아이템을 비활성화합니다.
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 아이템 인덱스를 랜덤하게 섞습니다. (Fisher-Yates shuffle)
        int[] ran = new int[items.Length];
        for (int i = 0; i < ran.Length; i++)
        {
            ran[i] = i;
        }

        for (int i = 0; i < ran.Length; i++)
        {
            int tmp = ran[i];
            int randomIdx = Random.Range(i, ran.Length);
            ran[i] = ran[randomIdx];
            ran[randomIdx] = tmp;
        }

        // 섞인 인덱스에서 3개를 고릅니다.
        int randomCount = 0;
        for (int i = 0; i < ran.Length && randomCount < 3; i++)
        {
            Item ranItem = items[ran[i]];
            // 이미 마스터 레벨인 아이템은 건너뜁니다.
            if (ranItem.level == ranItem.data.damages.Length)
            {
                continue;
            }
            else
            {
                ranItem.gameObject.SetActive(true);
                randomCount++;
            }
        }
    }
}