using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPalyer : MonoBehaviour {

    public GameObject videoPlayer;


    // Use this for initialization
	void Start () {
        videoPlayer.SetActive(false); // stoping the video at the star

    }



   private void OnTriggerEnter(Collider other) // the collider to other
    {
      if (other.tag == "player") // stating that if other collider hits the player collider
    {

     videoPlayer.SetActive(true); //set video to true, meaning play video
       
        }
    }



}
