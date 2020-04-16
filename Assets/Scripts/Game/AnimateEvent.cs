using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateEvent : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    //结束时将自身设为不可见
    public void CloseAnimateEnd() {
        this.gameObject.SetActive(false);
    }
    public void OpenAnimateEnd() {
        Time.timeScale = 0;
    }
}
