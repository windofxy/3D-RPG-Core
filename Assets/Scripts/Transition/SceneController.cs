using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    #region 配置项
    public GameObject playerPrefab;
    public SceneFader sceneFaderPrefab;
    #endregion

    #region 私有变量
    private Dictionary<string, Portal> portalMap = new Dictionary<string, Portal>();
    private bool isFadeFinished = true;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    void OnEnable()
    {
        GameManager.Instance.AddEndGameObserver(this);
    }

    void OnDisable()
    {
        GameManager.Instance.RemoveEndGameObserver(this);
    }

    public void RegisterPortal(Portal portal)
    {
        if (portal == null) return;
        if (!portalMap.TryAdd(portal.portalName, portal))
        {
            Debug.Log($"传送门注册失败，已存在名称为{portal.portalName}的传送门");
        }
    }

    public void RemovePortal(Portal portal)
    {
        if (portal == null) return;
        if (!portalMap.Remove(portal.portalName))
        {
            Debug.Log($"传送门移除失败，不存在名称为{portal.portalName}的传送门");
        }
    }

    public void ClearPortal()
    {
        portalMap.Clear();
    }

    public void TransitToFirstLevel()
    {
        StartCoroutine(TransitToDestination_Diff_Impl("GameScene_1", "portal_diff_1"));
    }

    public void TransitToDestination(Portal portal)
    {
        if (portal == null) return;
        SaveManager.Instance.SavePlayerData();
        if (portal.type == Portal.TransitionType.SAME_SCENE)
        {
            Portal destPortal = null;
            if (!portalMap.TryGetValue(portal.destnationName, out destPortal) || destPortal == null)
            {
                Debug.Log($"传送失败，不存在名称为{portal.destnationName}的传送门");
                return;
            }
            StartCoroutine(TransitToDestination_Same_Impl(destPortal));
        }
        else if (portal.type == Portal.TransitionType.DIFF_SCENE)
        {
            StartCoroutine(TransitToDestination_Diff_Impl(portal.destnationName, portal.portalName));
        }
    }

    private IEnumerator TransitToDestination_Same_Impl(Portal destination)
    {
        var player = GameManager.Instance.playerStats.gameObject;
        NavMeshAgent playerAgent = player.GetComponent<NavMeshAgent>();
        playerAgent.enabled = false;
        player.transform.SetPositionAndRotation(destination.destinationPoint.position, destination.destinationPoint.rotation);
        playerAgent.enabled = true;
        yield break;
    }

    private IEnumerator TransitToDestination_Diff_Impl(string sceneName, string destnationName)
    {
        if (string.IsNullOrEmpty(sceneName)) yield break;

        SceneFader fade = Instantiate(sceneFaderPrefab);
        yield return StartCoroutine(fade.FadeOut(1.5f));

        yield return SceneManager.LoadSceneAsync(sceneName);
        Portal destPortal = null;
        if (!portalMap.TryGetValue(destnationName, out destPortal) || destPortal == null)
        {
            Debug.Log($"传送失败，不存在名称为{destnationName}的传送门");
            yield break;
        }
        yield return Instantiate(playerPrefab, destPortal.destinationPoint.position, destPortal.destinationPoint.rotation);
        SaveManager.Instance.LoadPlayerData();

        yield return StartCoroutine(fade.FadeIn(1.5f));
    }

    private IEnumerator TransitToMainMenu()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        yield return StartCoroutine(fade.FadeOut(1.5f));
        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return StartCoroutine(fade.FadeIn(1.5f));
    }

    public void EndNotify()
    {
        if (isFadeFinished)
        {
            isFadeFinished = false;
            StartCoroutine(TransitToMainMenu());
        }
    }
}
