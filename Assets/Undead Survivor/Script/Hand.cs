using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isRight;
    public SpriteRenderer spriter;

    SpriteRenderer player;

    Vector3 leftPos = new Vector3(0.35f, -0.15f, 0);
    Vector3 leftPosReverse = new Vector3(-0.15f, -0.15f, 0);
    Quaternion rightRot = Quaternion.Euler(0,0,-35);
    Quaternion rightRotReverse = Quaternion.Euler(0,0,-135);

    void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    void LateUpdate()
    {
        bool isReverse = player.flipX;

        if(isRight)
        {
            transform.localRotation = isReverse ? rightRotReverse : rightRot;
            spriter.flipY = isReverse;
            spriter.sortingOrder = isReverse ? 4 : 6;
        }
        else{
            transform.localPosition = isReverse ? leftPosReverse : leftPos;
            spriter.flipX = isReverse;
            spriter.sortingOrder = isReverse ? 6 : 4;
        }
    }
}
