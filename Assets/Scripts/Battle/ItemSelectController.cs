using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 字面意思
/// </summary>
public class ItemSelectController : MonoBehaviour
{
    public List<SpriteRenderer> sons;
    public List<GameObject> sonsChanged;

    public int myItemMax;
    public int myItemSelect;
    public int myItemRealSelect;
    private Tween tweenSave;

    //SelectUIController selectUIController;
    private void Awake()
    {
        //selectUIController = transform.parent.GetComponent<SelectUIController>();
        for (int i = 0; i < transform.childCount; i++)
        {
            sons.Add(transform.GetChild(i).GetComponent<SpriteRenderer>());
        }
    }

    public void Open()
    {
        for (int i = 2; i < sons.Count; i++)
        {
            sons[i].gameObject.SetActive(i <= myItemMax + 1);
        }
        if (sons[myItemMax + 2].transform.localPosition.y > -1.725)//上
        {
            sons[0].transform.localPosition = sons[myItemMax + 2].transform.localPosition;
            sons[1].transform.localPosition = sons[myItemMax + 3].transform.localPosition;
        }
        else
        {
            sons[0].transform.localPosition = sons[myItemMax + 3].transform.localPosition;
            sons[1].transform.localPosition = sons[myItemMax + 2].transform.localPosition;
        }
        transform.DOLocalMoveX(0, 0.25f).SetEase(Ease.OutCirc);
        sons[0].transform.DOLocalMoveY(sons[0].transform.localPosition.y + 0.05f, 0.75f, false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
        sons[1].transform.DOLocalMoveY(sons[1].transform.localPosition.y - 0.05f, 0.75f, false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
        PressDown(true);
    }

    private void Update()
    {
        if (myItemRealSelect > 0 && sons[0].color.a == 0)
        {
            DOTween.To(() => sons[0].color, x => sons[0].color = x, new Color(1, 1, 1, 1), 0.15f).SetEase(Ease.Linear);
        }
        else if (myItemRealSelect == 0 && sons[0].color.a > 0)
        {
            DOTween.To(() => sons[0].color, x => sons[0].color = x, new Color(1, 1, 1, 0), 0.1f).SetEase(Ease.Linear);
        }

        if (myItemRealSelect < myItemMax - 1 && sons[1].color.a == 0)
        {
            DOTween.To(() => sons[1].color, x => sons[1].color = x, new Color(1, 1, 1, 1), 0.15f).SetEase(Ease.Linear);
        }
        else if (myItemRealSelect == myItemMax - 1 && sons[1].color.a > 0)
        {
            DOTween.To(() => sons[1].color, x => sons[1].color = x, new Color(1, 1, 1, 0), 0.1f).SetEase(Ease.Linear);
        }
    }

    public void PressDown(bool isUp)
    {
        tweenSave.Kill(true);
        if (!isUp && myItemSelect > 0)
            sonsChanged[myItemSelect - 1].transform.localScale = Vector3.one * 2;
        else if (myItemSelect < myItemMax - 1)
            sonsChanged[myItemSelect + 1].transform.localScale = Vector3.one * 2;
        sonsChanged[myItemSelect].transform.localScale = Vector3.one * 2;
        tweenSave = DOTween.To(() => sonsChanged[myItemSelect].transform.localScale, x => sonsChanged[myItemSelect].transform.localScale = x, Vector3.one * 3, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public void Close()
    {
        transform.DOLocalMoveX(0.65f, 0.25f).SetEase(Ease.InCirc).OnKill(SetNoActive);
    }

    public void SetNoActive()
    {
        sons[0].color = Color.clear;
        sons[1].color = Color.clear;
        for (int i = 0; i < sons.Count; i++)
        {
            sons[i].transform.DOKill();
        }
    }
}