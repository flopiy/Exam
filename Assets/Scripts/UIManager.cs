using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text generatorCountText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text promptText;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private Text notificationText;

    [Header("End Screens")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Text winStatsText;
    [SerializeField] private Button winRestartButton;

    [SerializeField] private GameObject losePanel;
    [SerializeField] private Text loseReasonText;
    [SerializeField] private Button loseRestartButton;

    private float notificationTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (winRestartButton != null)
        {
            winRestartButton.onClick.AddListener(OnRestartClicked);
        }
        if (loseRestartButton != null)
        {
            loseRestartButton.onClick.AddListener(OnRestartClicked);
        }

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (promptPanel != null) promptPanel.SetActive(false);
        if (notificationText != null) notificationText.text = "";
    }

    private void Update()
    {
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f && notificationText != null)
            {
                notificationText.text = "";
            }
        }
    }

    public void UpdateTimer(float remainingSeconds)
    {
        if (timerText == null) return;

        int mins = Mathf.Max(0, (int)remainingSeconds / 60);
        int secs = Mathf.Max(0, (int)remainingSeconds % 60);
        timerText.text = $"TIME: {mins:00}:{secs:00}";

        if (remainingSeconds <= 20f)
        {
            float blink = (Mathf.Sin(Time.time * 8f) > 0) ? 1f : 0.4f;
            timerText.color = new Color(1f, 0.2f * blink, 0.2f * blink, 1f);
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    public void UpdateGeneratorCount(int current, int total)
    {
        if (generatorCountText != null)
        {
            generatorCountText.text = $"Generators: {current} / {total}";
        }
    }

    public void UpdateObjective(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
    }

    public void ShowPrompt(string text)
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptText != null) promptText.text = text;
    }

    public void HidePrompt()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    public void ShowNotification(string text, Color color)
    {
        if (notificationText != null)
        {
            notificationText.text = text;
            notificationText.color = color;
            notificationTimer = 3.5f;
        }
    }

    public void ShowWinScreen(float timeRemaining)
    {
        HidePrompt();
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winStatsText != null)
            {
                int mins = (int)timeRemaining / 60;
                int secs = (int)timeRemaining % 60;
                winStatsText.text = $"All 4 Generators Activated!\nEscaped with {mins:00}:{secs:00} remaining!";
            }
        }
    }

    public void ShowLoseScreen(string reason)
    {
        HidePrompt();
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            if (loseReasonText != null)
            {
                loseReasonText.text = reason;
            }
        }
    }

    private void OnRestartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void BindHUD(Text timer, Text genCount, Text obj, Text prompt, GameObject promptBox, Text notif)
    {
        timerText = timer;
        generatorCountText = genCount;
        objectiveText = obj;
        promptText = prompt;
        promptPanel = promptBox;
        notificationText = notif;
    }

    public void BindPanels(GameObject winP, Text winStat, Button winBtn, GameObject loseP, Text loseReas, Button loseBtn)
    {
        winPanel = winP;
        winStatsText = winStat;
        winRestartButton = winBtn;
        losePanel = loseP;
        loseReasonText = loseReas;
        loseRestartButton = loseBtn;

        if (winRestartButton != null) winRestartButton.onClick.AddListener(OnRestartClicked);
        if (loseRestartButton != null) loseRestartButton.onClick.AddListener(OnRestartClicked);
    }
}
