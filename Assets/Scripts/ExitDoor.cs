using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitDoor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private Collider2D barrierCollider;
    [SerializeField] private TextMesh textLabel;

    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite openSprite;

    private bool isOpen = false;
    private float pulseTime = 0f;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (doorRenderer == null) doorRenderer = GetComponent<SpriteRenderer>();
        if (barrierCollider == null) barrierCollider = GetComponent<Collider2D>();
        if (textLabel == null) textLabel = GetComponentInChildren<TextMesh>();

        SetLocked();
    }

    private void Update()
    {
        pulseTime += Time.deltaTime;
        if (isOpen && doorRenderer != null)
        {
            float pulse = 0.85f + Mathf.Sin(pulseTime * 6f) * 0.15f;
            doorRenderer.color = new Color(1f, 1f, 1f, pulse);
        }
    }

    public void SetupSprites(Sprite locked, Sprite open)
    {
        lockedSprite = locked;
        openSprite = open;
        SetLocked();
    }

    public void SetLocked()
    {
        isOpen = false;
        if (doorRenderer != null)
        {
            if (lockedSprite != null) doorRenderer.sprite = lockedSprite;
            doorRenderer.color = Color.white;
        }
        if (barrierCollider != null) barrierCollider.isTrigger = false;
        if (textLabel != null)
        {
            textLabel.text = "EXIT LOCKED\n[0/4 Online]";
            textLabel.color = new Color(1f, 0.4f, 0.3f, 1f);
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        if (doorRenderer != null)
        {
            if (openSprite != null) doorRenderer.sprite = openSprite;
            doorRenderer.color = Color.white;
        }
        if (barrierCollider != null) barrierCollider.isTrigger = true;
        if (textLabel != null)
        {
            textLabel.text = "EXIT OPEN!\n[ESCAPE HERE]";
            textLabel.color = new Color(0.2f, 1f, 0.6f, 1f);
        }
    }

    public void UpdateProgress(int current, int total)
    {
        if (!isOpen && textLabel != null)
        {
            textLabel.text = $"EXIT LOCKED\n[{current}/{total} Online]";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpen && other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerWin();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isOpen && collision.gameObject.CompareTag("Player"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification("Exit is locked! Activate all 4 generators in sequence.", new Color(1f, 0.7f, 0.2f, 1f));
            }
        }
    }
}

