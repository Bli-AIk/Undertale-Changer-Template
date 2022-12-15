using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// �޸���ɱ��Tweeeeeeeeeeen
/// ����ת�����ͷ����Tween.Do���ò��˵��Թ�����
/// ˳����һ��Tween�Ա�ɾ
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
