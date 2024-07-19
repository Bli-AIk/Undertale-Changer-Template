using UnityEngine;

/// <summary>
/// Monster Control Script
/// Mainly used for animation control and storage ATKDEF
/// </summary>
public class EnemiesController : MonoBehaviour
{
    public Animator anim;
    public int atk, def;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void AnimHit()
    {
        if (anim.GetBool("Hit"))
        {
            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipBattle);
            anim.SetBool("Hit", false);
        }
    }
}
