using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ƽű�
/// ��Ҫ���ڶ������ƺʹ洢ATKDEF
/// </summary>
public class EnemiesController : MonoBehaviour
{
    public Animator anim;
    public int atk, def;
    
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void AnimHit()
    {
        if (anim.GetBool("Hit"))
        {
            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipBattle);
            anim.SetBool("Hit", false);
        }
        
    }
}
