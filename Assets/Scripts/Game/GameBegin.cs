using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBegin : MonoBehaviour {
    private Vector3 oriPos;
    void Awake() {
        oriPos = GameManager.instance.transform.position;
        ie= GameManager.instance.FillAll(0);//快速填充
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private IEnumerator ie;
    public void BeginStart() {
        GameManager.instance.transform.position=new Vector3(-0.03f,8,0);
        StartCoroutine(ie);//快速填充
    }
    public void BeginEnd() {
        StopCoroutine(ie);
        StartCoroutine(GameManager.instance.FillAll(0.1f));
        GameManager.instance.transform.position = oriPos;
        GameManager.instance.gameBegin = true;
        this.gameObject.SetActive(false);
    }

}
