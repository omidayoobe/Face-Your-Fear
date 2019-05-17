using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMoving : MonoBehaviour {

    public float speed = 5f; // variable defining speed


	// Use this for initialization
	void Start () {
		
	}

	
	// Update is called once per frame
	void Update () {

        transform.Translate(new Vector3(0, 0, 1) * speed * Time.deltaTime); // transform function
        //insided translate, new vector 3 made
        // insdie vector 3 direction of the car is selected in x,y,z
        //multipy by speed, where the car should go

	}
}
