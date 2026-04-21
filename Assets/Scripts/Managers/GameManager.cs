using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    // 注册玩家
    public void RegisterPlayer(CharacterStats characterStats)
    {
        playerStats = characterStats;
    }

    // 添加结束游戏观察者
    public void AddEndGameObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    // 移除结束游戏观察者
    public void RemoveEndGameObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    // 通知结束游戏观察者
    public void NotifyEndGameObservers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
}
