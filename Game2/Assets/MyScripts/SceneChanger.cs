using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneChanger : MonoBehaviour {

    //visable in the inspector
    [SerializeField] private string loadLevel; // on private, so other scripts cant access it. 
 

    void OnTriggerEnter(Collider other) // triggering collider (if other colliders touch it, it triggers)
    {
        if (other.tag == "Player" ) { // if statment, if other tags (the player) touches the collider
           
            SceneManager.LoadScene(loadLevel); // sceneManager will load the scene, which ever is selected.

        }
      }
}
