using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制怪物的...工具人脚本)
/// 主要用于动画控制和存储ATKDEF
/// </summary>
public class EnemiesController : MonoBehaviour
{
    public Animator anim;
    public int atk, def;
    // Start is called before the first frame update
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
