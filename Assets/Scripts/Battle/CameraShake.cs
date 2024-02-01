using DG.Tweening;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    private Tween tweenMove, tweenSpin;

    /// <summary>
    /// 摄像机摇晃
    /// loops会自动转换为偶数。
    /// </summary>

    public void Shake(Vector3 v3move, Vector3 v3spin, int loops = 4, float shakeTime = 1f / 60f * 4f, string getSon = "", Ease easeMove = Ease.Linear, Ease easeSpin = Ease.InOutCubic)
    {
        Transform transformer;
        if (getSon == "")
            transformer = transform;
        else
            transformer = transform.Find(getSon);

        Debug.Log(transformer, transformer);

        tweenMove.Kill(true);
        tweenSpin.Kill(true);

        if (loops % 2 != 0)
            loops++;
        if (v3move != Vector3.zero)
        {
            tweenMove = transformer.DOLocalMove(transformer.localPosition + v3move, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(easeMove);
        }

        if (v3spin != Vector3.zero)
        {
            tweenSpin = transformer.DOLocalRotate(transformer.localRotation.eulerAngles + v3spin, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(easeSpin);
        }

    }
}