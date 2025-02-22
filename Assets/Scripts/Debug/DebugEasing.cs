using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Debug
{
    public class DebugEasing : MonoBehaviour
    {
        [FormerlySerializedAs("vector2s")] public List<Vector2> vector2S;
        public float time;
        public float baseTime;
        public string enumStr;
        private LineRenderer _lineRenderer;
        private int _setNumber;
        private float _timer;

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            transform.localPosition = vector2S[0];
            transform.DOLocalMoveX(vector2S[1].x, time);
            transform.DOLocalMoveY(vector2S[1].y, time).SetEase((Ease)Enum.Parse(typeof(Ease), enumStr));
        }

        private void Update()
        {
            if (Time.time < time)
            {
                if (_timer <= 0)
                {
                    _timer = baseTime;

                    if (_lineRenderer.positionCount <= _setNumber)
                    {
                        _lineRenderer.positionCount++;
                    }

                    _lineRenderer.SetPosition(_setNumber, transform.position);
                    _setNumber++;
                }
                else
                {
                    _timer -= Time.deltaTime;
                }
            }
        }
    }
}