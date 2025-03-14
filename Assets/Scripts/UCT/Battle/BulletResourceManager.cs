using System.Collections.Generic;
using UCT.Control;
using UnityEngine;

namespace UCT.Battle
{
    public static class BulletResourceManager
    {
        private static readonly Dictionary<string, BulletControl> BulletCache = new();

        public static BulletControl LoadBullet(string bulletPathName)
        {
            if (BulletCache.TryGetValue(bulletPathName, out var bullet))
            {
                return bullet;
            }

            var path = "Assets/Bullets/" + bulletPathName;
            bullet = Resources.Load<BulletControl>(path);

            if (bullet)
            {
                BulletCache[bulletPathName] = bullet;
            }
            else
            {
                Debug.LogError($"Bullet resource not found: {path}");
            }

            return bullet;
        }
    }
}