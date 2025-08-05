using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 유키 진화 무기의 마크(디버프)를 관리하는 컴포넌트입니다.
/// 적에게 동적으로 추가되어 마크 폭발 효과를 처리합니다.
/// </summary>
public class YukiWeaponEvoMark : MonoBehaviour
{
    [Header("# Mark Settings")]
    private float explosionDamage; // 폭발 시 입힐 피해량
    private float markDuration; // 마크 지속 시간
    private Enemy targetEnemy; // 마크가 적용된 적
    private bool isBeingRemoved = false; // 제거 중인지 확인하는 플래그
    private SpriteRenderer spriteRenderer;

    public SpriteRenderer explosionEffect;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// 마크를 초기화하고 타이머를 시작합니다.
    /// </summary>
    /// <param name="damage">폭발 시 입힐 피해량</param>
    /// <param name="duration">마크 지속 시간</param>
    public void InitializeMark(float damage, float duration)
    {
        explosionDamage = damage;
        markDuration = duration;
        explosionEffect.gameObject.SetActive(false);
        spriteRenderer.color = new Color(1, 1, 1, 1);
        
        // 부모 오브젝트에서 Enemy 컴포넌트 찾기 (프리팹이 적의 자식으로 배치됨)
        targetEnemy = GetComponentInParent<Enemy>();
        
        if (targetEnemy == null)
        {
            Debug.LogError("YukiWeaponEvoMark: Enemy 컴포넌트를 찾을 수 없습니다!");
            RemoveMark();
            return;
        }
        
        // 기존 마크가 있는지 확인하고 제거
        YukiWeaponEvoMark[] existingMarks = targetEnemy.GetComponentsInChildren<YukiWeaponEvoMark>();
        foreach (YukiWeaponEvoMark existingMark in existingMarks)
        {
            if (existingMark != this && existingMark != null)
            {
                existingMark.RemoveMark();
            }
        }
        // 마크 페이드아웃 시작
        spriteRenderer.DOFade(0, markDuration);

        // 마크 타이머 시작
        StartCoroutine(MarkTimer());
    }
    
    /// <summary>
    /// 마크 타이머를 관리하고 폭발을 실행합니다.
    /// </summary>
    private IEnumerator MarkTimer()
    {      
        yield return new WaitForSeconds(markDuration);
        
        // 적이 아직 살아있는지 확인
        if (targetEnemy != null && targetEnemy.gameObject.activeSelf)
        {
            // 마크 폭발 실행
            ExplodeMark();
            yield return new WaitForSeconds(0.3f);
        }
        
        
        // 마크 컴포넌트 제거
        RemoveMark();
    }

    
    /// <summary>
    /// 마크 폭발을 실행합니다.
    /// </summary>
    private void ExplodeMark()
    {
        // 적에게 피해 적용
        targetEnemy.TakeDamage(explosionDamage);

        explosionEffect.gameObject.SetActive(true);
        explosionEffect.DOFade(0, 0.3f);
        
        // 폭발 효과음 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        
        Debug.Log($"유키 마크 폭발! 피해량: {explosionDamage}");
    }
    
    /// <summary>
    /// 마크를 제거하고 정리합니다.
    /// </summary>
    public void RemoveMark()
    {
        // 이미 제거 중이면 중복 호출 방지
        if (isBeingRemoved) return;
        isBeingRemoved = true;
        
        // 정리 작업
        targetEnemy = null;
        
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
        // OnDisable에서는 정리 작업만 수행, RemoveMark 호출하지 않음
        targetEnemy = null;
        isBeingRemoved = false; // 다음 사용을 위해 플래그 리셋
    }
}