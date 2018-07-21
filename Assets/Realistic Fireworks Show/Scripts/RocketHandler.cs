using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class RocketHandler : MonoBehaviour {

    //variables for display gameobjects
    public GameObject displayOne;
    public GameObject displayTwo;
    public GameObject displayThree;
    public GameObject displayFour;
    public GameObject displayFive;

    //variables for rocket delay timers
    public float delayOne;
    public float delayTwo;
    public float delayThree;
    public float delayFour;
    public float delayFive; 

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(StartDelayOne());
        StartCoroutine(StartDelayTwo());
        StartCoroutine(StartDelayThree());
        StartCoroutine(StartDelayFour());
        StartCoroutine(StartDelayFive());
	}
	
	
    //Timers for each game object
    IEnumerator StartDelayOne()
    {
        yield return new WaitForSeconds(delayOne);
        displayOne.SetActive(true);
        StopCoroutine(StartDelayOne());
    }

    IEnumerator StartDelayTwo()
    {
        yield return new WaitForSeconds(delayTwo);
        displayTwo.SetActive(true);
        StopCoroutine(StartDelayTwo());
    }

    IEnumerator StartDelayThree()
    {
        yield return new WaitForSeconds(delayThree);
        displayThree.SetActive(true);
        StopCoroutine(StartDelayThree());
    }

    IEnumerator StartDelayFour()
    {
        yield return new WaitForSeconds(delayFour);
        displayFour.SetActive(true);
        StopCoroutine(StartDelayFour());
    }

    IEnumerator StartDelayFive()
    {
        yield return new WaitForSeconds(delayFive);
        displayFive.SetActive(true);
        StopCoroutine(StartDelayFive());
    }
}
