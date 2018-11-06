using System.Collections;
using UnityEngine;

public class FlockingSchool : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject FishPrefab;
    
    [Header("Tank Specifications")]
    public int Width = 40;
    public int Height = 28;
    public int Depth = 20;

    [Header("Fish Specifications")]
    public int FlockSize = 20;
    public float MinVelocity = 3;
    public float MaxVelocity = 5;
    public float RotationSpeed = 20;
    public float Randomness = 10;
    private GameObject[] fishList;

    [Header("Target Specifications")]
    public int MinTargetMoveSpeed = 10;
    public int MaxTargetMoveSpeed = 20;
    public bool TargetResetOnCollision = false;
    public GameObject TargetCollisionPrefab;
    private GameObject targetCollider;

    [Header("Tunnelling Specifications")]
    public GameObject TunnelColliderPrefab;
    public int RetryCount = 20;
    private GameObject tunnelCollider;
    private Vector3 OldLocalTarget;
    private LayerMask tunnelLayer;
    
    [Header("UpdateTime Specifications")]
    public bool UseUpdateTime = false;
    public float MinUpdateTime = 0;
    public float MaxUpdateTime = 1;

    [Header("Avoidance Specifications")]
    public bool UseAvoidance = false;
    public float AvoidanceDistance = 5;
    public float AvoidanceOffset = 15;

    [Header("Time Of Day")]
    public bool UseTimeOfDay = false;
    public int OnTimeStart = 8;
    public int OffTimeEnd = 17;
    public FlockingTimeOfDaySpawn TimeOfDaySpawnPoint;

    [Header("Model Adjustments")]
    public bool SidewaysFish = false;

    [Header("Internal Use Only")]
    public Vector3 LocalTarget;
    public Vector3 FlockCenter;
    public Vector3 FlockVelocity;
    public bool TimeOfDayActive = false;
    private bool ready = false;

    public void Start()
    {
        if (TargetResetOnCollision) { CreateTargetCollider(); }
        if (UseTimeOfDay) { TimeOfDaySpawnPoint.SetSchool(this); }
        fishList = new GameObject[FlockSize];
        CreateFish();
        StartCoroutine(TargetMoving());
        tunnelLayer = 1 << TunnelColliderPrefab.layer;
        ready = true;
    }

    public void Update()
    {
        if (!ready) { return; }

        Vector3 theCenter = Vector3.zero;
        Vector3 theVelocity = Vector3.zero;

        foreach (GameObject fish in fishList)
        {
            theCenter = theCenter + fish.transform.localPosition;
            theVelocity = theVelocity + fish.GetComponent<Rigidbody>().velocity;
        }

        FlockCenter = theCenter / FlockSize;
        FlockVelocity = theVelocity / FlockSize;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(Width * 2, Height * 2, Depth * 2));
        Gizmos.color = new Color(0, 1, 0, 1);
        Gizmos.DrawSphere(ConvertLocalLocToGlobalLoc(LocalTarget), 1);
    }

    public void ResetTarget()
    {
        bool intersects = true;
        int tryCount = 0;

        while (intersects)
        {
            LocalTarget = GetRandomLocalLocInTank();
            tryCount++;

            //if the path doesnt collide
            if (CheckNewTarget() || tryCount == RetryCount) 
            {
                intersects = false;

                if (TargetResetOnCollision) { targetCollider.transform.localPosition = LocalTarget; }

                DestroyTunnel(); 
                CreateTunnel(); 
                OldLocalTarget = LocalTarget;
            }
        }
    }

    private Vector3 GetRandomLocalLocInTank()
    {
        return new Vector3(Random.Range(-Width, Width), Random.Range(-Height, Height), Random.Range(-Depth, Depth));
    }

    public void SetCurrentHour(int hour)
    { // enjoy this logic!
        if (!UseTimeOfDay) { return; }

        if (hour == OnTimeStart && !TimeOfDayActive) { TimeOfDayActivate(); } // if its start time and the school is not yet active
        else if (hour == OffTimeEnd && TimeOfDayActive) { TimeOfDayDeactivate(); } // if its end time and the school is still active
        else if (OnTimeStart < OffTimeEnd) // if start time comes before end time (day fish)
        {
            if (hour > OnTimeStart && hour < OffTimeEnd && !TimeOfDayActive) { TimeOfDayActivate(); } // if we are in active time range and not active
            else if (hour < OnTimeStart || hour > OffTimeEnd && TimeOfDayActive) { TimeOfDayDeactivate(); } // if we are outside active time range and still active
        }
        else if (OnTimeStart > OffTimeEnd) // if start time comes after end time (night fish)
        {
            if (hour > OffTimeEnd && hour < OnTimeStart && TimeOfDayActive) { TimeOfDayDeactivate(); } // if we are outside active time range and still active
            else if (hour < OffTimeEnd || hour > OnTimeStart && !TimeOfDayActive) { TimeOfDayActivate(); } // if we are in active time range and not active
        }
    }

    private void CreateTargetCollider()
    {
        targetCollider = Instantiate(TargetCollisionPrefab, ConvertLocalLocToGlobalLoc(GetRandomLocalLocInTank()), transform.rotation, transform) as GameObject;
        targetCollider.GetComponent<FlockingCollision>().SetSchool(this);
    }

    private void CreateFish()
    {
        OldLocalTarget = GetRandomLocalLocInTank();
        for (var i = 0; i < FlockSize; i++)
        {
            Vector3 randOffset = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
            Vector3 startPos = ConvertLocalLocToGlobalLoc(OldLocalTarget+ randOffset);
            GameObject fish = Instantiate(FishPrefab, startPos, transform.rotation, transform) as GameObject;
            fish.GetComponent<FlockingFish>().SetSchool(this);
            fishList[i] = fish;
        }
    }

    private Vector3 ConvertLocalLocToGlobalLoc(Vector3 local)
    {
        return (new Vector3(local.x + transform.position.x, local.y + transform.position.y, local.z + transform.position.z)); 
    }

    private IEnumerator TargetMoving()
    {
        while (true)
        {
            ResetTarget();
            yield return new WaitForSeconds(Random.Range(MinTargetMoveSpeed, MaxTargetMoveSpeed));
        }
    }

    private void TimeOfDayActivate()
    {
        TimeOfDayActive = true;
        foreach (GameObject fish in fishList)
        {
            fish.SetActive(true);
        }
    }

    private void TimeOfDayDeactivate()
    {
        TimeOfDayActive = false;
    }

    private bool CheckNewTarget()
    {
        float distance = Vector3.Distance(OldLocalTarget, LocalTarget);

        if (distance < 50f)
        {
            return false;
        }
        else
        {
            return !Physics.Raycast(ConvertLocalLocToGlobalLoc(OldLocalTarget), ConvertLocalLocToGlobalLoc(LocalTarget), distance, tunnelLayer);
        }
    }

    private void DestroyTunnel()
    {
        Destroy(tunnelCollider);
    }

    private void CreateTunnel()
    {
        Vector3 centrePoint = Vector3.Lerp(ConvertLocalLocToGlobalLoc(OldLocalTarget), ConvertLocalLocToGlobalLoc(LocalTarget), 0.5f);
        tunnelCollider = Instantiate(TunnelColliderPrefab, centrePoint, Quaternion.identity, transform);
        tunnelCollider.transform.LookAt(ConvertLocalLocToGlobalLoc(LocalTarget));
        var col = tunnelCollider.transform.GetComponent<CapsuleCollider>();
        col.height = Vector3.Distance(ConvertLocalLocToGlobalLoc(OldLocalTarget), ConvertLocalLocToGlobalLoc(LocalTarget));
    }

}
