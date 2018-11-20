using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AzureSky;

public class MovePathingObject : MonoBehaviour
{

    [Header("Movement")]
    public float MoveSpeed = 1;
    public int RotationSpeed = 1;
    public GameObject[] WayPoints;
    [Header("Time Of Day")]
    public bool UseTimeOfDay = false;
    public AzureSkyController SkyController;
    public int OnTimeStart = 8;
    public int OffTimeEnd = 17;
    [Header("Model Adjustments")]
    public bool ReverseFacing = false;

    private int currentWayPointIndex = 0;
    
    void Start ()
    {
        MoveToCurrentWayPoint();
        IncrementCurrentWayPoint();
        StartCoroutine(DelayedStart()); 
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        if (UseTimeOfDay) { CheckSetVisable(); }
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
    }

    void SetVisable(bool visable)
    {
        Renderer[] children = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in children)
        {
            renderer.enabled = visable;
        }
    }

    void CheckSetVisable()
    {
        int hour = (int)SkyController.timeOfDay.hour;

        if (OnTimeStart < OffTimeEnd)
        {
            if (hour >= OnTimeStart && hour < OffTimeEnd) { SetVisable(true); } // if we are in active time range and not active
            else { SetVisable(false); } // if we are outside active time range and still active
        }
        else if (OnTimeStart > OffTimeEnd) // if start time comes after end time (night fish)
        {
            if (hour > OffTimeEnd && hour <= OnTimeStart) { SetVisable(false); } // if we are outside active time range and still active
            else { SetVisable(true); } // if we are in active time range and not active
        }
    }

    void IncrementCurrentWayPoint()
    {
        currentWayPointIndex++;
        if (currentWayPointIndex == WayPoints.Length) { currentWayPointIndex = 0; }
        if (UseTimeOfDay) { CheckSetVisable(); }
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
