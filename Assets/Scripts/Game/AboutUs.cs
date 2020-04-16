using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutUs : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TimeScale() {
        Time.timeScale = 0;
    }
    public void AboutUsAnimateEnd() {
        //Debug.Log("end");
        this.gameObject.SetActive(false);
    }
}
