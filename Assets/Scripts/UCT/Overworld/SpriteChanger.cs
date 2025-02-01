using System.Collections.Generic;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UCT.Overworld
{
    /// <summary>
    ///     Overworld对话中更改Sprite
    /// </summary>
    public class SpriteChanger : MonoBehaviour
    {
        public bool useImage;
        public bool haveBack;
        public List<Sprite> sprites, spritesSayBack;
        public string path;
        public float backFrame;
        public bool inOverWorld;

        public bool justSaying;
        private bool _back;
        private float _clock;
        private int _number;
        private SpriteRenderer _sprite;
        private Image _spriteImage;
        private TalkBoxController _talkBoxController;

        private TypeWritter _typeWritter;

        private void Start()
        {
            _clock = backFrame / 60f;
            if (inOverWorld)
                _talkBoxController = TalkBoxController.Instance;
            if (haveBack)
                _typeWritter = GetComponent<TypeWritter>();

            if (path != "")
            {
                if (!useImage)
                    _sprite = transform.Find(path).GetComponent<SpriteRenderer>();
                else
                    _spriteImage = transform.Find(path).GetComponent<Image>();
            }
            else
            {
                if (!useImage)
                    _sprite = GetComponent<SpriteRenderer>();
                else
                    _spriteImage = GetComponent<Image>();
            }
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting) return;
            
            if (_clock > 0) _clock -= Time.deltaTime;
            if (justSaying || (haveBack && _typeWritter.isTyping && !_typeWritter.passText && !_typeWritter.isStop))
            {
                if (!(_clock <= 0) || _number < 0) return;
                _back = !_back;
                ChangeImage(_number, _back);
                _clock = backFrame / 60f;
            }
            else if (_clock <= 0)
            {
                if (_back)
                {
                    _back = false;
                    ChangeImage(_number, _back);
                }

                _clock = backFrame / 60f;
            }
        }

        public void ChangeImage(int i, bool back = false)
        {
            _talkBoxController.haveHead = i >= 0;
            if (i >= 0)
            {
                var sprite = !back ? sprites[i] : spritesSayBack[i];
                if (!useImage)
                {
                    _sprite.sprite = sprite;
                    _sprite.color = Color.white;
                }
                else
                {
                    _spriteImage.sprite = sprite;
                    _spriteImage.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
                    _spriteImage.color = Color.white;
                }
            }
            else
            {
                if (!useImage)
                {
                    _sprite.sprite = null;
                    _sprite.color = Color.clear;
                }
                else
                {
                    _spriteImage.sprite = null;
                    _spriteImage.color = Color.clear;
                }
            }

            _number = i;
        }
    }
}