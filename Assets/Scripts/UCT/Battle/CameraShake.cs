using DG.Tweening;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    public class CameraShake : MonoBehaviour
    {
        public bool moveWithHeart;
        public Vector3 heartBasicPos = new(0, -1.5f);

        [Header("moveExtent的Y轴对应摄像机Z轴")] public Vector2 moveExtent = new(0.5f, 0.5f);

        private Tween _tweenMove, _tweenSpin;
        private Tween _tweenMoveBack;

        private void Update()
        {
            if (moveWithHeart && !TurnController.Instance.isMyTurn)
                transform.position = new Vector3(
                    moveExtent.x * (MainControl.Instance.battlePlayerController.transform.position.x - heartBasicPos.x),
                    0,
                    moveExtent.y * (MainControl.Instance.battlePlayerController.transform.position.y -
                                    heartBasicPos.y));
            if (moveWithHeart && _tweenMoveBack == null && TurnController.Instance.isMyTurn &&
                transform.position != Vector3.zero)
                _tweenMoveBack = transform.DOMove(Vector3.zero, 0.5f).OnKill(KillTweenMoveBack);
        }

        private void KillTweenMoveBack()
        {
            _tweenMoveBack = null;
        }

        /// <summary>
        ///     摄像机摇晃
        ///     loops会自动转换为偶数。
        /// </summary>
        public void Shake(Vector3 v3Move, Vector3 v3Spin, int loops = 4, float shakeTime = 1f / 60f * 4f,
            string getSon = "", Ease easeMove = Ease.OutCubic, Ease easeSpin = Ease.InOutCubic)
        {
            Transform transformer;
            if (getSon == "")
                transformer = transform;
            else
                transformer = transform.Find(getSon);

            _tweenMove.Kill(true);
            _tweenSpin.Kill(true);

            if (loops % 2 != 0)
                loops++;
            if (v3Move != Vector3.zero)
                _tweenMove = transformer.DOLocalMove(transformer.localPosition + v3Move, shakeTime)
                    .SetLoops(loops, LoopType.Yoyo).SetEase(easeMove);

            if (v3Spin != Vector3.zero)
                _tweenSpin = transformer.DOLocalRotate(transformer.localRotation.eulerAngles + v3Spin, shakeTime)
                    .SetLoops(loops, LoopType.Yoyo).SetEase(easeSpin);
        }
    }
}