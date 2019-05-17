using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelText : MonoBehaviour {

    public GameObject GameText; //setting the game object
    public GameObject Cube;    //setting the game object


    private void OnTriggerExit(Collider other) // private inside the the scene, exite trigger for collider
    {

        gameObject.SetActive(false); //if game objext which is the player, exits the cube, remove the game object (the text)
             Destroy(Cube);// destroys the cube, so it doesnt get triggered again.
        }
    }
