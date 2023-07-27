using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ƹ����...�����˽ű�)
/// ��Ҫ���ڶ������ƺʹ洢ATKDEF
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
