using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CameraShake : MonoBehaviour
{
    Tween tween;
    public enum ShakeMode
    {
        random,
        up,
        down,
        left,
        right,
    }

    private void Update()
    {
        if (MainControl.instance.OverwroldControl.isDebug && Input.GetKeyDown(KeyCode.G))
        {
            ShakeMode shakeMode = (ShakeMode)Random.Range(0, 5);
            //Debug.Log(shakeMode);
            Shake(shakeMode);

        }
    }

    public float shakeTime = 0.05f;
    public float shakeDis = 0.2f;
    /// <summary>
    /// 摄像机摇晃
    /// loops会自动转换为偶数。
    /// </summary>
    public void Shake(ShakeMode shakeMode, int loops = 6)
    {
        tween.Kill();
        transform.position = new Vector3(0, 0, -10);

        if (loops % 2 != 0)
            loops++;

        switch (shakeMode)
        {
            case ShakeMode.random:
                tween = transform.DOMove(new Vector3(MainControl.instance.Get1Or_1() * shakeDis, MainControl.instance.Get1Or_1() * shakeDis, -10), shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
                break;
            case ShakeMode.up:
                tween = transform.DOMoveY(1 * shakeDis, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
                break;
            case ShakeMode.down:
                tween = transform.DOMoveY(-1 * shakeDis, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
                break;
            case ShakeMode.left:
                tween = transform.DOMoveX(-1 * shakeDis, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
                break;
            case ShakeMode.right:
                tween = transform.DOMoveX(-1 * shakeDis, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
                break;
        }
    }
}
