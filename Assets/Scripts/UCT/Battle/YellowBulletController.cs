using System;
using UnityEngine;

namespace UCT.Battle
{
    public class YellowBulletController : MonoBehaviour
    {
        public Action OnKill;

        private void Update()
        {
            transform.position += Vector3.up * (Time.deltaTime * 5);
            if (transform.position.y > 100)
            {
                OnKill?.Invoke();
            }
        }
    }
}