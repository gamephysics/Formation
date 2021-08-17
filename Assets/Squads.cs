using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//====================================================================
// Class: Squads
// Desc : Spawn Units from (public int unitCount = 4;)
//====================================================================
public class Squads : MonoBehaviour
{
    public int unitCount = 4;
    // UNIT
    [ReadOnly]
    public List<UnitMove> units = new List<UnitMove>();

    // Start is called before the first frame update
    void Awake()
    {
        units.Clear();

        var obj = Resources.Load<GameObject>("Prefabs/Unit");

        for (int i = 0; i < unitCount; ++i)
        {
            var unit = GameObject.Instantiate(obj);
            unit.transform.parent = this.transform;

            // random position
            unit.transform.position = new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10));

            units.Add(unit.GetComponent<UnitMove>());
        }
    }
}
