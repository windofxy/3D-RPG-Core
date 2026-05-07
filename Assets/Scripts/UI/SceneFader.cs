using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneFader : MonoBehaviour
{
    #region 配置项
    public float fadeInDuration;
    public float fadeOutDuration;
    #endregion

    #region 组件变量
    private CanvasGroup canvasGroup;
    #endregion

    void Awake()
    {
        DontDestroyOnLoad(this);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public IEnumerator FadeOut(float time)
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime / time;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public IEnumerator FadeIn(float time)
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime / time;
            yield return null;
        }
        canvasGroup.alpha = 0;
        Destroy(gameObject);
    }
}
