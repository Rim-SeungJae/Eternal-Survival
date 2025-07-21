using UnityEngine;
using DG.Tweening;

/// <summary>
/// 타이틀 화면의 오브젝트가 떨어지는 연출을 관리하는 클래스입니다.
/// DOTween 라이브러리를 사용하여 애니메이션을 구현합니다.
/// </summary>
public class TitleObjectEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("오브젝트가 떨어지기 시작하는 Z 위치")]
    public float startZ = -1000f;
    [Tooltip("오브젝트의 최종 Z 위치")]
    public float targetZ = 0f;
    [Tooltip("떨어지는 데 걸리는 시간")]
    public float dropDuration = 0.4f;
    [Tooltip("적용할 이징(Easing) 효과")]
    public Ease easing;

    [Header("Effects")]
    [Tooltip("땅에 부딪힐 때 재생할 효과음")]
    public AudioClip stampSfx;
    [Tooltip("땅에 부딪힐 때 생성할 먼지 파티클 프리팹")]
    public GameObject dustPrefab;

    private void Awake()
    {
        // 시작 시 오브젝트가 보이지 않도록 초기 Z 위치를 설정합니다.
        Vector3 pos = transform.localPosition;
        pos.z = startZ;
        transform.localPosition = pos;
    }

    /// <summary>
    /// 오브젝트가 떨어지는 효과를 재생하고, 해당 시퀀스를 반환합니다.
    /// </summary>
    /// <returns>DOTween 시퀀스</returns>
    public Sequence PlayEffect()
    {
        // 시작 위치를 다시 설정합니다.
        Vector3 start = transform.localPosition;
        start.z = startZ;
        transform.localPosition = start;

        // DOTween 시퀀스를 생성합니다.
        Sequence seq = DOTween.Sequence();

        // Z축 이동 애니메이션을 추가합니다.
        seq.Append(transform.DOLocalMoveZ(targetZ, dropDuration).SetEase(easing))
           // 애니메이션이 끝난 후 실행될 콜백을 추가합니다.
           .AppendCallback(() =>
           {
               // 먼지 파티클 생성
               if (dustPrefab)
                   Instantiate(dustPrefab, transform.position, Quaternion.identity);
               // 효과음 재생
               if (stampSfx)
                   AudioSource.PlayClipAtPoint(stampSfx, Camera.main.transform.position);
           });

        return seq;
    }
}
