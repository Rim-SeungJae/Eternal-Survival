using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;


    Rigidbody2D rg;
    SpriteRenderer sp;
    Animator an;

    void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        an = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        an.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if(!GameManager.instance.isLive) return;
        
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rg.MovePosition(rg.position + nextVec);
    }

    void LateUpdate()
    {
        if(!GameManager.instance.isLive) return;

        an.SetFloat("Speed",inputVec.magnitude);
        if(inputVec.x != 0)
        {
            sp.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(!GameManager.instance.isLive) return;

        GameManager.instance.health -= Time.deltaTime * 10;

        if(GameManager.instance.health < 0)
        {
            for(int i=2;i<transform.childCount;i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            an.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }

    }
}
