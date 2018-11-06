using System.Collections;
using UnityEngine;

public class FlockingFish : MonoBehaviour
{
    private FlockingSchool school;
    private Rigidbody rigidBody;
    private bool ready = false;
    private float waitTime = 0;
    private float updateTime = 0;
    
    public void Start()
    {
    }

    public void FixedUpdate()
    {
        waitTime += Time.deltaTime;
        if (!ready) { return; }
        FaceForward();
        if (school.UseAvoidance) { CalcAvoidance(); }
        if (school.UseUpdateTime && waitTime < updateTime) { return; }
        waitTime = 0;
        updateTime = Random.Range(school.MinUpdateTime, school.MaxUpdateTime);
        CalcVelocity();
        ClampSpeed();
    }

    public void SetSchool(FlockingSchool theSchool)
    {
        rigidBody = GetComponent<Rigidbody>();
        school = theSchool;
        ready = true;
    }

    public FlockingSchool GetSchool()
    {
        return school;
    }

    private void CalcVelocity()
    {
        Vector3 flockVelocity = school.FlockVelocity - rigidBody.velocity;
        Vector3 flockCenter;
        Vector3 follow;
        Vector3 randomize;
        Vector3 velocity;

        if (!school.UseTimeOfDay || school.TimeOfDayActive)
        { // Follow the Target
            follow = school.LocalTarget - transform.localPosition;
            flockCenter = school.FlockCenter - transform.localPosition;
            randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
            randomize.Normalize();
            velocity = flockCenter + flockVelocity + follow * 2 + randomize * school.Randomness;
        }
        else
        { // Follow the SpawnPoint to Despawn via OnTrigger
            follow = school.TimeOfDaySpawnPoint.transform.position - transform.localPosition;
            velocity = flockVelocity + follow * 2;
        }
        
        rigidBody.velocity = rigidBody.velocity + velocity * Time.deltaTime;
    }

    private void FaceForward()
    {
        if (rigidBody.velocity == Vector3.zero) { return; }
        Quaternion lookTarget = Quaternion.LookRotation(rigidBody.velocity);
        if (school.SidewaysFish) { lookTarget = lookTarget * Quaternion.Euler(0f, 90f, 0f); }
        transform.rotation = Quaternion.Slerp(transform.rotation, lookTarget, school.RotationSpeed);
    }

    private void CalcAvoidance()
    {
        Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
        Vector3 forward = transform.forward;
        if (school.SidewaysFish) { forward = transform.right * -1; }

        if (Physics.Raycast(transform.position, forward, school.AvoidanceDistance))
        {
            rigidBody.velocity = rigidBody.velocity + randomize * school.AvoidanceOffset;
        }
    }

    private void ClampSpeed()
    {
        float speed = rigidBody.velocity.magnitude;
        if (speed > school.MaxVelocity)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * school.MaxVelocity;
        }
        else if (speed < school.MinVelocity)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * school.MinVelocity;
        }
    }

}