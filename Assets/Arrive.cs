using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//====================================================================
// Class: Arrive 
// Desc : set position dest gameObjects & Order Unit Move to destination
//====================================================================
public class Arrive : MonoBehaviour
{
    public Squads   squads = null;

    public int      columns = 2;
    public float    spacing = 4;
    [ReadOnly]
    public List<GameObject> dests = new List<GameObject>();

    private Vector3 center_of_dest = Vector3.zero;

    //======================================
    // Destination GameObject Initial Position
    //======================================
    void Start()
    {
        if (squads != null && squads.units != null)
        {
            dests.Clear();

            var obj = Resources.Load<GameObject>("Prefabs/Dest");

            for (int i = 0; i < squads.units.Count; ++i)
            {
                var dest = GameObject.Instantiate(obj);
                dest.transform.parent   = this.transform;
                dest.transform.position = Vector3.zero;
                dests.Add(dest);
            }

            // (0, 0, spacing) 에 생성된 dest gameObject들을 위치시킨다.
            Formation(new Vector3(0, 0, spacing));
        }
    }

    //======================================
    // Mouse Click & Set Formation Position & Move Unit
    //======================================
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                Formation(hit.point);
                MoveSquad();
            }
        }
    }

    //======================================
    // center_of_dest -> dest 방향의 Formation 으로 
    // dest GameObject 들의 위치를 설정합니다.
    //======================================
    void Formation(Vector3 dest)
    {
        // Center of Destination GameObjects 
        if (dests != null && dests.Count > 0)
        {
            center_of_dest = Vector3.zero;
            foreach (var d in dests)
            {
                center_of_dest += d.transform.position;
            }
            center_of_dest /= dests.Count;
        }

        // Only 2D 
        dest.y              = 0;
        center_of_dest.y    = 0;

        // zero distance
        Vector3 delta = (dest - center_of_dest);
        if (delta.sqrMagnitude <= 0)
            return;

        // center_of_dest -> dest formation 
        Vector3 forwd  = delta.normalized;
        Quaternion rot = Quaternion.LookRotation(forwd);

        for (int i = 0; i < dests.Count; ++i)
        {
            int     ROW     = i / columns;
            int     COL     = i % columns;
            float   SIGN    = (COL % 2 == 0) ? -1f : 1f;
            float   offset  = (Mathf.Min(columns, dests.Count - ROW * columns) % 2) == 0 ? -0.5f : 0f;

            float Z = (float)(-ROW);
            float Y = 0f;
            float X = (float)((COL + 1) / 2) * SIGN + offset;

            Vector3 Relative = new Vector3(X, Y, Z) * spacing;
            Vector3 pos = rot * Relative + dest;

            dests[i].transform.position = pos;
        }
    }


    //======================================
    // Matching Destination & Move to Matched Position
    //======================================
    void MoveSquad()
    {
        if (squads.units.Count != dests.Count)
            return;

        //==========================================================================
        // CALCAULATE PREFER FROM DISTANCE BETWEEN POSITIONS
        //==========================================================================
        // SET CURRENT POSITION
        Vector3[] UnitPos = new Vector3[squads.units.Count];
        for (int i = 0; i < squads.units.Count; ++i)
        {
            UnitPos[i] = squads.units[i].transform.position;
        }

        Vector3[] DestPos = new Vector3[squads.units.Count];
        for (int i = 0; i < squads.units.Count; ++i)
        {
            DestPos[i] = dests[i].transform.position;
        }

        //======================================
        // UNIT : 0,1,2,3 
        // DEST : 4,5,6,7
        //======================================
        // ###### SET VALUE FROM DISTANCE BETWEEN POSITIONS ######
        //======================================
        var Unit_Dists = new List<Tuple<int, float>>[UnitPos.Length];
        for (int i = 0; i < UnitPos.Length; ++i)
        {
            Unit_Dists[i] = new List<Tuple<int, float>>();

            for (int j = 0; j < DestPos.Length; ++j)
            {
                // Value 를 어떻게 설정하느냐에따라 위치가 선택된다.
                float dist = Vector3.Distance(UnitPos[i], DestPos[j % columns]);
                // J : DEST (4,5,6,7)
                Unit_Dists[i].Add(new Tuple<int, float>(j + UnitPos.Length, dist)); 
            }
        }

        var Format_Dists = new List<Tuple<int, float>>[DestPos.Length];
        for (int i = 0; i < DestPos.Length; ++i)
        {
            Format_Dists[i] = new List<Tuple<int, float>>();

            for (int j = 0; j < UnitPos.Length; ++j)
            {
                // Value 를 어떻게 설정하느냐에따라 위치가 선택된다.
                float dist = Vector3.Distance(UnitPos[j], DestPos[i % columns]);
                // J : UNIT (0,1,2,3)
                Format_Dists[i].Add(new Tuple<int, float>(j, dist));
            }
        }

        // SORT VALUES FOR PERPER ORDER
        for (int i = 0; i < UnitPos.Length; ++i)
        {
            Unit_Dists[i].Sort(delegate (Tuple<int, float> x, Tuple<int, float> y)
            {
                return x.Item2.CompareTo(y.Item2);
            });
        }

        for (int i = 0; i < DestPos.Length; ++i)
        {
            Format_Dists[i].Sort(delegate (Tuple<int, float> x, Tuple<int, float> y)
            {
                return x.Item2.CompareTo(y.Item2);
            });
        }


        //==========================================================================
        // SET DATA
        //==========================================================================
        int[,] prefer = new int[UnitPos.Length + DestPos.Length, UnitPos.Length];
        //==========================================================================
        //int[,] prefer = new int[,]{
        //  {7, 5, 6, 4},    // UNIT 0:  from 0
        //  {5, 4, 6, 7},    // UNIT 1:
        //  {4, 5, 6, 7},    // UNIT 2:
        //  {4, 5, 6, 7},    // UNIT 3:
        //  {X, X, X, X},    // UNIT N-1:
        //
        //  {0, 1, 2, 3},    // DEST 4:  from N
        //  {0, 1, 2, 3},    // DEST 5:  
        //  {0, 1, 2, 3},    // DEST 6:  
        //  {0, 1, 2, 3},    // DEST 7:  
        //  {X, X, X, X},    // DEST 2N-1:
        //};
        //==========================================================================
        for (int i = 0; i < UnitPos.Length; ++i)
        {
            for (int j = 0; j < UnitPos.Length; ++j)
            {
                //from 0
                prefer[i, j] = Unit_Dists[i][j].Item1;
            }
        }
        for (int i = 0; i < UnitPos.Length; ++i)
        {
            for (int j = 0; j < UnitPos.Length; ++j)
            {
                //from N
                prefer[i + UnitPos.Length, j] = Format_Dists[i][j].Item1; 
            }
        }

        //==========================================================================
        // SOLVE DATA
        //==========================================================================
        var wPartner = GaleShapley.stableMarriage(prefer);

        //==========================================================================
        // MOVE TO TARGET 
        //==========================================================================
        Debug.Log("DEST UNIT");
        for (int i = 0; i < GaleShapley.N; i++)
        {
            int UnitID = wPartner[i];
            int DestID = i;

            squads.units[UnitID].SetTarget(DestPos[i]);
        }

    }

}
