using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 'Red Sprite' 무기의 번개 이펙트 자체의 동작(시각 효과, 자동 비활성화 등)을 관리합니다.
/// 피해 처리는 LightningWeapon.cs에서 직접 담당합니다.
/// </summary>
public class LightningEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("이펙트의 총 활성화 시간")]
    public float effectDuration = 0.2f; // 인스펙터에서 설정 가능

    private float timer; // 현재 경과 시간
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러 참조

    [Header("Lightning Drop Settings")]
    [Tooltip("번개가 시작될 Y축 상대 오프셋")]
    public float initialYOffset = 5f;
    [Tooltip("번개가 떨어지는 애니메이션에 걸리는 시간")]
    public float dropAnimationDuration = 0.1f; // 매우 짧게 설정

    private Vector3 initialPosition; // 이펙트가 활성화될 때의 초기 월드 위치 (타겟 적의 위치)
    private bool hasDropped = false; // 번개 떨어지는 애니메이션이 완료되었는지 여부

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱
    }

    void OnEnable()
    {
        timer = 0f;
        hasDropped = false;

        // 스프라이트가 다시 보이도록 알파 값을 1로 설정 (혹시 이전 사용에서 알파가 변경되었을 경우)
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        // 이펙트가 활성화되는 순간의 위치를 저장 (LightningWeapon에서 이미 설정된 위치)
        initialPosition = transform.position;
        // 시작 위치를 initialYOffset만큼 위로 설정
        transform.position = new Vector3(initialPosition.x, initialPosition.y + initialYOffset, initialPosition.z);

        // 만약 총 이펙트 지속 시간이 0 이하면 즉시 비활성화
        if (effectDuration <= 0)
        {
            DeactivateEffect();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 번개 떨어지는 애니메이션 처리
        if (!hasDropped && timer < dropAnimationDuration)
        {
            // 현재 위치에서 목표 위치(initialPosition.y)까지 선형 보간
            float t = timer / dropAnimationDuration; // 0에서 1까지 진행
            float currentY = Mathf.Lerp(initialYOffset, 0f, t); // 오프셋을 0으로 보간
            transform.position = new Vector3(initialPosition.x, initialPosition.y + currentY, initialPosition.z);
        }
        else if (!hasDropped && timer >= dropAnimationDuration)
        {
            // 애니메이션 완료 후 최종 위치 고정
            transform.position = initialPosition;
            hasDropped = true;
        }

        // 설정된 총 이펙트 지속시간이 지나면 즉시 비활성화됩니다.
        if (effectDuration > 0 && timer > effectDuration)
        {
            DeactivateEffect();
        }
    }

    /// <summary>
    /// 이펙트를 비활성화하고 풀에 반환합니다.
    /// </summary>
    private void DeactivateEffect()
    {
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
}