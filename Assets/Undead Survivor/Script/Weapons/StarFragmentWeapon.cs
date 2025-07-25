using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Star Fragment 무기 - 사정거리 내 적 위치에 메테오를 떨어뜨리는 무기
/// 충돌 예정 위치를 미리 표시하고 일정 시간 후 메테오가 충돌하여 피해를 줍니다.
/// </summary>
public class StarFragmentWeapon : WeaponBase
{
    [Header("메테오 낙하 연출")]
    [Tooltip("메테오 낙하 시간")]
    public float meteorFallDuration = 0.5f;
    
    [Tooltip("메테오 낙하 시작 오프셋 (오른쪽 위에서 시작)")]
    public Vector2 meteorStartOffset = new Vector2(10f, 20f);

    private float timer;
    private StarFragmentData starFragmentData;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Init(ItemData data)
    {
        base.Init(data);
        this.starFragmentData = data as StarFragmentData;
        
        if (starFragmentData == null)
        {
            Debug.LogError($"[{name}] StarFragmentData is null! WeaponData should be StarFragmentData type.");
        }
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        if (timer > cooldown.Value)
        {
            timer = 0f;
            Attack();
        }
    }

    private void Attack()
    {
        // 사정거리 내 적들을 찾습니다
        Enemy[] enemiesInRange = FindEnemiesInRange();
        
        if (enemiesInRange.Length == 0) return;

        // 레벨에 따른 동시 공격 수만큼 적을 선택
        int attackCount = Mathf.Min((int)count.Value, enemiesInRange.Length);
        
        for (int i = 0; i < attackCount; i++)
        {
            Vector3 targetPos = enemiesInRange[i].transform.position;
            StartCoroutine(LaunchMeteor(targetPos));
        }
    }

    private Enemy[] FindEnemiesInRange()
    {
        if (starFragmentData == null) return new Enemy[0];
        
        // 플레이어 주변 사정거리 내의 모든 적을 찾습니다
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, starFragmentData.attackRange);
        System.Collections.Generic.List<Enemy> enemies = new System.Collections.Generic.List<Enemy>();

        foreach (Collider2D col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemies.Add(enemy);
            }
        }

        return enemies.ToArray();
    }

    private IEnumerator LaunchMeteor(Vector3 targetPos)
    {
        // 1. 충돌 예정 위치에 경고 표시 (Circle01_v1 스프라이트 사용)
        GameObject warningIndicator = GameManager.instance.pool.Get(starFragmentData.warningProjectileTag);
        if (warningIndicator != null)
        {
            warningIndicator.transform.position = targetPos;
            warningIndicator.transform.localScale = Vector3.one * attackArea.Value;
            
            // 경고 표시 초기화
            StarFragmentWarning warningComponent = warningIndicator.GetComponent<StarFragmentWarning>();
            if (warningComponent != null)
            {
                warningComponent.Init(attackArea.Value);
            }
            
            warningIndicator.SetActive(true);
        }

        // 2. 경고 시간만큼 대기
        yield return new WaitForSeconds(starFragmentData.warningDurations.Length > level ? starFragmentData.warningDurations[level] : starFragmentData.warningDurations[starFragmentData.warningDurations.Length - 1]);

        // 3. 경고 표시 제거
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(false);
        }

                 // 4. 메테오 낙하 연출 시작
         StartCoroutine(MeteorFallAnimation(targetPos));
     }

    private IEnumerator MeteorFallAnimation(Vector3 targetPos)
    {
        // 메테오 이펙트 생성
        GameObject meteor = GameManager.instance.pool.Get(starFragmentData.projectileTag);
        if (meteor == null) yield break;

        // 시작 위치 계산 (타겟 위치에서 오른쪽 위 오프셋)
        Vector3 startPos = targetPos + new Vector3(meteorStartOffset.x, meteorStartOffset.y, 0f);
        
        // 메테오 초기 설정
        meteor.transform.position = startPos;
        meteor.transform.localScale = Vector3.one * attackArea.Value;
        meteor.SetActive(true);

        // 메테오 컴포넌트 가져오기 (피해 적용을 나중에 하기 위해)
        StarFragmentMeteor meteorComponent = meteor.GetComponent<StarFragmentMeteor>();
        if (meteorComponent != null)
        {
            // 낙하 중에는 피해를 주지 않도록 설정
            meteorComponent.SetDamageEnabled(false);
        }

        // DOTween을 사용한 낙하 애니메이션
        Sequence meteorSequence = DOTween.Sequence();
        
        // 1. 메테오 낙하 (위치 이동 + 회전)
        meteorSequence.Append(meteor.transform.DOMove(targetPos, meteorFallDuration)
            .SetEase(Ease.InQuad)); // 중력처럼 가속하면서 떨어지는 효과
        
        
        // 2. 낙하 완료 후 콜백
        meteorSequence.OnComplete(() => {
            // 충돌 시 피해 적용 및 이펙트 시작
            if (meteorComponent != null)
            {
                meteorComponent.Init(damage.Value, attackArea.Value, duration.Value);
            }
        });

        yield return meteorSequence.WaitForCompletion();
    }

    // 기즈모로 사정거리 표시 (에디터에서만)
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (player != null && starFragmentData != null)
        {
            Gizmos.color = Color.yellow;
            DrawWireCircle(player.transform.position, starFragmentData.attackRange);
        }
    }

    private void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
} 