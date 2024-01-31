using DG.Tweening;
using UnityEngine;

public class DebugDoTween : MonoBehaviour
{
    private void Start()
    {
        transform.DOMove(transform.position + new Vector3(10, 0, 0), 1, false)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /*
     void DoTweenVoid()
    {
        transform.DOMove(transform.position + new Vector3(0, 2.5f, 0), 0.5f, false).SetEase(Ease.InSine).SetLoops(-1, LoopType.Yoyo);
    }
    */
}