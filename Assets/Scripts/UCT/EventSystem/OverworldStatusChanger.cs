using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.EventSystem
{
    /// <summary>
    ///     OW物体状态转换器，用于根据fact的变化更改物体的状态。
    /// </summary>
    [Serializable]
    public class OverworldStatusChanger : MonoBehaviour
    {
        public enum MethodType
        {
            ChangeSprite,
            SetColliderEnable
        }
        
        public List<RuleCriterion> ruleCriteria;
        
        public List<MethodType> methodTypes;
        public List<Sprite> targetSprites;
        public List<bool> targetEnables;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            var maxCount = Mathf.Max(ruleCriteria.Count, methodTypes.Count, targetSprites.Count, targetEnables.Count);
            for (var i = 0; i < maxCount; i++)
            {
                if (i >= ruleCriteria.Count)
                {
                    ruleCriteria.Add(default);
                }

                if (i >= methodTypes.Count)
                {
                    methodTypes.Add(default);
                }

                if (i >= targetSprites.Count)
                {
                    targetSprites.Add(null);
                }

                if (i >= targetEnables.Count)
                {
                    targetEnables.Add(false);
                }

                var ruleCriterion = ruleCriteria[i];
                var methodType = methodTypes[i];
                var targetSprite = targetSprites[i];
                var targetEnabled = targetEnables[i];

                if (ruleCriterion.GetResult())
                {
                    switch (methodType)
                    {
                        case MethodType.ChangeSprite:
                            if (!_spriteRenderer)
                            {
                                break;
                            }

                            _spriteRenderer.sprite = targetSprite;
                            break;
                        case MethodType.SetColliderEnable:
                            SetCollidersEnabled(gameObject, targetEnabled);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public static void SetCollidersEnabled(GameObject obj, bool input)
        {
            var colliders = obj.GetComponents<Collider2D>();
            foreach (var item in colliders)
            {
                item.enabled = input;
            }
        }
    }
}