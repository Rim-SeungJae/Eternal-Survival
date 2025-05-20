using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    public void Select(int index)
    {
        items[index].OnClick();
    }

    void Next()
    {
        foreach(Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        int[] ran = new int[items.Length];

        for(int i=0;i<ran.Length;i++) ran[i] = i;

        for(int i=0;i<ran.Length;i++)
        {
            int tmp = ran[i];
            int randomIdx = Random.Range(0,ran.Length);
            ran[i] = ran[randomIdx];
            ran[randomIdx] = tmp;
        }

        int randomCount = 0;
        for(int i=0;randomCount<3 && i<ran.Length;i++)
        {
            Item ranItem = items[ran[i]];
            if(ranItem.level == ranItem.data.damages.Length) continue;
            else
            {
                ranItem.gameObject.SetActive(true);
                randomCount++;
            }
        }
    }
}
