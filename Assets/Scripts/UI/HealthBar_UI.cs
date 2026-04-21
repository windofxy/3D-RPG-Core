using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar_UI : MonoBehaviour
{
    #region 配置项
    public GameObject healthBarUIPrefab;
    public Transform barPoint;
    public float lerpTime; // 血条缓动开始时间
    public bool alwaysVisible; // 是否一直可见
    public float visibleTime; // 停止攻击后多久隐藏
    #endregion

    #region 组件变量
    private Canvas healthBarCanvas;
    private Image lerpHealthSlider;
    private Image currentHealthSlider;
    private Transform uiBarTransform;
    private Transform mainCameraTransform;
    private CharacterStats currentStats;
    #endregion

    #region 私有变量
    private float hideTimeLeft;
    private float lerpTimeLeft;
    private Coroutine lerpHealthCoroutine;
    #endregion

    void Awake()
    {
        healthBarCanvas = GameObject.Find("HealthBar Canvas").GetComponent<Canvas>();
        currentStats = GetComponent<CharacterStats>();
    }

    void OnEnable()
    {
        mainCameraTransform = Camera.main.transform;
        uiBarTransform = Instantiate(healthBarUIPrefab, healthBarCanvas.transform).transform;
        lerpHealthSlider = uiBarTransform.GetChild(0).GetComponent<Image>();
        currentHealthSlider = uiBarTransform.GetChild(1).GetComponent<Image>();
        uiBarTransform.gameObject.SetActive(alwaysVisible);

        currentStats.TakeDamaged += UpdateHealthBar;
    }

    void LateUpdate()
    {
        if (uiBarTransform != null)
        {
            uiBarTransform.position = barPoint.position;
            uiBarTransform.forward = -mainCameraTransform.forward;

            // 更新血条缓动剩余时间
            if (lerpTimeLeft > 0f)
                lerpTimeLeft = Mathf.Max(lerpTimeLeft - Time.deltaTime, 0f);
            else {
                if(lerpHealthCoroutine != null) { StopCoroutine(lerpHealthCoroutine); lerpHealthCoroutine = null; }
                lerpHealthCoroutine = StartCoroutine(UpdateLerpHealthBar());
            }

            // 更新血条隐藏剩余时间
            if (hideTimeLeft > 0f)
                hideTimeLeft = Mathf.Max(hideTimeLeft - Time.deltaTime, 0f);
            else if (!alwaysVisible && hideTimeLeft <= 0f)
                uiBarTransform.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            Destroy(uiBarTransform.gameObject);
        }
        uiBarTransform.gameObject.SetActive(true);
        // 重置缓动时间
        lerpTimeLeft = lerpTime;
        // 重置可见时间
        hideTimeLeft = visibleTime;
        // 计算生命百分比并设置填充比例
        float sliderPercent = (float)currentHealth / maxHealth;
        currentHealthSlider.fillAmount = sliderPercent;
    }

    private IEnumerator UpdateLerpHealthBar()
    {
        while (lerpTimeLeft <= 0f)
        {
            // 血条缓动
            lerpHealthSlider.fillAmount = Mathf.Lerp(lerpHealthSlider.fillAmount, currentHealthSlider.fillAmount, Mathf.Min(3f * Time.deltaTime, 1f));
            if (lerpHealthSlider.fillAmount - currentHealthSlider.fillAmount <= 0.01f) {
                lerpHealthSlider.fillAmount = currentHealthSlider.fillAmount;
                yield break;
            }
            // 等待下一帧
            yield return new WaitForEndOfFrame();
        }
    }
}
