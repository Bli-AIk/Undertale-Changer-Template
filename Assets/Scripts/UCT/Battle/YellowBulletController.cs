using System;
using UnityEngine;

namespace UCT.Battle
{
    public class YellowBulletController : MonoBehaviour
    {
        public Action OnKill;
        private const float Speed = 12.5f;

        private void Update()
        {
            transform.position += Vector3.up * (Time.deltaTime * Speed);
            if (transform.position.y > 100)
            {
                OnKill?.Invoke();
            }
        }
    }
}