using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    public int levelIndex;
    public bool isLevelLock;
    public GameObject lockImage;
    public GameObject unlockImage;

    private RectTransform rect;
    private Vector2 pos;
	// Use this for initialization
	void Start () {
        rect = GetComponent<RectTransform>();
        pos = rect.anchoredPosition;
    }

    public void Unlock() {
        isLevelLock = false;
        unlockImage.SetActive(true);
        lockImage.SetActive(false);
    }

    public void Lock() {
        isLevelLock = true;
        unlockImage.SetActive(false);
        lockImage.SetActive(true);
//        for (int i = 1; i <= 3; i++) {
//            transform.GetChild(1).transform.GetChild(i).gameObject.SetActive(false);
//        }
    }
//    IEnumerator Idle() {
//        float offset=0;
//        for (int i = 0; i < 100; i++) {
//            offset += Time.deltaTime;
//        }
//        rect.anchoredPosition=new Vector2(pos.x,pos.y+offset);
//    }
	// Update is called once per frame
	void Update () {
		
	}
}
