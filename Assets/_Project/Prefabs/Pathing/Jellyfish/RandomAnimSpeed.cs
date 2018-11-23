using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimSpeed : MonoBehaviour {

    [Header("JellyFish Animation Speed")]
    public float LowestSpeed = 0.9f;
    public float HighestSpeed = 1f;
    public float randomSpeedChosen;

    void Start () {
        Animator anim = gameObject.GetComponent<Animator>();
        randomSpeedChosen = Random.Range(LowestSpeed, HighestSpeed);
        anim.speed = randomSpeedChosen;
        anim = gameObject.GetComponentInChildren<Animator>();
        anim.speed = randomSpeedChosen;
    }
}
