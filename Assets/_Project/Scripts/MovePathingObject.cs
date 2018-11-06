using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePathingObject : MonoBehaviour
{

    [Header("Movement")]
    public int MoveSpeed = 1;
    public int RotationSpeed = 1;
    public GameObject[] WayPoints;
    [Header("Model Adjustments")]
    public bool ReverseFacing = false;

    private int currentWayPointIndex = 0;
    private Rigidbody rb;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        MoveToCurrentWayPoint();
        IncrementCurrentWayPoint();
    }
	
	void Update ()
    {
        if (NearCurrentWayPoint()) { IncrementCurrentWayPoint(); }
        TravelToCurrentWayPoint();
    }

    void TravelToCurrentWayPoint()
    {
        int ReverseModifier = 1;
        if (ReverseFacing) { ReverseModifier = -1; }

        Vector3 direction = (WayPoints[currentWayPointIndex].transform.position - transform.position) * ReverseModifier;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), RotationSpeed * Time.deltaTime);
        transform.Translate(0, 0, Time.deltaTime * MoveSpeed * ReverseModifier);
        //rb.position += Time.deltaTime * MoveSpeed * transform.forward; // this code doesnt work on the octopus or the whale.  Whale goes backwards and occy goes sideways.
    }

    void IncrementCurrentWayPoint()
    {
        currentWayPointIndex++;
        if (currentWayPointIndex == WayPoints.Length) { currentWayPointIndex = 0; }
    }

    bool NearCurrentWayPoint()
    {
        float dist = Vector3.Distance(transform.position, WayPoints[currentWayPointIndex].transform.position);
        return (dist < MoveSpeed);
    }

    void MoveToCurrentWayPoint()
    {
        transform.position = new Vector3(WayPoints[currentWayPointIndex].transform.position.x, WayPoints[currentWayPointIndex].transform.position.y, WayPoints[currentWayPointIndex].transform.position.z);
    }

}
