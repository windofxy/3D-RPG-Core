using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region 配置项
    #endregion

    #region 组件变量
    CinemachineFreeLook cinemachineFreeLook;
    #endregion

    [HideInInspector]
    public CharacterStats playerStats;

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        cinemachineFreeLook = GetComponent<CinemachineFreeLook>();
    }

    // 注册玩家
    public void RegisterPlayer(CharacterStats characterStats)
    {
        playerStats = characterStats;
        if (playerStats != null)
        {
            cinemachineFreeLook.Follow = playerStats.transform;
            cinemachineFreeLook.LookAt = playerStats.transform;
        }
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
