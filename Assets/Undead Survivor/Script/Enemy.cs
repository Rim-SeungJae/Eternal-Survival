using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;
    public Transform shadow;
    public DayNightController dayNightController;
    private SpriteRenderer spriteren;

    bool isLive;
    bool isKnockBack;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;
    CapsuleCollider2D capsule;

    // Start is called before the first frame update
    void Awake()
    {
        capsule = GetComponent<CapsuleCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        shadow = transform.Find("Shadow");
        wait = new WaitForFixedUpdate();
        spriteren = GetComponent<SpriteRenderer>();
        dayNightController = FindObjectsByType<DayNightController>(FindObjectsSortMode.None)[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!GameManager.instance.isLive) return;

        if(!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit") ) return;

        if(isKnockBack) return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (GameManager.instance.IsNight())
        {
            Vector3 playerPos = GameManager.instance.player.transform.position;
            float dist = Vector3.Distance(playerPos, transform.position);
            spriteren.enabled = dist < dayNightController.CurrentLightRadius / 2.0f;
        }
        else if (!spriteren.enabled)
        {
            spriteren.enabled = true;
        }
    }


    private void LateUpdate()
    {
        if (!GameManager.instance.isLive) return;

        if (!isLive) return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        isKnockBack = false;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead",false);
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        if (shadow != null)
        {
            Vector3 pos = shadow.localPosition;
            pos = data.shadowOffset;
            shadow.localPosition = pos;
            shadow.localScale = data.shadowSize;
        }
        capsule.size = data.colliderSize;
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = maxHealth;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(! collision.CompareTag("Bullet") || !isLive) return;

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());

        if(health > 0 )
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.SetBool("Dead",true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
            
            if(GameManager.instance.isLive) AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator KnockBack()
    {
        isKnockBack = true;
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.1f); // 넉백 지속시간
        isKnockBack = false;
    }

    public void Dead()
    {
        gameObject.SetActive(false);
    }
}
