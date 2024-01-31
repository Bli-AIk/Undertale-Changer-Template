using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugEasing : MonoBehaviour
{
    public List<Vector2> vector2s;
    public float time;
    public float baseTime;
    private float timer;
    public string enumStr;
    private LineRenderer lineRenderer;
    private int setNum;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        transform.localPosition = vector2s[0];
        transform.DOLocalMoveX(vector2s[1].x, time);
        transform.DOLocalMoveY(vector2s[1].y, time).SetEase((Ease)Enum.Parse(typeof(Ease), enumStr));
    }

    private void Update()
    {
        if (Time.time < time)
        {
            if (timer <= 0)
            {
                timer = baseTime;

                if (lineRenderer.positionCount <= setNum)
                    lineRenderer.positionCount++;

                lineRenderer.SetPosition(setNum, transform.position);
                setNum++;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
    }
}