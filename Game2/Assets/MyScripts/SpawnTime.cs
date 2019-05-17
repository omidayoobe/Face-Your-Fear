 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTime : MonoBehaviour {

    public Transform[] SpawnPoints; //public array  storing spawn points
    public float spawnTime = 2.5f; //timer variable (how long it takes to spawn the variable)

  
    public GameObject[] Cars; // game object cars (the item that will be spawned)
                              // also an array       

	// Use this for initialization
	void Start () {
                        //methode name
        InvokeRepeating("SpawnCars", spawnTime, spawnTime); // repeating method, calls method over 
        // calls "spawnCars" over and over
        //the initial time it will take 
        //repeat time
		
	}
	

    void SpawnCars () { //private function


        int spawnIndex = Random.Range(0, SpawnPoints.Length); //spawn point array
        // set the index number of array randomly
        // range from min 0 (first number in teh array)
        // max = maximum numbers in the array
        //takes any number randomly inside the array


        int objectIndex = Random.Range(0, Cars.Length); // cars array
        // set the index number of array randomly
        // range from min 0 (first number in teh array)
        // max = maximum numbers in the array
        //takes any number randomly inside the array


        Instantiate(Cars[objectIndex],SpawnPoints[spawnIndex].position, SpawnPoints[spawnIndex].rotation);
        //instantiates what needs to be spawned
        //we take teh game object (what we want to spawn)
        // the transform position and the transform rotation

    }



}
