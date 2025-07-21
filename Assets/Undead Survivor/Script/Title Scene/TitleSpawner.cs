using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 타이틀 화면의 인트로 연출 시퀀스를 관리하는 클래스입니다.
/// 코루틴을 사용하여 정해진 순서대로 오브젝트들을 등장시킵니다.
/// </summary>
public class TitleIntroController : MonoBehaviour
{
    [Header("Title Objects")]
    [Tooltip("등장시킬 오브젝트들")]
    public GameObject char1, char2, title, gameStartButton;

    void Start()
    {
        // 게임 오버나 일시정지 후 타이틀로 돌아왔을 때를 대비하여 Time.timeScale을 1로 복원합니다.
        Time.timeScale = 1f;
        // 인트로 시퀀스 코루틴을 시작합니다.
        StartCoroutine(PlayIntroSequence());
    }

    /// <summary>
    /// 정해진 순서와 시간에 따라 타이틀 오브젝트들을 등장시키는 코루틴입니다.
    /// </summary>
    IEnumerator PlayIntroSequence()
    {
        // 초기 딜레이
        yield return new WaitForSeconds(2f);

        // 1번 캐릭터 등장
        char1.SetActive(true);
        // TitleObjectEffect의 PlayEffect가 끝날 때까지 대기합니다.
        yield return char1.GetComponent<TitleObjectEffect>().PlayEffect().WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        // 2번 캐릭터 등장
        char2.SetActive(true);
        yield return char2.GetComponent<TitleObjectEffect>().PlayEffect().WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        // 타이틀 로고 등장
        title.SetActive(true);
        yield return title.GetComponent<TitleObjectEffect>().PlayEffect().WaitForCompletion();

        // 모든 연출이 끝난 후 게임 시작 버튼 활성화
        gameStartButton.SetActive(true);
    }
}
