using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// MagicLampWeapon에서 발사되는 주먹 투사체의 로직을 관리합니다.
/// DOTween을 사용해서 펀치감 있는 애니메이션을 구현합니다.
/// </summary>
public class MagicLampEffect : MonoBehaviour
{
    [Header("펀치 애니메이션 설정")]
    [Tooltip("펀치 준비 시간")]
    public float prepareTime = 0.3f;
    
    [Tooltip("펀치 이동 시간")]
    public float punchTime = 0.15f;
    
    [Tooltip("펀치 후 유지 시간")]
    public float holdTime = 0.2f;
    
    [Tooltip("펀치 거리")]
    public float punchDistance = 3f;
    
    [Tooltip("펀치 준비 시 스케일")]
    public float prepareScale = 0.8f;
    
    [Tooltip("펀치 시 최대 스케일")]
    public float punchScale = 1.3f;

    private float damage;
    private Vector3 direction;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool hasDealtDamage = false;
    private Sequence punchSequence;

    void OnEnable()
    {
        hasDealtDamage = false;
    }

    /// <summary>
    /// 주먹 투사체를 초기화하고 펀치 애니메이션을 시작합니다.
    /// </summary>
    public void Init(float dmg, float dur, float spd, Vector3 dir)
    {
        damage = dmg;
        direction = dir.normalized;
        startPosition = transform.position;
        targetPosition = startPosition + direction * punchDistance;
        
        // 기존 애니메이션 정리
        if (punchSequence != null)
        {
            punchSequence.Kill();
        }

        // 펀치 애니메이션 시작
        StartPunchAnimation();
    }

    /// <summary>
    /// DOTween을 사용한 펀치 애니메이션 시퀀스 (회전 효과 제외)
    /// </summary>
    private void StartPunchAnimation()
    {
        // 초기 설정
        transform.localScale = Vector3.one;
        
        // DOTween 시퀀스 생성
        punchSequence = DOTween.Sequence();
        
        // 1단계: 펀치 준비 (뒤로 약간 당기면서 스케일 축소)
        Vector3 preparePosition = startPosition - direction * 0.3f;
        punchSequence.Append(transform.DOMove(preparePosition, prepareTime * 0.6f)
            .SetEase(Ease.OutQuad));
        punchSequence.Join(transform.DOScale(prepareScale, prepareTime * 0.6f)
            .SetEase(Ease.OutQuad));
        
        // 약간의 대기 (긴장감 조성)
        punchSequence.AppendInterval(prepareTime * 0.4f);
        
        // 2단계: 펀치 발사 (빠르게 전진하면서 스케일 확대)
        punchSequence.Append(transform.DOMove(targetPosition, punchTime)
            .SetEase(Ease.OutQuint)); // 빠르게 시작해서 점점 느려짐
        punchSequence.Join(transform.DOScale(punchScale, punchTime * 0.3f)
            .SetEase(Ease.OutBack)); // 오버슈트 효과
        
        // 3단계: 임팩트 효과
        punchSequence.AppendCallback(() => {
            PlayImpactEffect();
        });
        
        // 4단계: 스케일 원복
        punchSequence.Append(transform.DOScale(Vector3.one, holdTime * 0.5f)
            .SetEase(Ease.InQuad));
        
        // 5단계: 페이드아웃 및 비활성화
        punchSequence.AppendInterval(holdTime * 0.5f);
        punchSequence.OnComplete(() => {
            StartCoroutine(FadeOutAndDeactivate());
        });
    }

    /// <summary>
    /// 임팩트 효과 - 펀치가 최대로 뻗었을 때의 효과
    /// </summary>
    private void PlayImpactEffect()
    {
        // 스케일 펀치 효과 (빠르게 커졌다 작아짐)
        transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 2, 0.5f);
        
        // 약간의 화면 쉐이크 효과를 위한 위치 펀치
        transform.DOPunchPosition(direction * 0.1f, 0.1f, 1, 0.3f);
    }

    /// <summary>
    /// 페이드아웃 효과 후 비활성화
    /// </summary>
    private IEnumerator FadeOutAndDeactivate()
    {
        // 스프라이트 렌더러 페이드아웃
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0f, 0.2f);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        Deactivate();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameTags.ENEMY) && !hasDealtDamage)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hasDealtDamage = true;
                
                // 적중 시 추가 임팩트 효과
                PlayHitEffect();
            }
        }
    }

    /// <summary>
    /// 적 적중 시 추가 임팩트 효과
    /// </summary>
    private void PlayHitEffect()
    {
        // 적중 시 더 강한 펀치 효과
        transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 3, 0.8f);
        
        // 적중 시 잠깐 정지 효과 (히트스톱)
        Time.timeScale = 0.1f;
        DOVirtual.DelayedCall(0.05f, () => {
            Time.timeScale = 1f;
        }).SetUpdate(true);
    }

    /// <summary>
    /// 투사체를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void Deactivate()
    {
        // DOTween 시퀀스 정리
        if (punchSequence != null)
        {
            punchSequence.Kill();
            punchSequence = null;
        }

        // 스프라이트 알파 원복
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        // 트랜스폼 원복
        transform.localScale = Vector3.one;

        // 풀에 반환
        Poolable poolable = GetComponent<Poolable>();
        if (poolable != null)
        {
            GameManager.instance.pool.ReturnToPool(poolable.poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        // 비활성화 시 DOTween 정리
        if (punchSequence != null)
        {
            punchSequence.Kill();
            punchSequence = null;
        }
    }
}
