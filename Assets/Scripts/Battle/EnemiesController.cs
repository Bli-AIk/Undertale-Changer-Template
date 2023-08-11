using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物控制脚本
/// 主要用于动画控制和存储ATKDEF
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
