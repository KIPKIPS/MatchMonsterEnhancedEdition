using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AchievementEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public enum AchievementType {
        Cross,Rainbow,TimeAdd,MonsterKiller,Fans,TotalScore,LuckDog,WallBreaker
    }

    public GameObject messagePanel;//消息框
    public Text messageText;
    //public string message;
    public AchievementType Type;
    public int x;
    public int y;
    public void OnPointerEnter(PointerEventData eventData) {
        switch (Type) {
            case AchievementType.Cross:
                messageText.text = "十字消除大师\n解锁条件:累计使用10次十字消除技能球";
                break;
            case AchievementType.Rainbow:
                messageText.text = "彩虹糖收集者\n解锁条件:累计使用10次彩虹球";
                break;
            case AchievementType.TimeAdd:
                messageText.text = "分秒必争\n解锁条件:任意关卡累计获得30秒加时奖励";
                break;
            case AchievementType.MonsterKiller:
                messageText.text = "怪兽公敌\n解锁条件:消除怪兽累计总数大于100只";
                break;
            case AchievementType.Fans:
                messageText.text = "重度瘾君子\n解锁条件:游戏累计次数达到5次";
                break;
            case AchievementType.TotalScore:
                messageText.text = "分数大爆炸\n解锁条件:游戏累计总分大于1000分";
                break;
            case AchievementType.LuckDog:
                messageText.text = "幸运儿\n解锁条件:单一关卡获得奖励球数量大于10个";
                break;
            case AchievementType.WallBreaker:
                messageText.text = "拆迁队员\n解锁条件:游戏消除的墙壁累计数量大于100个";
                break;
        }
        messagePanel.SetActive(true);
        messagePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-254 + x * 160, 350 - y * 150);
        Debug.Log(Type);
    }

    public void OnPointerExit(PointerEventData eventData) {
        messagePanel.SetActive(false);
    }
}
