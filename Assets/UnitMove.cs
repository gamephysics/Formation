using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//====================================================================
// Class: UnitMove 
// Desc : Just Move Unit to target
//====================================================================
public class UnitMove : MonoBehaviour
{
    static  int     MAXID = 0;
    // WAYPOINT
    public  List<Vector3> target = new List<Vector3>();
    // MOVE SPEED
    const   float   speed = 10.0f;
    
    private int     Id = MAXID++;
    // VARIABLE
    private double  start = 0;
    private int     index = 0;
    private Vector3 targetPos;
    private Vector3 targetDir;

    // Start is called before the first frame update
    void Start()
    {
        //======================================
        // PREPRE START 
        //======================================
        index = 0;
        if (target != null && target.Count > index)
        {
            transform.position = target[index];
        }

        //======================================
        // PREPRE NEXT
        //======================================
        ++index;
        if (target != null && target.Count > index)
        {
            // SET TARGET POSITION, TARGET DIRECTION
            targetPos = target[index];
            targetDir = Vector3.Normalize(targetPos - transform.position);
        }

        //======================================
        // START TIME
        //======================================
        start = Time.timeSinceLevelLoadAsDouble;
        //Debug.LogFormat("START");
    }
        
    // Update is called once per frame
    void Update()
    {
        //======================================
        // CONDITION
        //======================================
        // NO MORE WAYPOINT
        if (target == null || target.Count <= index)
            return;
        float delta = (float)(Time.timeSinceLevelLoadAsDouble - start);
        if (delta <= float.Epsilon) 
            return;

        // UPDATE TIME
        start = Time.timeSinceLevelLoadAsDouble;        

        //======================================
        // MOVE 
        //======================================
        transform.position += delta * speed * targetDir;
        
        //======================================
        // CHECK ARRIVED 
        //======================================
        // DIRECTION from CURRENT POSITION to TARGE TPOSITION
        var curdir  = Vector3.Normalize(targetPos - transform.position);
        // CHECK ARRIVED or PASSED
        var dotdir  = Vector3.Dot(targetDir, curdir);
        if (dotdir <= 0)
        {
            transform.position = targetPos;

            //======================================
            // PREPRE NEXT
            //======================================
            ++index;
            if (target != null && target.Count > index)
            {
                // SET TARGET POSITION, TARGET DIRECTION
                targetPos = target[index];
                targetDir = Vector3.Normalize(targetPos - transform.position);
            }
            else
            {
                // ARRIVED
                Debug.LogFormat("ARRIVED");
                return;
            }
        }
        // MOVED
        //Debug.LogFormat("MOVED");        
    }

    public void SetTarget(Vector3 pos)
    {
        if (target != null)
        {
            target.Clear();
            target.Add(pos);

            //======================================
            // PREPRE NEXT
            //======================================
            index = 0;
            // SET TARGET POSITION, TARGET DIRECTION
            targetPos = target[index];
            targetDir = Vector3.Normalize(targetPos - transform.position);


            //======================================
            // START TIME
            //======================================
            start = Time.timeSinceLevelLoadAsDouble;
        }
    }
}
