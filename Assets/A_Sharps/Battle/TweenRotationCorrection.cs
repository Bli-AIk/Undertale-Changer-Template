using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 修复天杀的Tweeeeeeeeeeen
/// 让旋转必须从头来拿Tween.Do还用不了的脑梗问题
/// 顺带存一下Tween以便删
/// </summary>
public class TweenRotationCorrection : MonoBehaviour
{
    public Vector3 euler;
    public bool notLocal;
    public List<Tween> tweens = new List<Tween>();

    // Update is called once per frame
    void Update()
    {
        if(notLocal)
            transform.rotation = Quaternion.Euler(euler);
        else
            transform.localRotation = Quaternion.Euler(euler);
    }
    public void KillAllTween()
    {
        for (int i = 0; i < tweens.Count; i++)
        {
            tweens[i].Kill();
        }
    }
}
