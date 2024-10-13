using DG.Tweening;
using UnityEngine;


public class CameraShake : MonoBehaviour
{
    private Tween tweenMove, tweenSpin;
    private Tween tweenMoveBack;

    public bool moveWithHeart;
    public Vector3 heartBasicPos = new Vector3(0, -1.5f);
    [Header("moveExtent的Y轴对应摄像机Z轴")]
    public Vector2 moveExtent = new Vector2(0.5f, 0.5f);

    private void Update()
    {
        if (moveWithHeart && !TurnController.instance.isMyTurn)
        {
            transform.position = new Vector3(moveExtent.x * (MainControl.instance.battlePlayerController.transform.position.x - heartBasicPos.x),
                                             0, 
                                             moveExtent.y * (MainControl.instance.battlePlayerController.transform.position.y - heartBasicPos.y));
        }
        if (moveWithHeart && tweenMoveBack == null && TurnController.instance.isMyTurn && transform.position != Vector3.zero)
        {
            tweenMoveBack = transform.DOMove(Vector3.zero, 0.5f).OnKill(KillTweenMoveBack);
        }
    }
    void KillTweenMoveBack()
    {
        tweenMoveBack = null;
    }
    /// <summary>
    /// 摄像机摇晃
    /// loops会自动转换为偶数。
    /// </summary>

    public void Shake(Vector3 v3move, Vector3 v3spin, int loops = 4, float shakeTime = 1f / 60f * 4f, string getSon = "", Ease easeMove = Ease.OutCubic, Ease easeSpin = Ease.InOutCubic)
    {
        Transform transformer;
        if (getSon == "")
            transformer = transform;
        else
            transformer = transform.Find(getSon);

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