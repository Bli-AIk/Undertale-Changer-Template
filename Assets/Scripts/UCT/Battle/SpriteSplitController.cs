using System;
using System.Collections;
using System.Collections.Generic;
using UCT.Global.Audio;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Battle
{
    /// <summary>
    ///     内含Pool。
    ///     实现精灵的碎片化效果。
    ///     一行一行自上而下。
    ///     启用本脚本后立刻生效。
    /// </summary>
    public class SpriteSplitController : MonoBehaviour
    {
        public int poolCount;
        public List<Color> colorExclude;
        public Vector2 startPos;
        public float speed;
        private readonly Queue<GameObject> _available = new();
        private Texture2D _map;
        private GameObject _mask;
        public SpriteRenderer spriteRenderer { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            _map = ExtractSpriteTexture(spriteRenderer.sprite);
            _mask = transform.Find("Mask").gameObject;
        }

        private void OnEnable()
        {
            DisableAllChildrenExceptMask();
            spriteRenderer.color = Color.white;
            startPos = new Vector2(-_map.width / 2f * 0.05f, _map.height / 2f * 0.05f);
            if (_map.width % 2 == 0)
            {
                startPos += new Vector2(0.025f, 0);
            }

            if (_map.height % 2 == 0)
            {
                startPos -= new Vector2(0, 0.025f);
            }

            _mask.transform.localScale = new Vector2(_map.width, _map.height);
            _mask.transform.localPosition = new Vector3(0, 0.05f * _map.height);

            spriteRenderer.sprite = Sprite.Create(_map,
                new Rect(0, 0, _map.width, _map.height),
                new Vector2(0.5f, 0.5f),
                spriteRenderer.sprite.pixelsPerUnit);

            AudioController.Instance.PlayFx(5, MainControl.Instance.AudioControl.fxClipBattle);
            StartCoroutine(_SummonPixel());
        }

        private void DisableAllChildrenExceptMask()
        {
            foreach (Transform child in transform)
            {
                if (!string.Equals(child.name, "Mask", StringComparison.CurrentCultureIgnoreCase))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator _SummonPixel()
        {
            for (var y = _map.height - 1; y >= 0; y--)
            {
                for (var x = 0; x < _map.width; x++)
                {
                    var color = _map.GetPixel(x, y);
                    var skip = colorExclude.Contains(color);

                    if (skip)
                    {
                        continue;
                    }

                    var obj = GetFromPool();
                    var spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = color;

                    spriteRenderer.sortingOrder = this.spriteRenderer.sortingOrder + 1;

                    obj.transform.localPosition = startPos + new Vector2(x * 0.05f, -(_map.height - y - 1) * 0.05f);
                }

                _mask.transform.localPosition -= new Vector3(0, 0.05f);

                yield return new WaitForSeconds(speed);
            }
        }


        /// <summary>
        ///     从 Sprite 中提取正确的 Texture2D 部分（裁剪出 Sprite 对应的区域）。
        /// </summary>
        private static Texture2D ExtractSpriteTexture(Sprite sprite)
        {
            if (!sprite)
            {
                return null;
            }

            var readableTexture = MakeTextureReadable(sprite.texture);
            var rect = sprite.textureRect;

            var newTexture = new Texture2D((int)rect.width, (int)rect.height);
            newTexture.SetPixels(readableTexture.GetPixels(
                (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
            newTexture.filterMode = FilterMode.Point;
            newTexture.wrapMode = TextureWrapMode.Clamp;
            newTexture.Apply();
            return newTexture;
        }

        /// <summary>
        ///     复制一个可读写的 Texture2D。
        /// </summary>
        private static Texture2D MakeTextureReadable(Texture2D sourceTexture)
        {
            if (!sourceTexture)
            {
                return null;
            }

            var rt = RenderTexture.GetTemporary(
                sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(sourceTexture, rt);

            var previous = RenderTexture.active;
            RenderTexture.active = rt;

            var readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
            readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }

        //-----对象池部分-----

        /// <summary>
        ///     初始化/填充对象池
        /// </summary>
        private void FillPool()
        {
            for (var i = 0; i < poolCount; i++)
            {
                var newObj = Instantiate(Resources.Load<GameObject>("Template/Square Template"), transform);
                ReturnPool(newObj);
            }
        }

        /// <summary>
        ///     返回对象池
        /// </summary>
        public void ReturnPool(GameObject inputObject)
        {
            inputObject.SetActive(false);
            inputObject.transform.SetParent(transform);
            _available.Enqueue(inputObject);
        }

        /// <summary>
        ///     获取 square 对象
        /// </summary>
        private GameObject GetFromPool()
        {
            if (_available.Count == 0)
            {
                FillPool();
            }

            var square = _available.Dequeue();

            square.SetActive(true);
            return square;
        }
    }
}