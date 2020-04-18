using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    //单例
    private static GameManager _instance;
    public static GameManager instance {
        get { return _instance; }
        set { _instance = value; }
    }
    public bool isLevelPass;
    public GameObject[] scoreAddEffects;
    public int xCol;//列
    public int yRow;//行
    public GameObject gridPrefab;
    public ModelBase lastSelectModel;
    public ModelBase selectModel;//鼠标点击的当前对象
    public ModelBase targetModel;//玩家目的移动对象
    //种类
    public enum ModelType {
        //空,默认,障碍物,行消除,列消除,彩虹道具
        Empty, Normal, Wall, CrossClear, RainBow, Count//count为标记类型
    }
    //通过字典查找对应类型的预制体
    public Dictionary<ModelType, GameObject> modelPrefabDict;
    [System.Serializable]
    public struct ModelPrefab {
        public ModelType type;
        public GameObject prefab;
    }
    //结构体数组
    public ModelPrefab[] modelPrefabs;
    public ModelBase[,] models;//二维数组
    public float fillTime = 0.1f;//填充时间间隔
    //audio
    public AudioClip[] audios;
    //UI控制相关
    public GameObject pausePanel;
    public GameObject aboutUs;
    public GameObject audioOnOff;
    public bool canAudio;
    public Text scoreText;//score
    public Text restTimeText;//time
    public float gameTime = 60f;
    public bool gameover;
    public int score;//分数
    public GameObject gameoverPanel;
    public Text overScoreText;
    public Text historySoreText;
    public GameObject breakRecord;
    public Transform spawn;
    public GameObject[] excellent;
    public GameObject beginPanel;
    public bool gameBegin;
    private int scoreStep;
    int curLevelTime;
    public Transform canvas;
    public GameObject[] timeAddEffetc;
    public int awardNums;
    public Text curStep;
    public int step; //二次移动
    public bool isSkill;
    public bool canClick;

    public Text taskText;
    public Text awardText;
    public int curClearModels;
    private int award;
    private int goalNums;
    private string curTaskType;
    private bool showTaskFinishedPanel;
    public GameObject taskFinishPanel;
    private int matchSixTimes;
    public Image bar;
    public Text progress;
    private bool doubleScore;
    public Text diamandText;
    public GameObject passImage;
    void Awake() {
        curLevelTime = 60;
        matchSixTimes = 0;
        isLevelPass = false;
        doubleScore = false;
        taskFinishPanel.SetActive(false);
        //showTaskFinishedPanel = true;
        curClearModels = 0;
        //GameTask task=new GameTask(UnityEngine.Random.Range(0,2));
        GameTask task = new GameTask(2);
        string[] taskInfo = task.TaskInfo();
        taskText.text = taskInfo[1];
        awardText.text = taskInfo[3];
        curTaskType = taskInfo[0];
        goalNums = Convert.ToInt32(taskInfo[2]);
        award= Convert.ToInt32(taskInfo[3]);
        isSkill = false;
        canClick = true;
        step = 0;
        //Destroy(this);
        awardNums = 0;
        gameBegin = false;
        gameover = false;
        instance = this;
        
        canAudio = PlayerPrefs.GetInt("Audio", 1) == 1;
        if (canAudio) {
            audioOnOff.SetActive(false);
            Camera.main.GetComponent<AudioSource>().Play();
        }
        else {
            audioOnOff.SetActive(true);
            Camera.main.GetComponent<AudioSource>().Stop();
        }
        beginPanel.SetActive(true);
        if (PlayerPrefs.GetInt("level")==1) {
            Time.timeScale = 0.1f;
        }
    }
    void Start() {
        passImage.SetActive(false);
        models = new ModelBase[xCol, yRow];
        //为字典赋值
        modelPrefabDict = new Dictionary<ModelType, GameObject>();
        foreach (var mp in modelPrefabs) {
            if (!modelPrefabDict.ContainsKey(mp.type)) {
                modelPrefabDict.Add(mp.type, mp.prefab);
            }
        }
        //实例化格子
        for (int x = 0; x < xCol; x++) {
            for (int y = 0; y < yRow; y++) {
                //生成格子
                GameObject grid = Instantiate(gridPrefab, CalGridPos(x, y), Quaternion.identity);
                grid.transform.parent = this.transform;//将格子的父物体设置为GameManager
            }
        }
        //实例化模型
        for (int x = 0; x < xCol; x++) {
            for (int y = 0; y < yRow; y++) {
                CreatNewModel(x, y, ModelType.Empty);
            }
        }
    }

    void Update() {
        if (gameover) {
            return;
        }
        //Debug.Log(time);
        if (gameTime <= 0) {
            gameTime = 0;
            //TODO:失败处理
            gameover = true;
            gameoverPanel.SetActive(true);
            GameOver();
            return;
        }
        if (gameBegin) {
            gameTime -= Time.deltaTime;
            restTimeText.text = gameTime.ToString("0.0");//0取整,0.0保留一位小数,0.00保留两位小数......
            scoreText.text = score + "";
            if (curTaskType=="0") {
                progress.text = Mathf.Clamp((float)curClearModels * 100 / goalNums, 0, 100).ToString("0.0") + "%";
                bar.fillAmount = Mathf.Clamp(Mathf.Lerp(bar.fillAmount, (float)curClearModels / goalNums, 0.2f),0,1);
            }
            else {
                bar.fillAmount = Mathf.Clamp(Mathf.Lerp(bar.fillAmount, (float)score / goalNums, 0.2f),0,1);
                progress.text = (Mathf.Clamp((float)score * 100 / goalNums,0,100)).ToString("0.0") + "%";
            }
            if (progress.text == "100%"&&doubleScore) {

                doubleScore = false;
            }
        }
        else {
            score = 0;
        }
        if (showTaskFinishedPanel) {
            if (curTaskType == "0") {
                if (curClearModels >= goalNums) {
                    Debug.Log("task 0 finish");
                    taskFinishPanel.SetActive(true);
                    showTaskFinishedPanel = false;
                }
            }
            else {
                if (score >= goalNums) {
                    Debug.Log("task 1 finish");
                    taskFinishPanel.SetActive(true);
                    showTaskFinishedPanel = false;
                }
            }
        }
    }
    //计算格子的位置坐标
    public Vector3 CalGridPos(int x, int y) {
        return new Vector3(transform.position.x - xCol / 2f * 0.56f + x * 0.52f, transform.position.y + yRow / 2f * 0.15f - y * 0.52f);
    }
    //产生model的方法
    public ModelBase CreatNewModel(int x, int y, ModelType type) {
        GameObject newModel = Instantiate(modelPrefabDict[type], CalGridPos(x, y), Quaternion.identity);
        newModel.transform.parent = transform;
        models[x, y] = newModel.GetComponent<ModelBase>();
        models[x, y].Init(x, y, this, type);
        return models[x, y];
    }
    //全部填充
    public IEnumerator FillAll(float t) {
        bool needFill = true;
        while (needFill) {
            yield return new WaitForSeconds(5*t);
            while (Fill(5*t)) {
                yield return new WaitForSeconds(5*t);
            }
            //清除匹配的model
            needFill = ClearAllMatchModels();
        }
        isSkill = false;
    }
    //分布填充
    public bool Fill(float t) {
        isSkill = true;
        bool notFinished = false;//本次填充是否完成
        for (int y = yRow - 2; y >= 0; y--) {
            for (int x = 0; x < xCol; x++) {
                ModelBase model = models[x, y];//当前元素的基础组件
                //向下填充空缺
                if (model.CanMove()) {
                    ModelBase modelBelow = models[x, y + 1];//正下方model组件
                    if (modelBelow.Type == ModelType.Empty) {//垂直填充
                        if (modelBelow.gameObject != null) {
                            Destroy(modelBelow.gameObject);
                            model.ModelMoveComponent.Move(x, y + 1, t);//向下移动
                            models[x, y + 1] = model;//正下方的组件指向当前组件
                            CreatNewModel(x, y, ModelType.Empty);//当前元素置空
                            notFinished = true;
                        }
                        else {
                            CreatNewModel(x, y + 1, ModelType.Empty);//生成一个空物体
                        }
                    }
                    //斜向填充,用于解决存在障碍物的情况
//                    else {
//                        for (int down = -8; down <= 8; down++) {
//                            if (down != 0) {
//                                int downX = x + down;
//                                if (downX >= 0 && downX < xCol) {//排除最右侧
//                                    ModelBase downModel = models[Mathf.Clamp(downX, 0, 7), y + 1];
//                                    if (downModel.Type == ModelType.Empty) {
//                                        bool canFill = true;//是否满足垂直填充
//                                        for (int aboveY = y; aboveY >= 0; aboveY--) {
//                                            ModelBase modelAbove = models[downX, aboveY];
//                                            if (modelAbove.CanMove()) {
//                                                break;
//                                            }
//                                            else if (!modelAbove.CanMove() && modelAbove.Type != ModelType.Empty) {
//                                                canFill = false;
//                                                break;
//                                            }
//                                        }
//                                        //斜向填充
//                                        if (!canFill) {
//                                            if (downModel.gameObject != null) {
//                                                Destroy(downModel.gameObject);
//                                                model.ModelMoveComponent.Move(downX, y + 1, t);
//                                                models[downX, y + 1] = model;
//                                                CreatNewModel(x, y, ModelType.Empty);
//                                                notFinished = true;
//                                                break;
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
                }
            }
        }
        //最底下的一层
        for (int x = 0; x < xCol; x++) {
            ModelBase model = models[x, 0];//当前元素的基础组件
            if (model.Type == ModelType.Empty) {
                //在y坐标为-1的位置生成
                GameObject newModel = Instantiate(modelPrefabDict[ModelType.Normal], CalGridPos(x, -1), Quaternion.identity);
                newModel.transform.parent = this.transform;//设置父物体
                models[x, 0] = newModel.GetComponent<ModelBase>();//更新基础组件位置
                models[x, 0].Init(x, -1, this, ModelType.Normal);//初始化
                if (models[x, 0].CanMove()) {
                    models[x, 0].ModelMoveComponent.Move(x, 0, fillTime);//向下移动
                }
                //随机一个颜色
                models[x, 0].ModelColorComponent.SetColor((ModelColor.ColorType)UnityEngine.Random.Range(0, models[x, 0].ModelColorComponent.Nums));
                notFinished = true;
            }
        }
        canClick = true;
        return notFinished;
    }
    //是否相邻判定
    public bool IsNeighbor(ModelBase m1, ModelBase m2) {
        return m1.X == m2.X && Mathf.Abs(m1.Y - m2.Y) == 1 || m1.Y == m2.Y && Mathf.Abs(m1.X - m2.X) == 1;
    }
    //交换model
    private void ExchangeModel(ModelBase m1, ModelBase m2) {
        //Debug.Log(selectModel.ModelColorComponent.Color+" "+targetModel.ModelColorComponent.Color);
        if (m1.CanMove() && m2.CanMove()) {
            models[m1.X, m1.Y] = m2;
            models[m2.X, m2.Y] = m1;
            if (step == 0) {
                if (MatchModels(m2, m1.X, m1.Y) != null || MatchModels(m1, m2.X, m2.Y) != null || m1.Type == ModelType.RainBow || m2.Type == ModelType.RainBow) {
                    int tempX = m1.X;
                    int tempY = m1.Y;
                    m1.ModelMoveComponent.Move(m2.X, m2.Y, fillTime);//交换
                    m2.ModelMoveComponent.Move(tempX, tempY, fillTime);
                    if (m1.Type == ModelType.RainBow && m1.CanClear() && m2.CanClear()) {
                        ModelColorClearByType mcct = m1.transform.GetComponent<ModelColorClearByType>();
                        //Debug.Log(mcct == null);
                        if (mcct != null) {
                            mcct.Color = m2.ModelColorComponent.Color;
                            ClearByType(mcct.Color);
                        }
                        m1.ModelClearComponent.Clear();
                        models[m1.X, m1.Y] = CreatNewModel(m1.X, m1.Y, ModelType.Empty);
                        ClearModel(m1.X, m1.Y);
                        //StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
                    }
                    if (m2.Type == ModelType.RainBow && m2.CanClear() && m1.CanClear()) {
                        ModelColorClearByType mcct = m2.transform.GetComponent<ModelColorClearByType>();
                        //Debug.Log(mcct == null);
                        if (mcct != null) {
                            mcct.Color = m1.ModelColorComponent.Color;
                            ClearByType(mcct.Color);
                        }
                        m2.ModelClearComponent.Clear();
                        models[m2.X, m2.Y] = CreatNewModel(m2.X, m2.Y, ModelType.Empty);
                        ClearModel(m2.X, m2.Y);
                        //StartCoroutine(FillAll(time));//将消除后的空位进行填充
                    }
                    ClearAllMatchModels();//清除所有匹配的model
                    StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
                    selectModel = null;
                    targetModel = null;
                    step = 0;
                    curStep.text = "0";
                }
                else {
                    int tempX = m1.X;
                    int tempY = m1.Y;
                    m1.ModelMoveComponent.Move(m2.X, m2.Y, fillTime);//交换
                    m2.ModelMoveComponent.Move(tempX, tempY, fillTime);
                    step++;
                    targetModel = null;
                    curStep.text = "1";
                }
            }
            else {
                if (MatchModels(m2, m1.X, m1.Y) != null || MatchModels(m1, m2.X, m2.Y) != null || m1.Type == ModelType.RainBow || m2.Type == ModelType.RainBow) {
                    int tempX = m1.X;
                    int tempY = m1.Y;
                    m1.ModelMoveComponent.Move(m2.X, m2.Y, fillTime);//交换
                    m2.ModelMoveComponent.Move(tempX, tempY, fillTime);
                    if (m1.Type == ModelType.RainBow && m1.CanClear() && m2.CanClear()) {
                        ModelColorClearByType mcct = m1.transform.GetComponent<ModelColorClearByType>();
                        //Debug.Log(mcct == null);
                        if (mcct != null) {
                            mcct.Color = m2.ModelColorComponent.Color;
                            ClearByType(mcct.Color);
                        }
                        m1.ModelClearComponent.Clear();
                        models[m1.X, m1.Y] = CreatNewModel(m1.X, m1.Y, ModelType.Empty);
                        ClearModel(m1.X, m1.Y);
                        //StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
                    }
                    if (m2.Type == ModelType.RainBow && m2.CanClear() && m1.CanClear()) {
                        ModelColorClearByType mcct = m2.transform.GetComponent<ModelColorClearByType>();
                        //Debug.Log(mcct == null);
                        if (mcct != null) {
                            mcct.Color = m1.ModelColorComponent.Color;
                            ClearByType(mcct.Color);
                        }
                        m2.ModelClearComponent.Clear();
                        models[m2.X, m2.Y] = CreatNewModel(m2.X, m2.Y, ModelType.Empty);
                        ClearModel(m2.X, m2.Y);
                        //StartCoroutine(FillAll(time));//将消除后的空位进行填充
                    }
                    ClearAllMatchModels();//清除所有匹配的model
                    StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
                    selectModel = null;
                    targetModel = null;
                    step = 0;
                    curStep.text = "0";
                }
                else {
                    //还原基础脚本
                    models[m1.X, m1.Y] = m1;
                    models[m2.X, m2.Y] = m2;
                    models[m1.X, m1.Y].ModelMoveComponent.Undo(m1, m2, fillTime);//交换位置再还原
                    step = 1;
                    curStep.text = "1";
                }
            }
        }
        //StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
    }
    //新的交换规则
    private void ExchangeModel_New(ModelBase m1, ModelBase m2) {
        if (m1.CanMove() && m2.CanMove()) {
            models[m1.X, m1.Y] = m2;
            models[m2.X, m2.Y] = m1;
            if (step == 0) {
                int tempX = m1.X;
                int tempY = m1.Y;
                if (MatchModels(m2, m1.X, m1.Y) != null || MatchModels(m1, m2.X, m2.Y) != null) {
                    m1.ModelMoveComponent.Move(m2.X, m2.Y, fillTime);//交换
                    m2.ModelMoveComponent.Move(tempX, tempY, fillTime);
                    step = 0;
                    ClearAllMatchModels();//清除所有匹配的model
                    StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
                    curStep.text = 2 + "";
                }
                else {
                    m1.ModelMoveComponent.Move(m2.X, m2.Y, fillTime);//交换
                    m2.ModelMoveComponent.Move(tempX, tempY, fillTime);
                    step++;
                    curStep.text = "1";
                }
            }
            else {
                int tempX = m1.X;
                int tempY = m1.Y;
                if (MatchModels(m2, m1.X, m1.Y) != null || MatchModels(m1, m2.X, m2.Y) != null) {
                    m1.ModelMoveComponent.Move(m2.X, m2.Y, fillTime);//交换
                    m2.ModelMoveComponent.Move(tempX, tempY, fillTime);
                    ClearAllMatchModels();//清除所有匹配的model
                    StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
                    step = 0;
                    curStep.text =  "2";
                }
                else {
                    //还原基础脚本
                    models[m1.X, m1.Y] = m1;
                    models[m2.X, m2.Y] = m2;
                    models[m1.X, m1.Y].ModelMoveComponent.Undo(m1, m2, fillTime);//交换位置再还原
                    step = 1;
                    curStep.text = "1";
                }
            }
        }
        m1.ModelColorComponent.UnSelect(m1.ModelColorComponent.highlight);
        m2.ModelColorComponent.UnSelect(m2.ModelColorComponent.highlight);
        lastSelectModel = selectModel;
        selectModel = null;
    }
    //选中对象
    public void SelectModel(ModelBase m) {
        if (gameover) {
            return;
        }
        if (isSkill==false) {
            lastSelectModel = selectModel;
            selectModel = m;
        }
    }
    //目标对象
    public void TargetModel(ModelBase m) {
        if (gameover) {
            return;
        }
        if (isSkill==false) {
            targetModel = m;
        }
    }
    //鼠标抬起,model交换
    public void ReleaseModel() {
        if (gameover) {
            return;
        }
        if (isSkill==false&&selectModel!=null&&targetModel!=null) {
            if (IsNeighbor(selectModel, targetModel)) {
                ExchangeModel_New(selectModel, targetModel);
                //ExchangeModel_New(selectModel, targetModel);
            }
        }
        StartCoroutine(FillAll(fillTime));//将消除后的空位进行填充
    }
    //匹配model
    public List<ModelBase> MatchModels(ModelBase model, int newX, int newY) {
        if (model.CanColor()) {
            ModelColor.ColorType color = model.ModelColorComponent.Color;
            List<ModelBase> matchRow = new List<ModelBase>();//存取行
            List<ModelBase> matchCol = new List<ModelBase>();//存取列
            List<ModelBase> match = new List<ModelBase>();//存取全部可消除的列表
            //行匹配
            matchRow.Add(model);
            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++) {
                for (int xDistance = 1; xDistance < xCol; xDistance++) {
                    int x;
                    if (i == 0) {
                        x = newX - xDistance;
                    }
                    else {
                        x = newX + xDistance;
                    }
                    if (x < 0 || x >= xCol) {
                        break;
                    }
                    if (models[x, newY].CanColor() && models[x, newY].ModelColorComponent.Color == color) {
                        matchRow.Add(models[x, newY]);
                    }
                    else {
                        break;
                    }
                }
            }
            if (matchRow.Count >= 3) {
                foreach (var r in matchRow) {
                    match.Add(r);
                }
            }
            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchRow.Count >= 3) {
                for (int i = 0; i < matchRow.Count; i++) {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++) {
                        for (int yDistance = 1; yDistance < yRow; yDistance++) {
                            int y;
                            if (j == 0) {
                                y = newY - yDistance;
                            }
                            else {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow) {
                                break;
                            }
                            if (models[matchRow[i].X, y].CanColor() && models[matchRow[i].X, y].ModelColorComponent.Color == color) {
                                matchCol.Add(models[matchRow[i].X, y]);
                            }
                            else {
                                break;
                            }
                        }
                    }
                    if (matchCol.Count < 2) {
                        matchCol.Clear();
                    }
                    else {
                        for (int j = 0; j < matchCol.Count; j++) {
                            match.Add(matchCol[j]);
                        }
                        break;
                    }
                }
            }
            //if (match.Count >= 3) {
            //    return match;
            //}
            matchRow.Clear();
            matchCol.Clear();
            matchCol.Add(model);
            //列匹配
            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++) {
                for (int yDistance = 1; yDistance < yRow; yDistance++) {
                    int y;
                    if (i == 0) {
                        y = newY - yDistance;
                    }
                    else {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >= yRow) {
                        break;
                    }
                    if (models[newX, y].CanColor() && models[newX, y].ModelColorComponent.Color == color) {
                        matchCol.Add(models[newX, y]);
                    }
                    else {
                        break;
                    }
                }
            }
            if (matchCol.Count >= 3) {
                for (int i = 0; i < matchCol.Count; i++) {
                    match.Add(matchCol[i]);
                }
            }
            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchCol.Count >= 3) {
                for (int i = 0; i < matchCol.Count; i++) {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++) {
                        for (int xDistance = 1; xDistance < xCol; xDistance++) {
                            int x;
                            if (j == 0) {
                                x = newX - xDistance;
                            }
                            else {
                                x = newX + xDistance;
                            }
                            if (x < 0 || x >= xCol) {
                                break;
                            }
                            if (models[x, matchCol[i].Y].CanColor() && models[x, matchCol[i].Y].ModelColorComponent.Color == color) {
                                matchRow.Add(models[x, matchCol[i].Y]);
                            }
                            else {
                                break;
                            }
                        }
                    }
                    if (matchRow.Count < 2) {
                        matchRow.Clear();
                    }
                    else {
                        for (int j = 0; j < matchRow.Count; j++) {
                            match.Add(matchRow[j]);
                        }
                        break;
                    }
                }
            }
            if (match.Count >= 3) {
                return match;
            }
        }
        return null;
    }
    //清除模块
    #region Clear Module
    //清除model
    public bool ClearModel(int x, int y) {
        //当前model可以清除并且没有正在清除
        if (models[x, y].CanClear() && models[x, y].ModelClearComponent.IsClearing == false) {
            if (models[x, y].Type != ModelType.CrossClear && models[x, y].Type != ModelType.RainBow&&models[x,y].Type!=ModelType.Count) {
                models[x, y].ModelClearComponent.Clear();//将model清除掉
                CreatNewModel(x, y, ModelType.Empty);//原地生成一个新的空类型
                if (gameBegin) {
                    curClearModels++;
                    PlayerPrefs.SetInt("ClearModelNums", PlayerPrefs.GetInt("ClearModelNums", 0) + 1);//记录消除块数
                }
                return true;
            }
        }

        selectModel = null;
        targetModel = null;
        return false;
    }
    //清除匹配的model列表
    public bool ClearAllMatchModels() {
        bool needFill = false;
        for (int y = 0; y < yRow; y++) {
            for (int x = 0; x < xCol; x++) {
                if (models[x, y].CanClear()) {
                    List<ModelBase> matchList = MatchModels(models[x, y], x, y);
                    if (matchList != null) {
                        int num = matchList.Count;
                        //根据消除个数处理分数
                        if (gameBegin) {
                            switch (num) {
                                case 3:
                                    scoreStep = 20;
                                    score += scoreStep;
                                    ScoreAddEffect(0);
                                    break;
                                case 4:
                                    Excellent(0);
                                    scoreStep = 60;
                                    score += scoreStep;
                                    ScoreAddEffect(1);
                                    break;
                                case 5:
                                    scoreStep = 80;
                                    score += scoreStep;
                                    if (isSkill == false) {
                                        gameTime += 6f;
                                        curLevelTime += 6;
                                        Excellent(1);
                                        PlayerPrefs.SetInt("TimeAddNums", PlayerPrefs.GetInt("TimeAddNums", 0) + 6);//记录累计加时
                                    }
                                    ScoreAddEffect(2);
                                    break;
                                case 6:
                                    matchSixTimes++;
                                    scoreStep = 180;
                                    score += scoreStep;
                                    if (isSkill == false) {
                                        gameTime += 10;
                                        curLevelTime += 10;
                                        Excellent(2);
                                        PlayerPrefs.SetInt("TimeAddNums", PlayerPrefs.GetInt("TimeAddNums", 0) + 10);//记录累计加时
                                    }
                                    ScoreAddEffect(3);
                                    break;
                                case 7:
                                    scoreStep = 310;
                                    score += scoreStep;
                                    if (isSkill == false) {
                                        curLevelTime += 15;
                                        gameTime += 15;
                                        Excellent(3);
                                        PlayerPrefs.SetInt("TimeAddNums", PlayerPrefs.GetInt("TimeAddNums", 0) + 15);//记录累计加时
                                    }
                                    ScoreAddEffect(4);
                                    showTaskFinishedPanel = true;
                                    break;
                            }
                        }
                        //生成奖励球
                        ModelType specialModelType = ModelType.Count;//是否产生特殊奖励
                        ModelBase model = matchList[UnityEngine.Random.Range(0, matchList.Count)];
                        //ModelBase model = models[UnityEngine.Random.Range(0, xCol), UnityEngine.Random.Range(0, yRow)];
                        int specialModelX = model.X;
                        int specialModelY = model.Y;
                        if (matchList.Count == 5) {
                            specialModelType = ModelType.CrossClear;
                        }
                        else if (matchList.Count >= 6) {
                            specialModelType = ModelType.RainBow;
                        }

                        if (isSkill) {
                            specialModelType = ModelType.Count;
                        }
                        matchList.Add(models[model.x, model.y]);
                        foreach (var m in matchList) {
                            if (ClearModel(m.X, m.Y)) {
                                needFill = true;
                            }
                        }
                        if (specialModelType != ModelType.Count) {
                            isSkill = true;
                        }
                        if (specialModelType != ModelType.Count && gameBegin) {
                            if (models[specialModelX, specialModelY].type == ModelType.Empty) {
                                Destroy(models[specialModelX, specialModelY]);
                                ModelBase newModel = CreatNewModel(specialModelX, specialModelY, specialModelType);
                                if (specialModelType == ModelType.CrossClear && newModel.CanColor() && matchList[0].CanColor()) {
                                    newModel.ModelColorComponent.SetColor(ModelColor.ColorType.Cross);
                                }
                                //Rainbow
                                else if (specialModelType == ModelType.RainBow && newModel.CanColor()) {
                                    newModel.ModelColorComponent.SetColor(ModelColor.ColorType.Rainbow);
                                }
                            }
                        }
                    }
                }
            }
        }
        return needFill;
    }
    //同类型消除
    public void ClearByType(ModelColor.ColorType color) {
        if (gameBegin) {
            PlayerPrefs.SetInt("RainbowNums", PlayerPrefs.GetInt("RainbowNums", 0) + 1);//记录彩虹个数
            awardNums++;
            PlayerPrefs.SetInt("LuckDog", awardNums);
            int count = 0;
            for (int x = 0; x < xCol; x++) {
                for (int y = 0; y < yRow; y++) {
                    if (models[x, y].CanColor() && models[x, y].ModelColorComponent.Color == color) {
                        count++;
                        ClearModel(x, y);
                    }
                }
            }
            score += 5 * count;
            score += 20;
            StartCoroutine(FillAll(fillTime));
        }
    }
    //十字消除
    public void ClearCross(int x, int y) {
        if (gameBegin) {
            PlayerPrefs.SetInt("CrossNums", PlayerPrefs.GetInt("CrossNums", 0) + 1);//记录十字个数
            awardNums++;
            //Debug.Log("cross");
            for (int i = Mathf.Clamp(x - 1, 0, xCol - 1); i <= Mathf.Clamp(x + 1, 1, xCol - 1); i++) {
                ClearModel(i, y);
                score += 10;
            }
            for (int j = Mathf.Clamp(y - 1, 0, yRow - 1); j <= Mathf.Clamp(y + 1, 1, yRow - 1); j++) {
                if (j != y) {
                    ClearModel(x, j);
                    score += 10;
                }
            }
            score += 10;
            if (models[x, y] != null && models[x, y].CanClear()) {
                models[x, y].ModelClearComponent.Clear();
                models[x, y] = CreatNewModel(x, y, ModelType.Empty);
            }
            StartCoroutine(FillAll(fillTime));
        }
    }
    #endregion
    //处理UI界面的事件
    #region UI Events
    //加时特效
    public void Excellent(int index) {
        Instantiate(excellent[index], spawn);
        if (index != 0) {
            GameObject go = Instantiate(timeAddEffetc[index], canvas);
            switch (index) {
                case 1: go.GetComponent<RectTransform>().anchoredPosition = new Vector2(700, 800); break;
                case 2: go.GetComponent<RectTransform>().anchoredPosition = new Vector2(774, 697); break;
                case 3: go.GetComponent<RectTransform>().anchoredPosition = new Vector2(774, 697); break;
            }
            go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
        }
    }
    //加分特效
    public void ScoreAddEffect(int index) {
        Debug.Log(index+3);
        GameObject go = Instantiate(scoreAddEffects[index], canvas);
        go.GetComponent<RectTransform>().anchoredPosition=new Vector2(840,980);
        go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
    }
    public void Pause() {
        pausePanel.SetActive(true);
        pausePanel.GetComponent<Animator>().SetTrigger("open");
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 1);
        }
    }

    public void Resume() {
        Time.timeScale = 1;
        pausePanel.GetComponent<Animator>().SetTrigger("close");
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[1], Camera.main.transform.position, 1);
        }
    }

    public void Replay() {
        Time.timeScale = 1;
        if (gameoverPanel.activeInHierarchy) {
            gameoverPanel.GetComponent<Animator>().SetTrigger("close");
        }
        SceneManager.LoadScene(1);
    }
    public void Quit() {
        Time.timeScale = 1;
        if (gameover) {
            gameoverPanel.GetComponent<Animator>().SetTrigger("close");
        }
        SceneManager.LoadScene(0);
    }
    //界面显示
    public void AboutUsDisplay() {
        Time.timeScale = 1;
        pausePanel.GetComponent<Animator>().SetTrigger("close");
        aboutUs.SetActive(true);
        aboutUs.GetComponent<Animator>().SetTrigger("display");
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 1);
        }
    }
    public void AboutUsClose() {
        Time.timeScale = 1;
        aboutUs.GetComponent<Animator>().SetTrigger("close");
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[1], Camera.main.transform.position, 1);
        }
    }
    public void AudioController() {
        if (canAudio) {
            canAudio = false;
            audioOnOff.SetActive(true);
            Camera.main.GetComponent<AudioSource>().Stop();
            PlayerPrefs.SetInt("Audio", 0);
        }
        else {
            canAudio = true;
            audioOnOff.SetActive(false);
            Camera.main.GetComponent<AudioSource>().Play();
            PlayerPrefs.SetInt("Audio", 1);
        }
    }
    //游戏结束的逻辑
    public void GameOver() {
        //排行榜
        if (score>PlayerPrefs.GetInt("Top1",0)) {
            if (PlayerPrefs.GetInt("Top2", 0)!=0) {
                PlayerPrefs.SetInt("Top3",PlayerPrefs.GetInt("Top2"));
                PlayerPrefs.SetInt("Time3", PlayerPrefs.GetInt("Time2"));//
                PlayerPrefs.SetInt("Top2", PlayerPrefs.GetInt("Top1"));
                PlayerPrefs.SetInt("Time2", PlayerPrefs.GetInt("Time1"));//
            }
            else {
                PlayerPrefs.SetInt("Top2", PlayerPrefs.GetInt("Top1"));
                PlayerPrefs.SetInt("Time2", PlayerPrefs.GetInt("Time1"));//
            }
            PlayerPrefs.SetInt("Top1",score);
            PlayerPrefs.SetInt("Time1", curLevelTime);//
        }
        else if (score > PlayerPrefs.GetInt("Top2", 0)) {
            if (PlayerPrefs.GetInt("Top3",0)!=0) {
                PlayerPrefs.SetInt("Top3", PlayerPrefs.GetInt("Top2"));
                PlayerPrefs.SetInt("Time3", PlayerPrefs.GetInt("Time2"));//
            }
            PlayerPrefs.SetInt("Top2", score);
            PlayerPrefs.SetInt("Time2",curLevelTime);//
        }
        else if(score > PlayerPrefs.GetInt("Top3", 0)) {
            PlayerPrefs.SetInt("Top3", score);
            PlayerPrefs.SetInt("Time3", curLevelTime);//
        }

        if (awardNums > PlayerPrefs.GetInt("LuckDog", 0)) {
            PlayerPrefs.SetInt("LuckDog", awardNums);
        }
        PlayerPrefs.SetInt("TotalScore", (PlayerPrefs.GetInt("TotalScore", 0) + score));//记录累计分数
        //Debug.Log(score + " " + PlayerPrefs.GetInt("TotalScore", 0));
        if (score > PlayerPrefs.GetInt("HistoryHighestScore", 0)) {
            PlayerPrefs.SetInt("HistoryHighestScore", score);
            breakRecord.SetActive(true);
        }
        //2000+六消
        if (score >= 2000&&matchSixTimes>=2) {
            Debug.Log(score);
            Debug.Log(passImage.name);
            passImage.SetActive(true);
        }
        gameoverPanel.GetComponent<Animator>().SetTrigger("display");
        overScoreText.text = score.ToString();
        historySoreText.text = PlayerPrefs.GetInt("HistoryHighestScore").ToString();
        StartCoroutine(FillAll(0.1f));
        if (curTaskType == "0") {
            if (curClearModels >= goalNums) {
                diamandText.text = award + "";
                PlayerPrefs.SetInt("Diamand", PlayerPrefs.GetInt("Diamand", 0) + award);
                PlayerPrefs.SetInt("CurLevel", PlayerPrefs.GetInt("CurLevel", 0)+1);//通关
            }
            else {
                diamandText.text =  "0";
            }
        }
        else {
            if (score >= goalNums) {
                diamandText.text = award + "";
                PlayerPrefs.SetInt("Diamand", PlayerPrefs.GetInt("Diamand", 0) + award);
                PlayerPrefs.SetInt("CurLevel", PlayerPrefs.GetInt("CurLevel", 0) + 1);//通关
            }
            else {
                diamandText.text = "0";
            }
        }
    }
    #endregion

}
