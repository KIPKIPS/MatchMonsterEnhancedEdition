using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelClear : MonoBehaviour {
    public AnimationClip destoryAnim;
    public bool isClearing;//当前model是否正在被清除
    public bool IsClearing {
        get { return isClearing; }
    }
    public ModelBase modelBase;
    public AudioClip destoryNormalAudio;
    Animator animator;
    public virtual void Clear() {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    public IEnumerator ClearCoroutine() {
        animator = GetComponent<Animator>();
        if (animator!=null) {
            animator.SetTrigger("clear");
            //animator.Play(destoryAnim.name);//播放清除动画
            //TODO:音效
            if (GameManager.instance.canAudio&&GameManager.instance.gameBegin) {
                AudioSource.PlayClipAtPoint(destoryNormalAudio,this.transform.position);
            }
            yield return new WaitForSeconds(destoryAnim.length);//等待清除动画播放的时间
            Destroy(this.gameObject);
        }
    }

    void Awake() {
        modelBase = GetComponent<ModelBase>();
    }
    void Start() {

    }
    void Update() {

    }

    void OnMouseDown() {
        
    }
}
