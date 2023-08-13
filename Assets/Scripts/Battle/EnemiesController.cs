using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Monster Control Script
/// Mainly used for animation control, storing ATK, DEF
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
