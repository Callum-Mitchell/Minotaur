using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {

    public List<AudioClip> songList;
    private int timeToNextClip;

	// Use this for initialization
	void Start () {
        timeToNextClip = 20;
        GetComponent<AudioSource>().volume = 0.2f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        if(timeToNextClip <= 0)
        {
            int songID = Random.Range(0, songList.Count);
            GetComponent<AudioSource>().clip = songList[songID];
            GetComponent<AudioSource>().Play();
            timeToNextClip = Mathf.RoundToInt(songList[songID].length / Time.deltaTime);
        }
        timeToNextClip--;
	}
}
