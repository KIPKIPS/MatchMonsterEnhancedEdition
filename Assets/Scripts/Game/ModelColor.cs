using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ModelColor : MonoBehaviour {
    public enum ColorType {
        Blue, Green, Red, Purple,Pink,Cross, Rainbow, Count
    }

    public ColorType color;
    public ColorType Color {
        get { return color; }
        set { SetColor(value); }
    }
    public Dictionary<ColorType, Sprite> colorSpriteDict;
    [System.Serializable]
    public struct ColorSprite {
        public ColorType color;
        public Sprite sprite;
    }
    public ColorSprite[] colorSprites;

    //HighlightSprite
    public Dictionary<ColorType, Sprite> highlightDict;
    [System.Serializable]
    public struct HighlightSprite {
        public ColorType color;
        public Sprite sprite;
    }
    public HighlightSprite[] highlightSprites;

    public SpriteRenderer sprite;//默认sprite
    public SpriteRenderer spriteHighlight;
    public GameObject highlight;
    public int Nums {
        get { return colorSprites.Length; }
    }

    public Animator anim;
    private IEnumerator ie;
    void Awake() {
        anim = GetComponent<Animator>();
        spriteHighlight = highlight.GetComponent<SpriteRenderer>();
        ie = Idle();
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        //为字典填充值
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        foreach (var cs in colorSprites) {
            if (!colorSpriteDict.ContainsKey(cs.color)) {
                colorSpriteDict.Add(cs.color, cs.sprite);
            }
        }
        highlightDict = new Dictionary<ColorType, Sprite>();
        foreach (var hs in highlightSprites) {
            if (!highlightDict.ContainsKey(hs.color)) {
                highlightDict.Add(hs.color, hs.sprite);
            }
        }
    }
    void Start() {
        if (GetComponent<ModelBase>().Type==GameManager.ModelType.Normal) {
            StartCoroutine(ie);
        }
    }

    IEnumerator Idle() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(0, 50));
            anim.SetTrigger("idle");
        }

    }
    // Update is called once per frame
    void Update() {

    }

    public void SetColor(ColorType newColor) {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor)) {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }
    public void Select(GameObject select) {
        if (select!=null) {
            select.gameObject.SetActive(true);
        }
        spriteHighlight.sprite = highlightDict[color];
    }
    public void UnSelect(GameObject unselect) {
        if (unselect != null) {
            unselect.gameObject.SetActive(false);
        }
    }
    void OnDestroy() {
        if (ie != null) {
            StopCoroutine(ie);
        }

    }
}
