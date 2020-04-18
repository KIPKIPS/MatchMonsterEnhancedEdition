using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimateEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    // Use this for initialization
	void Start () {
    }

    public Button showMoreBtn;
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

    public void OnPointerExit(PointerEventData eventData) {
        if (name=="ChatPanel") {
            GetComponent<Animator>().SetTrigger("close");
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        
    }

    public void ShowMoreOpenEnd() {
        showMoreBtn.enabled = true;
    }
}
