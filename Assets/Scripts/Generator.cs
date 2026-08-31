using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Generator : MonoBehaviour
{
    public static readonly List<Generator> AllGenerators = new List<Generator>();

    [Header("Generator Settings")]
    [Range(1, 4)]
    [SerializeField] private int sequenceIndex = 1;
    [SerializeField] private string colorName = "Red";

    [Header("Visual Elements")]
    [SerializeField] private SpriteRenderer baseRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private TextMesh textLabel;
    [SerializeField] private Sprite offlineSprite;
    [SerializeField] private Sprite onlineSprite;

    [Header("Colors")]
    [SerializeField] private Color activeGlowColor = new Color(0.2f, 1f, 0.7f, 0.9f);
    [SerializeField] private Color inactiveGlowColor = new Color(0.2f, 0.2f, 0.2f, 0f);

    private bool isActivated = false;
    private float pulseTime = 0f;

    public int SequenceIndex => sequenceIndex;
    public string ColorName => colorName;
    public bool IsActivated => isActivated;

    private void OnEnable()
    {
        if (!AllGenerators.Contains(this))
        {
            AllGenerators.Add(this);
        }
    }

    private void OnDisable()
    {
        AllGenerators.Remove(this);
    }

    private void Awake()
    {
        if (baseRenderer == null)
        {
            baseRenderer = GetComponent<SpriteRenderer>();
        }
        if (glowRenderer == null)
        {
            Transform glowT = transform.Find("Glow");
            if (glowT != null) glowRenderer = glowT.GetComponent<SpriteRenderer>();
        }
        if (textLabel == null)
        {
            textLabel = GetComponentInChildren<TextMesh>();
        }

        UpdateVisuals();
    }

    private void Update()
    {
        pulseTime += Time.deltaTime;

        if (glowRenderer != null)
        {
            if (isActivated)
            {
                float pulse = 0.75f + Mathf.Sin(pulseTime * 5f) * 0.25f;
                glowRenderer.color = new Color(activeGlowColor.r, activeGlowColor.g, activeGlowColor.b, pulse);
                float s = 1f + Mathf.Sin(pulseTime * 4f) * 0.08f;
                glowRenderer.transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                glowRenderer.color = new Color(0, 0, 0, 0);
            }
        }
    }

    public void Setup(int index, string colName, Sprite offSprite, Sprite onSprite)
    {
        sequenceIndex = index;
        colorName = colName;
        offlineSprite = offSprite;
        onlineSprite = onSprite;
        UpdateVisuals();
    }

    public void Setup(int index, string colName, Sprite mainSprite)
    {
        sequenceIndex = index;
        colorName = colName;
        offlineSprite = mainSprite;
        if (baseRenderer != null && mainSprite != null)
        {
            baseRenderer.sprite = mainSprite;
        }
        UpdateVisuals();
    }

    public void Activate()
    {
        isActivated = true;
        UpdateVisuals();
    }

    public void ResetGenerator()
    {
        isActivated = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (baseRenderer != null)
        {
            if (isActivated && onlineSprite != null)
            {
                baseRenderer.sprite = onlineSprite;
            }
            else if (!isActivated && offlineSprite != null)
            {
                baseRenderer.sprite = offlineSprite;
            }
            baseRenderer.color = Color.white;
        }

        if (glowRenderer != null)
        {
            glowRenderer.gameObject.SetActive(isActivated);
        }

        if (textLabel != null)
        {
            textLabel.text = $"#{sequenceIndex} {colorName.ToUpper()}\n{(isActivated ? "[ONLINE]" : "[OFFLINE]")}";
            textLabel.color = isActivated ? new Color(0.2f, 1f, 0.6f, 1f) : new Color(1f, 0.9f, 0.5f, 1f);
        }
    }
}

