using System.Collections.Generic;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// 投影原先心的判定。
    /// 如果你直接把弹幕放在原先心的位置（负一千），可能会导致显示问题。
    /// </summary>
    public class ProjectionCheckCollider : ObjectPool
    {
        GameObject _canvasBoxProjectionSet;
        List<GameObject> _sets = new List<GameObject>();
        List<GameObject> _checkColliders = new List<GameObject>();
        // Start is called before the first frame update
        void Start()
        {
            _canvasBoxProjectionSet = GameObject.Find("CanvasBoxProjectionSet");

            obj = (GameObject)Resources.Load("Prefabs/CheckCollider");
            for (int i = 0; i < _canvasBoxProjectionSet.transform.childCount; i++)
            {
                _sets.Add(_canvasBoxProjectionSet.transform.GetChild(i).gameObject);
                _checkColliders.Add(GetFromPool());
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 relative = (Vector2)MainControl.Instance.battlePlayerController.transform.position - MainControl.Instance.battlePlayerController.sceneDrift;
            for (int i = 0; i < _sets.Count; i++)
            {
                Vector3 convert = _sets[i].transform.position + _sets[i].transform.rotation * relative;
                _checkColliders[i].transform.position = (convert - _canvasBoxProjectionSet.transform.position);
                _checkColliders[i].transform.rotation = _sets[i].transform.rotation;
            }
        }
    }
}
