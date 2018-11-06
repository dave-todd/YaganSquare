using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AzureSky;

public class FlockingTime : MonoBehaviour
{

    public AzureSkyController skyController;
    public FlockingSchool school;

    public void Start()
    {
        StartCoroutine(UpdateSchoolTime());
    }

    private IEnumerator UpdateSchoolTime()
    {
        while (true)
        {
            if (school != null && skyController != null) { school.SetCurrentHour((int)skyController.timeOfDay.hour); }
            yield return new WaitForSeconds(1);
        }
    }


}
