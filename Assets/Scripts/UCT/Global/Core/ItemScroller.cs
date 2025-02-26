using System;
using System.Collections.Generic;
using DG.Tweening;
using UCT.Extensions;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.Core
{
    /// <summary>
    ///     物品选择控制器
    /// </summary>
    public class ItemScroller : MonoBehaviour
    {
        private int _itemCount;
        private int _currentSelectionIndex;
        private int _highlightedIndex;
        private List<SpriteRenderer> _allScrollPoints;
        private Tween _closeTween;
        private Tween _currentHighlightTween;
        private List<GameObject> _currentScrollPoints;
        private float _originalLocalXValue;

        private void Awake()
        {
            _allScrollPoints = new List<SpriteRenderer>();
            for (var i = 0; i < transform.childCount; i++)
            {
                _allScrollPoints.Add(transform.GetChild(i).GetComponent<SpriteRenderer>());
            }

            _currentScrollPoints = new List<GameObject>();
            for (var i = 0; i < 7; i++)
            {
                _currentScrollPoints.Add(transform.Find($"Point{i}").gameObject);
            }
            SetAllPointActive(false);

            _originalLocalXValue = transform.localPosition.x;
        }

        private void Update()
        {
            switch (_highlightedIndex)
            {
                case > 0 when _allScrollPoints[0].color.a == 0:
                    DOTween.To(() => _allScrollPoints[0].color, x => _allScrollPoints[0].color = x, Color.white, 0.15f)
                        .SetEase(Ease.Linear);
                    break;
                case 0 when _allScrollPoints[0].color.a > 0:
                    DOTween.To(() => _allScrollPoints[0].color, x => _allScrollPoints[0].color = x, ColorEx.WhiteClear,
                            0.1f)
                        .SetEase(Ease.Linear);
                    break;
                default:
                    break;
            }

            if (_highlightedIndex < _itemCount - 1 && _allScrollPoints[1].color.a == 0)
            {
                DOTween.To(() => _allScrollPoints[1].color, x => _allScrollPoints[1].color = x, Color.white, 0.15f)
                    .SetEase(Ease.Linear);
            }
            else if (_highlightedIndex == _itemCount - 1 && _allScrollPoints[1].color.a > 0)
            {
                DOTween.To(() => _allScrollPoints[1].color, x => _allScrollPoints[1].color = x, ColorEx.WhiteClear,
                        0.1f)
                    .SetEase(Ease.Linear);
            }
        }

        public void Open(int itemCount, float tweenXValue)
        {
            _closeTween.Kill();
            SetAllPointActive(true);
            _itemCount = itemCount;

            for (var i = 2; i < _allScrollPoints.Count; i++)
            {
                _allScrollPoints[i].gameObject.SetActive(i <= _itemCount + 1);
            }

            if (_allScrollPoints[_itemCount + 2].transform.localPosition.y > -1.725)
            {
                _allScrollPoints[0].transform.localPosition = _allScrollPoints[_itemCount + 2].transform.localPosition;
                _allScrollPoints[1].transform.localPosition = _allScrollPoints[_itemCount + 3].transform.localPosition;
            }
            else
            {
                _allScrollPoints[0].transform.localPosition = _allScrollPoints[_itemCount + 3].transform.localPosition;
                _allScrollPoints[1].transform.localPosition = _allScrollPoints[_itemCount + 2].transform.localPosition;
            }

            transform.DOLocalMoveX(tweenXValue, 0.25f).SetEase(Ease.OutCirc);
            _allScrollPoints[0].transform.DOLocalMoveY(_allScrollPoints[0].transform.localPosition.y + 0.05f, 0.75f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
            _allScrollPoints[1].transform.DOLocalMoveY(_allScrollPoints[1].transform.localPosition.y - 0.05f, 0.75f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
            PressUp();
        }

        private void SetAllPointActive(bool value)
        {
            foreach (var point in _allScrollPoints)
            {
                point.gameObject.SetActive(value);
            }
        }

        private void HighlightCurrentPoint()
        {
            _currentHighlightTween.Kill(true);
            _currentScrollPoints[_currentSelectionIndex].transform.localScale = Vector3.one * 2;
            _currentHighlightTween = DOTween
                .To(() => _currentScrollPoints[_currentSelectionIndex].transform.localScale,
                    x => _currentScrollPoints[_currentSelectionIndex].transform.localScale = x, Vector3.one * 3, 0.5f)
                .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void PressDown()
        {
            if (_currentSelectionIndex > 0)
            {
                _currentScrollPoints[_currentSelectionIndex - 1].transform.localScale = Vector3.one * 2;
            }
            HighlightCurrentPoint();
        }

        private void PressUp()
        {
            if (_currentSelectionIndex < _itemCount - 1)
            {
                _currentScrollPoints[_currentSelectionIndex + 1].transform.localScale = Vector3.one * 2;
            }
            HighlightCurrentPoint();
        }


        public void Close()
        {
            _closeTween = transform.DOLocalMoveX(_originalLocalXValue, 0.25f).SetEase(Ease.InCirc).OnKill(SetNoActive);
        }

        private void SetNoActive()
        {
            _allScrollPoints[0].color = Color.clear;
            _allScrollPoints[1].color = Color.clear;
            foreach (var t in _allScrollPoints)
            {
                t.transform.DOKill();
            }

            SetAllPointActive(false);
        }

        /// <summary>
        ///     处理类似物品选择控制器的计算
        /// </summary>
        public void UpdateHandleItemInput(
            ref int globalItemIndex,
            ref int visibleItemIndex,
            int count,
            Action<int> onKeyDown)
        {
            _highlightedIndex = globalItemIndex;
            CalculateAndSetCurrentSelectionIndex(globalItemIndex, count);
            
            if (InputService.GetKeyDown(KeyCode.UpArrow) && globalItemIndex > 0)
            {
                if (visibleItemIndex > 0)
                {
                    visibleItemIndex--;
                }
                PressUp();
                globalItemIndex--;
                onKeyDown.Invoke(globalItemIndex);
            }
            else if (InputService.GetKeyDown(KeyCode.DownArrow) && globalItemIndex < count - 1)
            {
                if (visibleItemIndex < 2)
                {
                    visibleItemIndex++;
                }
                PressDown();
                globalItemIndex++;
                onKeyDown.Invoke(globalItemIndex);
            }
        }


        private void CalculateAndSetCurrentSelectionIndex(int globalItemIndex, int count)
        {
            if (count >= 8)
            {
                _currentSelectionIndex = globalItemIndex;
            }
            else
            {
                var number = count switch
                {
                    >= 6 => 8,
                    >= 4 => 7,
                    >= 2 => 6,
                    >= 1 => 5,
                    _ => 8
                };
                _currentSelectionIndex = count % 2 == 0
                    ? globalItemIndex + (number - 1 - count)
                    : globalItemIndex + (number - count);
            }
        }
    }
}