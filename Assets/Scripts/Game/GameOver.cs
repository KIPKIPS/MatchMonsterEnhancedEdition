using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GameOverStart() {
        Time.timeScale = 0;
    }

    public void GameOverEnd() {
        GameManager.instance.breakRecord.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
