using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Characters.FirstPerson
{

    public class GameStateManager : MonoBehaviour {

        public GameObject player;
        public GameObject musicPlayer;
        public List<GameObject> minotaurs;
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
            if(Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (!player.GetComponent<FirstPersonController>().isAlive || player.GetComponent<FirstPersonController>().isEscaped)
            {
                musicPlayer.SetActive(false);
                for(int i = 0; i < minotaurs.Count; i++)
                {
                    minotaurs[i].SetActive(false);
                }
            }
        }

        void onLossCondition(int treasureCollected)
        {

        }
    }
}