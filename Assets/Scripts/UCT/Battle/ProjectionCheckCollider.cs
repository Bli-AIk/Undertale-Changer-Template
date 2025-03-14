using System.Collections.Generic;
using UCT.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     投影原先心的判定。
    ///     如果你直接把弹幕放在原先心的位置（负一千），可能会导致显示问题。
    /// </summary>
    public class ProjectionCheckCollider : ObjectPool
    {
        private readonly List<GameObject> _checkColliders = new();

        private readonly List<GameObject> _sets = new();
        private GameObject _projectionBoxes;

        // Start is called before the first frame update
        private void Start()
        {
            _projectionBoxes = MainControl.Instance.selectUIController.projectionBoxes;

            poolObject = (GameObject)Resources.Load("Prefabs/CheckCollider");
            for (var i = 0; i < _projectionBoxes.transform.childCount; i++)
            {
                _sets.Add(_projectionBoxes.transform.GetChild(i).gameObject);
                var obj = GetFromPool<Transform>();
                _checkColliders.Add(obj.gameObject);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (!MainControl.Instance.battlePlayerController)
            {
                return;
            }

            var relative = (Vector2)MainControl.Instance.battlePlayerController.transform.position -
                           MainControl.Instance.battlePlayerController.sceneDrift;
            for (var i = 0; i < _sets.Count; i++)
            {
                var convert = _sets[i].transform.position + _sets[i].transform.rotation * relative;
                _checkColliders[i].transform.position = convert - _projectionBoxes.transform.position;
                _checkColliders[i].transform.rotation = _sets[i].transform.rotation;
            }
        }
    }
}