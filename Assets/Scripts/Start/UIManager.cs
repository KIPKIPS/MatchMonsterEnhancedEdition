using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager instance {
        get { return _instance; }
    }
    private static UIManager _instance;
    public static UIManager instace {
        get { return _instance; }
    }
    public GameObject settingPanel;
    public GameObject aboutUsPanel;
    public GameObject audioOff;
    public AudioClip[] audios;
    public bool canAudio;
    public Animator[] anims;
    public GameObject mapPanel;
    public GameObject achievementPanel;

    public GameObject crossLock;
    public GameObject rainbowLock;
    public GameObject timeAddLock;
    public GameObject monsterKillerLock;
    public GameObject playTimeLock;
    public GameObject totalScoreLock;
    public GameObject luckDogLock;
    public GameObject wallBreakLock;

    public GameObject showMoreBtn;
    public GameObject showMorePanel;
    private bool isShowMoreOpen;

    public GameObject topKPanel;
    public Text top1;
    public Text top2;
    public Text top3;
    public Animator showMoreAnim;
    public Animator shopPanelAnim;
    public bool isMapOpen;

    public int curSelectLevelIndex;
    public LevelManager[] levels;

    public bool isChatOpen;//聊天窗口是否打开
    private Queue<string> textQueue;
    void Awake() {
        textQueue=new Queue<string>();
        isChatOpen = false;
        curSelectLevelIndex = PlayerPrefs.GetInt("CurLevel", 0);
        if (curSelectLevelIndex==0) {
            PlayerPrefs.SetInt("CurLevel",0);
        }
        //manager = GameManager.instance;
        isShowMoreOpen = false;
        _instance = this;
        canAudio = PlayerPrefs.GetInt("Audio", 1) == 1;
        if (canAudio) {
            Camera.main.GetComponent<AudioSource>().Play();
        }
        else {
            Camera.main.GetComponent<AudioSource>().Stop();
        }
        showMoreAnim = showMorePanel.GetComponent<Animator>();
        levels[0].Unlock();
        levels[1].Unlock();
    }
    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            SendMessage();
        }
        canAudio = PlayerPrefs.GetInt("Audio", 1) == 1;
        Debug.Log(curSelectLevelIndex);
    }
    public void QuitGame() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        Application.Quit();
    }
    public void StartGame() {
        isMapOpen = true;
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[1], Camera.main.transform.position, 1);
        }
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        mapPanel.GetComponent<Animator>().SetTrigger("tomap");
    }
    public void Setting() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        settingPanel.SetActive(true);
        Debug.Log(canAudio + " " + PlayerPrefs.GetInt("CanAudio"));
        if (canAudio) {
            audioOff.SetActive(false);
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[1], Camera.main.transform.position, 1);
        }
        else {
            audioOff.SetActive(true);
        }
        settingPanel.GetComponent<Animator>().SetTrigger("open");
    }
    public void SettingPanelClose() {
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[2], Camera.main.transform.position, 1);
        }
        settingPanel.GetComponent<Animator>().SetTrigger("close");
    }
    public void AboutUs() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[1], Camera.main.transform.position, 1);
        }

        settingPanel.SetActive(false);
        aboutUsPanel.GetComponent<Animator>().SetTrigger("open");
    }
    public void AboutUsClose() {
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[2], Camera.main.transform.position, 1);
        }
        aboutUsPanel.GetComponent<Animator>().SetTrigger("close");
    }
    public void AudioOnOff() {
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
        }
        if (canAudio) {
            audioOff.SetActive(true);
            PlayerPrefs.SetInt("Audio", 0);
            Camera.main.GetComponent<AudioSource>().Stop();
        }
        else {
            audioOff.SetActive(false);
            PlayerPrefs.SetInt("Audio", 1);
            Camera.main.GetComponent<AudioSource>().Play();
        }
    }
    public void BackToMenu() {
        showMoreBtn.GetComponent<UnityEngine.UI.Button>().enabled = true;
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[2], Camera.main.transform.position, 1);
        }

        if (isMapOpen) {
            isMapOpen = false;
        }
        mapPanel.GetComponent<Animator>().SetTrigger("tomenu");
    }
    public void SelectLevel() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        curSelectLevelIndex = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);
        PlayerPrefs.SetInt("PlayTimes", PlayerPrefs.GetInt("PlayTimes", 0) + 1);//记录游玩次数
        Debug.Log(EventSystem.current.currentSelectedGameObject.name);
        if (curSelectLevelIndex==0|| curSelectLevelIndex==1) {
            SceneManager.LoadScene(1);
        }
        else {
            messageText.text = "关卡未解锁";
            messageText.transform.parent.gameObject.SetActive(true);
        }
        PlayerPrefs.SetInt("level", Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name));
    }
    public void ClearHistoryScore() {
        for (int i = 0; i <= 13; i++) {
            levels[i].Lock();
        }
        levels[0].Unlock();
        levels[1].Unlock();
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
        }
        PlayerPrefs.DeleteAll();
        audioOff.SetActive(false);
        Camera.main.GetComponent<AudioSource>().Play();

    }
    public Text playTimeNumsText;
    public Text clearModelNumsText;
    public Text totalScoreText;
    //成就界面的展示
    public void AchievementOpen() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[1], Camera.main.transform.position, 1);
        }
        //成就数据读取
        playTimeNumsText.text = PlayerPrefs.GetInt("PlayTimes", 0) + "";
        clearModelNumsText.text = PlayerPrefs.GetInt("ClearModelNums", 0) + "";
        totalScoreText.text = PlayerPrefs.GetInt("TotalScore", 0) + "";
        Debug.Log(PlayerPrefs.GetInt("TotalScore", 0));

        crossLock.SetActive(PlayerPrefs.GetInt("CrossNums", 0) < 10);
        rainbowLock.SetActive(PlayerPrefs.GetInt("RainbowNums", 0) < 10);
        timeAddLock.SetActive(PlayerPrefs.GetInt("TimeAddNums", 0) < 30);
        monsterKillerLock.SetActive(PlayerPrefs.GetInt("ClearModelNums", 0) < 100);
        playTimeLock.SetActive(PlayerPrefs.GetInt("PlayTimes", 0) < 5);
        totalScoreLock.SetActive(PlayerPrefs.GetInt("TotalScore", 0) < 1000);
        luckDogLock.SetActive(PlayerPrefs.GetInt("LuckDog", 0) < 10);
        wallBreakLock.SetActive(PlayerPrefs.GetInt("WallNums", 0) < 100);
        //
        achievementPanel.SetActive(true);
        achievementPanel.GetComponent<Animator>().SetTrigger("open");
    }
    public void AchievementClose() {
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
            AudioSource.PlayClipAtPoint(audios[2], Camera.main.transform.position, 1);
        }
        achievementPanel.GetComponent<Animator>().SetTrigger("close");
    }
    public void ShowMore() {
        if (canAudio) {
            AudioSource.PlayClipAtPoint(audios[0], Camera.main.transform.position, 0.5f);
        }
        if (isShowMoreOpen == false) {
            showMoreBtn.GetComponent<Animator>().SetTrigger("open");
            showMorePanel.GetComponent<Animator>().SetTrigger("open");
            showMoreBtn.GetComponent<UnityEngine.UI.Button>().enabled = false;
            isShowMoreOpen = true;
        }
        else {
            showMoreBtn.GetComponent<Animator>().SetTrigger("close");
            showMorePanel.GetComponent<Animator>().SetTrigger("close");
            showMoreBtn.GetComponent<UnityEngine.UI.Button>().enabled = false;
            isShowMoreOpen = false;
        }
    }

    public Text time1;
    public Text time2;
    public Text time3;
    public void TopKOpen() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        topKPanel.gameObject.SetActive(true);
        topKPanel.GetComponent<Animator>().SetTrigger("open");
        if (PlayerPrefs.GetInt("Top1", 0) != 0) {
            top1.text = PlayerPrefs.GetInt("Top1") + "";
            time1.text = PlayerPrefs.GetInt("Time1", 0) + "";
        }
        else {
            top1.text = "暂未获得成绩";
            time1.text = "暂无";
        }
        if (PlayerPrefs.GetInt("Top2", 0) != 0) {
            top2.text = PlayerPrefs.GetInt("Top2") + "";
            time2.text = PlayerPrefs.GetInt("Time2", 0) + "";
        }
        else {
            top2.text = "暂未获得成绩";
            time2.text = "暂无";
        }
        if (PlayerPrefs.GetInt("Top3", 0) != 0) {
            top3.text = PlayerPrefs.GetInt("Top3") + "";
            time3.text = PlayerPrefs.GetInt("Time3", 0) + "";
        }
        else {
            top3.text = "暂未获得成绩";
            time3.text = "暂无";
        }
    }
    public void TopKClose() {
        topKPanel.GetComponent<Animator>().SetTrigger("close");
    }

    public Text diamandText;
    //商店
    public void ShopOpen() {
        if (isMapOpen) {
            mapPanel.GetComponent<Animator>().SetTrigger("tomenu");
        }
        shopPanelAnim.SetTrigger("open");
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        diamandText.text = PlayerPrefs.GetInt("Diamand", 0) + "";
    }
    public void ShopClose() {
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }
        shopPanelAnim.SetTrigger("close");
    }

    public GameObject diamondShop;
    public GameObject sugerShop;
    public GameObject toolsShop;
    public Animator payFailureAnim;
    public void DiamondShop() {
        diamondShop.SetActive(true);
        sugerShop.SetActive(false);
        toolsShop.SetActive(false);
    }
    public void SugerShop() {
        diamondShop.SetActive(false);
        sugerShop.SetActive(true);
        toolsShop.SetActive(false);
    }
    public void ToolsShop() {
        diamondShop.SetActive(false);
        sugerShop.SetActive(false);
        toolsShop.SetActive(true);
    }

    public Text warningText;
    public void RMBPay() {
        payFailureAnim.gameObject.SetActive(true);
        payFailureAnim.SetTrigger("open");
        if (EventSystem.current.currentSelectedGameObject.transform.parent.name == "group_tools") {
            warningText.text = "FBI Warning!\n" + "购买失败,代码没写完,买了你也用不了";
        }
        else {
            warningText.text = "FBI Warning!\n" + "购买失败,开发模式下仅限购买道具";
        }
    }
    public void PayFailurePageClose() {
        payFailureAnim.SetTrigger("close");
    }

    public Text buyNums;
    public Animator payPageAnim;
    private int buyItemNums;
    public string buyType;
    //打开支付界面
    public void DiamandPay() {
        buyItemNums = 0;
        buyNums.text = "0";
        buyType = "";
        if (payPageAnim.gameObject.activeInHierarchy == false) {
            payPageAnim.gameObject.SetActive(true);
        }
        payPageAnim.SetTrigger("open");
        if (EventSystem.current.currentSelectedGameObject.name == "cross") {
            buyType = "cross";
        }
        else {
            buyType = "rainbow";
        }
    }
    public void AddItemNum() {
        buyItemNums++;
        buyNums.text = buyItemNums + "";
    }
    public void MinusItemNum() {
        buyItemNums--;
        buyItemNums = Mathf.Clamp(buyItemNums, 0, Int32.MaxValue);
        buyNums.text = buyItemNums + "";
    }
    public void DiamandPayClose() {
        payPageAnim.SetTrigger("close");
    }

    public Text messageText;
    public void Pay() {
        int restDiamand = Convert.ToInt32(diamandText.text);
        if (buyType == "cross") {
            if (restDiamand >= buyItemNums * 5 && buyItemNums > 0) {
                diamandText.text = (restDiamand - buyItemNums * 5) + "";
                PlayerPrefs.SetInt("Diamand", restDiamand - buyItemNums * 5);
                messageText.text = "购买成功!";
            }
            else {
                if (buyItemNums == 0) {
                    messageText.text = "购买数不能为零!";
                }
                else {
                    messageText.text = "购买失败,钻石不足!";
                }
            }
        }
        else {
            if (restDiamand >= buyItemNums * 20 && buyItemNums > 0) {
                diamandText.text = (restDiamand - buyItemNums * 10) + "";
                PlayerPrefs.SetInt("Diamand", restDiamand - buyItemNums * 10);
                messageText.text = "购买成功!";
            }
            else {
                if (buyItemNums==0) {
                    messageText.text = "购买数不能为零!";
                }
                else {
                    messageText.text = "购买失败,钻石不足!";
                }
            }
        }
        messageText.transform.parent.gameObject.SetActive(true);
    }
    public Animator chatPanelAnim;
    public void ChatController() {
        if (isChatOpen==false) {
            chatPanelAnim.SetTrigger("open");
        }
        if (isShowMoreOpen) {
            showMoreAnim.SetTrigger("close");
            isShowMoreOpen = false;
        }

        if (yourMessage.text!="") {
            yourMessage.text += "\n" + "历史消息记录              ";
            yourMessage.color=Color.grey;
        }
    }

    public void ChatPanelClose() {
        chatPanelAnim.SetTrigger("close");
    }
    public InputField inputField;
    public Text yourMessage;
    public void SendMessage() {
        yourMessage.color = Color.white;
        if (inputField.text!="") {
            if (textQueue.Count<=10) {
                textQueue.Enqueue(inputField.text);
            }
            else {
                textQueue.Dequeue();
                textQueue.Enqueue(inputField.text);
            }
            for (int i = 0; i < textQueue.Count; i++) {
                //yourMessage=textQueue
            }
            yourMessage.text = yourMessage.text+"\n" + inputField.text;
        }
        else {
            messageText.text = "发送内容不能为空!";
            messageText.transform.parent.gameObject.SetActive(true);
        }
        inputField.text = "";
        inputField.ActivateInputField();
        
    }
}
