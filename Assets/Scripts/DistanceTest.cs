using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceTest : MonoBehaviour
{

    public Transform FixedPlane;

    private Bounds PlattformBounds;
    private Vector3 PlattformCenter;

    private float MaxDistance;
    private float RelYDistanceToParent;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform t in transform)
        {
            if (t.name == "Ground")
            {
                PlattformBounds = t.GetComponent<Collider>().bounds;
                PlattformCenter = t.position;
            }
        }

        MaxDistance = Mathf.Abs(transform.parent.position.y - FixedPlane.transform.position.y);
        RelYDistanceToParent = Mathf.Abs(transform.parent.position.y - transform.position.y);
        Debug.Log(MaxDistance);
    }

    // Update is called once per frame
    void Update()
    {
        float shortestDistance = 99999999;

        Vector3 pos1 = new Vector3(PlattformCenter.x + PlattformBounds.extents.x, FixedPlane.transform.position.y, PlattformCenter.z + PlattformBounds.extents.z);
        Vector3 pos2 = new Vector3(PlattformCenter.x + PlattformBounds.extents.x, FixedPlane.transform.position.y, PlattformCenter.z - PlattformBounds.extents.z);
        Vector3 pos3 = new Vector3(PlattformCenter.x - PlattformBounds.extents.x, FixedPlane.transform.position.y, PlattformCenter.z + PlattformBounds.extents.z);
        Vector3 pos4 = new Vector3(PlattformCenter.x - PlattformBounds.extents.x, FixedPlane.transform.position.y, PlattformCenter.z - PlattformBounds.extents.z);
        Vector3[] planePoints = { pos1, pos2, pos3, pos4 };
        
        Vector3[] plattformPoints = { 
            new Vector3(PlattformBounds.extents.x, -RelYDistanceToParent, PlattformBounds.extents.z), 
            new Vector3(PlattformBounds.extents.x, -RelYDistanceToParent, -PlattformBounds.extents.z), 
            new Vector3(-PlattformBounds.extents.x, -RelYDistanceToParent, PlattformBounds.extents.z),
            new Vector3(-PlattformBounds.extents.x, -RelYDistanceToParent, -PlattformBounds.extents.z)};

        for (int i = 0; i < 4; i++)
        {
            Vector3 platformPoint = transform.TransformPoint(plattformPoints[i]);
            Vector3 planePoint = planePoints[i];

            float distance = Vector3.Distance(platformPoint, planePoint);
            Debug.Log(distance + " bei " + planePoint + " und " + platformPoint + " MIN: " + MaxDistance);

            if (distance < shortestDistance) shortestDistance = distance;
        }

        Debug.Log(shortestDistance / MaxDistance);
    }
}
