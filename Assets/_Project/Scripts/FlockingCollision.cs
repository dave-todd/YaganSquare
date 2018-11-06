using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingCollision : MonoBehaviour
{
    private FlockingSchool school;

    public void SetSchool(FlockingSchool theSchool)
    {
        school = theSchool;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Flocking.Fish")
        {
            FlockingFish fish = other.gameObject.GetComponent<FlockingFish>();
            if (fish.GetSchool() == school) { school.ResetTarget(); }
            
        }
    }

}
