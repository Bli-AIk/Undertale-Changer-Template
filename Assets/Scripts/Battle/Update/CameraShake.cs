using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CameraShake : MonoBehaviour
{
    Tween tween;

    private void Update()
    {
        if (MainControl.instance.OverwroldControl.isDebug && Input.GetKeyDown(KeyCode.G))
        {
            Shake();

        }
    }

    /// <summary>
    /// 摄像机摇晃
    /// loops会自动转换为偶数。
    /// </summary>
    public void Shake(int loops = 4, float shakeTime = 1f / 60f * 4f)
    {
        tween.Kill();
        transform.position = new Vector3(0, 0, transform.position.z);

        if (loops % 2 != 0)
            loops++;

        tween = transform.DOMove(transform.position + new Vector3(Random.Range(0.025f, 0.05f) * MainControl.instance.Get1Or_1(), Random.Range(0.025f, 0.05f) * MainControl.instance.Get1Or_1()), shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
             
    }

    public void Shake(Vector3 v3, int loops = 4, float shakeTime = 1f / 60f * 4f)
    {
        tween.Kill();
        transform.position = new Vector3(0, 0, transform.position.z);

        if (loops % 2 != 0)
            loops++;

        tween = transform.DOMove(transform.position + v3, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.Linear);

    }
}
