using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    #region 组件变量
    private Transform playerUI_Canvas;
    private Transform levelUI;
    private Transform healthBarUI;
    private Transform expBarUI;
    private TMP_Text levelText;
    private Image lerpHealthSlider;
    private Image currentHealthSlider;
    private Image lerpExpSlider;
    private Image currentExpSlider;
    #endregion

    #region 私有变量
    private readonly StringBuilder sb;
    #endregion

    void Awake()
    {
        playerUI_Canvas = GameObject.Find("PlayerUI Canvas").transform;
        levelUI = playerUI_Canvas.Find("Level");
        healthBarUI = playerUI_Canvas.Find("Health Bar");
        expBarUI = playerUI_Canvas.Find("Exp Bar");
        levelText = levelUI.GetComponent<TMP_Text>();
        lerpHealthSlider = healthBarUI.GetChild(0).GetComponent<Image>();
        currentHealthSlider = healthBarUI.GetChild(1).GetComponent<Image>();
        lerpExpSlider = expBarUI.GetChild(1).GetComponent<Image>();
        currentExpSlider = expBarUI.GetChild(0).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLevel();
        UpdateHealth();
        UpdateExp();
    }

    void UpdateLevel()
    {
        var playerStats = GameManager.Instance?.playerStats ?? null;
        if (playerStats != null)
        {
            levelText.text = string.Format("Level {0:D}", playerStats.CurrentLevel);
        }
    }

    void UpdateHealth()
    {
        var playerStats = GameManager.Instance?.playerStats ?? null;
        if (playerStats != null)
        {
            currentHealthSlider.fillAmount = (float)playerStats.CurrentHealth / playerStats.MaxHealth;
            // 血条缓动
            lerpHealthSlider.fillAmount = Mathf.Lerp(lerpHealthSlider.fillAmount, currentHealthSlider.fillAmount, Mathf.Min(3f * Time.deltaTime, 1f));
        }
    }

    void UpdateExp()
    {
        var playerStats = GameManager.Instance?.playerStats ?? null;
        if (playerStats != null)
        {
            currentExpSlider.fillAmount = (float)playerStats.CurrentExp / playerStats.BaseExp;
            // 经验条缓动
            lerpExpSlider.fillAmount = Mathf.Lerp(lerpExpSlider.fillAmount, currentExpSlider.fillAmount, Mathf.Min(3f * Time.deltaTime, 1f));
        }
    }
}
