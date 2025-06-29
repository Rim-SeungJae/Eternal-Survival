using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TitleIntroController : MonoBehaviour
{
    public GameObject char1, char2, title, gameStartButton;

    void Start()
    {
        Time.timeScale = 1f; // 게임 오버시 일시정지된 상태를 해제
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    // 코루틴을 사용하여 타이틀 인트로 시퀀스를 재생합니다.
    {
        yield return new WaitForSeconds(1f);
        char1.SetActive(true);
        yield return char1.GetComponent<TitleObjectEffect>().PlayEffect().WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        char2.SetActive(true);
        yield return char2.GetComponent<TitleObjectEffect>().PlayEffect().WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        title.SetActive(true);
        yield return title.GetComponent<TitleObjectEffect>().PlayEffect().WaitForCompletion();

        gameStartButton.SetActive(true);

    }
}