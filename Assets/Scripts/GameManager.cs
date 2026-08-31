using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Rules")]
    [SerializeField] private float totalTime = 120f;
    [SerializeField] private int totalGenerators = 4;

    [Header("References")]
    [SerializeField] private List<Generator> generators = new List<Generator>();
    [SerializeField] private ExitDoor exitDoor;
    [SerializeField] private PlayerController player;

    private float timeRemaining;
    private int currentSequenceStep = 1;
    private bool isGameOver = false;

    public bool IsGameOver => isGameOver;
    public float TimeRemaining => timeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        timeRemaining = totalTime;
    }

    private void Start()
    {
        if (generators.Count == 0)
        {
            generators.AddRange(FindObjectsByType<Generator>(FindObjectsSortMode.None));
            generators.Sort((a, b) => a.SequenceIndex.CompareTo(b.SequenceIndex));
        }

        if (exitDoor == null)
        {
            exitDoor = FindFirstObjectByType<ExitDoor>();
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(timeRemaining);
            UIManager.Instance.UpdateGeneratorCount(0, totalGenerators);
            UIManager.Instance.UpdateObjective("Objective: Activate Generator #1");
            UIManager.Instance.ShowNotification("Mission: Activate 4 generators in order", Color.cyan);
        }

        if (exitDoor != null)
        {
            exitDoor.UpdateProgress(0, totalGenerators);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(timeRemaining);
        }

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            TriggerLose("TIME EXPIRED! Facility power collapsed.");
        }
    }

    public void TryActivateGenerator(Generator gen)
    {
        if (isGameOver || gen == null) return;

        if (gen.IsActivated)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification($"Generator #{gen.SequenceIndex} is already ONLINE.", new Color(1f, 0.8f, 0.2f, 1f));
            }
            return;
        }

        if (gen.SequenceIndex == currentSequenceStep)
        {
            // Correct sequence!
            gen.Activate();
            int activatedCount = currentSequenceStep;
            currentSequenceStep++;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateGeneratorCount(activatedCount, totalGenerators);
            }

            if (exitDoor != null)
            {
                exitDoor.UpdateProgress(activatedCount, totalGenerators);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayActivate();
            }

            if (activatedCount >= totalGenerators)
            {
                // All 4 active!
                if (exitDoor != null)
                {
                    exitDoor.OpenDoor();
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowNotification("ALL SYSTEMS ONLINE! Exit Door is UNLOCKED!", new Color(0.2f, 1f, 0.5f, 1f));
                    UIManager.Instance.UpdateObjective("Objective: REACH THE EXIT PORTAL!");
                }
            }
            else
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowNotification($"Generator #{gen.SequenceIndex} Online! ({activatedCount}/{totalGenerators})", new Color(0.3f, 0.9f, 1f, 1f));
                    UIManager.Instance.UpdateObjective($"Objective: Activate Generator #{currentSequenceStep}");
                }
            }
        }
        else
        {
            // WRONG sequence! Reset everything
            foreach (var g in generators)
            {
                if (g != null) g.ResetGenerator();
            }

            currentSequenceStep = 1;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateGeneratorCount(0, totalGenerators);
                UIManager.Instance.ShowNotification($"WRONG SEQUENCE! Expected Generator #{currentSequenceStep}. Resetting all generators!", new Color(1f, 0.3f, 0.3f, 1f));
                UIManager.Instance.UpdateObjective("Objective: Activate Generator #1");
            }

            if (exitDoor != null)
            {
                exitDoor.SetLocked();
                exitDoor.UpdateProgress(0, totalGenerators);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWrong();
            }
        }
    }

    public void TriggerWin()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen(timeRemaining);
        }
    }

    public void TriggerLose(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLose();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLoseScreen(reason);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RegisterGenerator(Generator gen)
    {
        if (!generators.Contains(gen))
        {
            generators.Add(gen);
            generators.Sort((a, b) => a.SequenceIndex.CompareTo(b.SequenceIndex));
        }
    }

    public void SetExitDoor(ExitDoor door)
    {
        exitDoor = door;
    }

    public void SetPlayer(PlayerController p)
    {
        player = p;
    }
}
