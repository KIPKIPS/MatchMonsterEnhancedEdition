using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelBase : MonoBehaviour {
    public int x;
    public int HP;
    public int X {
        get { return x; }
        set {
            if (CanMove()) {
                x = value;
            }
        }
    }
    public float firstClickTime;
    public bool isFirstClick;
    public bool canSkill;
    public int y;
    public int Y {
        get { return y; }
        set {
            if (CanMove()) {
                y = value;
            }
        }
    }
    public GameManager.ModelType type;
    public GameManager.ModelType Type {
        get { return type; }
        set { type = value; }
    }
    public GameManager manager;
    public ModelMove modelMoveComponent;
    public ModelMove ModelMoveComponent {
        get { return modelMoveComponent; }
    }
    public ModelColor modelColorComponent;
    public GameObject effect_Row;
    public GameObject effect_Col;
    public ModelColor ModelColorComponent {
        get { return modelColorComponent; }
    }
    public ModelClear modelClearComponent;
    public ModelClear ModelClearComponent {
        get { return modelClearComponent; }
    }
    void Awake() {
        firstClickTime = 0;
        canSkill = false;
        isFirstClick = true;
        HP = 3;
        modelClearComponent = GetComponent<ModelClear>();
        modelMoveComponent = GetComponent<ModelMove>();
        modelColorComponent = GetComponent<ModelColor>();
    }
    public bool CanClear() {
        return modelClearComponent != null;
    }
    public bool CanMove() {
        return modelMoveComponent != null;
    }
    public bool CanColor() {
        return modelColorComponent != null;
    }
    void Start() {
        if (this.Type == GameManager.ModelType.Empty) {
            Invoke("Destroy", 6f);
        }
    }

    void Destroy() {
        Destroy(this.gameObject);
    }

    void Update() {

    }
    //初始化方法
    public void Init(int _x, int _y, GameManager _manager, GameManager.ModelType _type) {
        x = _x;
        y = _y;
        manager = _manager;
        type = _type;
    }

    public void OnMouseEnter() {
        manager.TargetModel(this);//设置目标
    }
    public void OnMouseDown() {
        if (manager.canClick) {
            manager.canClick = false;
            manager.SelectModel(this);//选中model
            if (GameManager.instance.lastSelectModel != GameManager.instance.selectModel) {
                if (GameManager.instance.lastSelectModel != null&& GameManager.instance.lastSelectModel.CanColor()) {
                    ModelColorComponent.UnSelect(GameManager.instance.lastSelectModel.ModelColorComponent.highlight);
                }
                if (GameManager.instance.selectModel!=null&& GameManager.instance.selectModel.CanColor()) {
                    ModelColorComponent.Select(GameManager.instance.selectModel.ModelColorComponent.highlight);
                }
            }
            else {
                if (GameManager.instance.selectModel != null && GameManager.instance.selectModel.CanColor()) {
                    ModelColorComponent.Select(GameManager.instance.selectModel.ModelColorComponent.highlight);
                }
            }
        }
        if (Type == GameManager.ModelType.CrossClear || Type == GameManager.ModelType.RainBow) {
            if (Time.time - firstClickTime > 0.5f) {
                Debug.Log("chaoshi");
                isFirstClick = true;
            }
            if (isFirstClick) {
                firstClickTime = Time.time;
                isFirstClick = false;
            }
            else {
                canSkill = true;
            }
        }
    }
    public void OnMouseUp() {
        manager.ReleaseModel();
        if (manager.isSkill == false && canSkill) {
            if (Time.time - firstClickTime <= 0.5f) {
                if (Type == GameManager.ModelType.CrossClear) {
                    Debug.Log("skill1");
                    manager.ClearCross(x, y);
                    Instantiate(effect_Row, manager.CalGridPos(x, y), Quaternion.Euler(new Vector3(0, 0, 90)));
                    Instantiate(effect_Col, manager.CalGridPos(x, y), Quaternion.identity);
                }
                else {
                    Debug.Log("skill2");
                    manager.ClearByType((ModelColor.ColorType)Random.Range(0, 4));
                    this.modelClearComponent.Clear();
                    if (manager.models[x, y] != null && manager.models[x, y].CanClear()) {
                        manager.models[x, y].ModelClearComponent.Clear();
                        manager.models[x, y] = manager.CreatNewModel(x, y, GameManager.ModelType.Empty);
                    }
                }
            }
            canSkill = false;
        }
    }
}