using DG.Tweening;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    private Tween tweenMove, tweenSpin;

    private void Update()
    {
        if (MainControl.instance.PlayerControl.isDebug && Input.GetKeyDown(KeyCode.G))
        {
            Shake();
        }
    }

    /// <summary>
    /// �����ҡ��
    /// loops���Զ�ת��Ϊż����
    /// </summary>
    public void Shake(int loops = 4, float shakeTime = 1f / 60f * 4f,  float spinMax = 0, bool isSpin3D = false)
    {
        tweenMove.Kill();
        transform.position = new Vector3(0, 0, transform.position.z);

        if (loops % 2 != 0)
            loops++;

        tweenMove = transform.DOMove(transform.position + new Vector3(Random.Range(0.025f, 0.05f) * MainControl.instance.Get1Or_1(), Random.Range(0.025f, 0.05f) * MainControl.instance.Get1Or_1()), shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCirc);
        if (spinMax != 0)
        {
            tweenSpin = transform.DORotate(transform.rotation.eulerAngles + new Vector3(Random.Range(-spinMax, spinMax) * Convert.ToInt32(isSpin3D), Random.Range(-spinMax, spinMax) * Convert.ToInt32(isSpin3D), Random.Range(-spinMax, spinMax)), shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCubic);
        }
    }

    public void Shake(Vector3 v3, int loops = 4, float shakeTime = 1f / 60f * 4f, float spinMax = 0, bool isSpin3D = false)
    {
        tweenMove.Kill();
        transform.position = new Vector3(0, 0, transform.position.z);

        if (loops % 2 != 0)
            loops++;

        tweenMove = transform.DOMove(transform.position + v3, shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.Linear);

        if (spinMax != 0)
        {
            tweenSpin = transform.DORotate(transform.rotation.eulerAngles + new Vector3(Random.Range(-spinMax, spinMax) * Convert.ToInt32(isSpin3D), Random.Range(-spinMax, spinMax) * Convert.ToInt32(isSpin3D), Random.Range(-spinMax, spinMax)), shakeTime).SetLoops(loops, LoopType.Yoyo).SetEase(Ease.InOutCubic);
        }
    }
}