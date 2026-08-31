using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Sprites & Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite downIdle;
    [SerializeField] private Sprite[] downSprites;
    [SerializeField] private Sprite upIdle;
    [SerializeField] private Sprite[] upSprites;
    [SerializeField] private Sprite sideIdle;
    [SerializeField] private Sprite[] sideSprites;
    [SerializeField] private float animSpeed = 10f;
    private float animTimer = 0f;
    private int facingDir = 0; // 0 = Down, 1 = Up, 2 = Right, 3 = Left

    [Header("Respawn")]
    [SerializeField] private Vector2 startPosition;
    private bool isInvulnerable = false;
    private float invulnerableTimer = 0f;

    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 3.2f;
    private Generator currentNearbyGenerator = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        startPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            moveInput = Vector2.zero;
            return;
        }

        HandleInput();
        UpdateAnimation();
        CheckNearbyInteractables();
        HandleInteraction();
        HandleInvulnerabilityVisuals();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void HandleInput()
    {
        moveInput = Vector2.zero;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveInput.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveInput.y -= 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveInput.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput.x += 1f;
        }

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput = moveInput.normalized;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                facingDir = (moveInput.x > 0) ? 2 : 3;
            }
            else
            {
                facingDir = (moveInput.y > 0) ? 1 : 0;
            }
        }
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        Sprite idleSprite = downIdle;
        Sprite[] walkArray = downSprites;
        bool flipX = false;

        switch (facingDir)
        {
            case 0: // Down
                idleSprite = downIdle != null ? downIdle : (downSprites != null && downSprites.Length > 0 ? downSprites[0] : null);
                walkArray = downSprites;
                flipX = false;
                break;
            case 1: // Up
                idleSprite = upIdle != null ? upIdle : (upSprites != null && upSprites.Length > 0 ? upSprites[0] : null);
                walkArray = upSprites;
                flipX = false;
                break;
            case 2: // Right
                idleSprite = sideIdle != null ? sideIdle : (sideSprites != null && sideSprites.Length > 0 ? sideSprites[0] : null);
                walkArray = sideSprites;
                flipX = false;
                break;
            case 3: // Left
                idleSprite = sideIdle != null ? sideIdle : (sideSprites != null && sideSprites.Length > 0 ? sideSprites[0] : null);
                walkArray = sideSprites;
                flipX = true;
                break;
        }

        if (isMoving && walkArray != null && walkArray.Length > 0)
        {
            animTimer += Time.deltaTime * animSpeed;
            int frame = (int)animTimer % walkArray.Length;
            if (walkArray[frame] != null)
            {
                spriteRenderer.sprite = walkArray[frame];
            }
        }
        else
        {
            animTimer = 0f;
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
            else if (walkArray != null && walkArray.Length > 0 && walkArray[0] != null)
            {
                spriteRenderer.sprite = walkArray[0];
            }
        }

        spriteRenderer.flipX = flipX;
    }

    private void CheckNearbyInteractables()
    {
        // Zero-allocation generator distance check
        Generator nearestGen = null;
        float minDistSq = interactionRadius * interactionRadius;
        Vector2 myPos = transform.position;

        var allGens = Generator.AllGenerators;
        for (int i = 0; i < allGens.Count; i++)
        {
            var gen = allGens[i];
            if (gen != null)
            {
                Vector2 genPos = gen.transform.position;
                float distSq = (myPos - genPos).sqrMagnitude;
                if (distSq <= minDistSq)
                {
                    minDistSq = distSq;
                    nearestGen = gen;
                }
            }
        }

        currentNearbyGenerator = nearestGen;

        if (UIManager.Instance != null)
        {
            if (currentNearbyGenerator != null)
            {
                if (currentNearbyGenerator.IsActivated)
                {
                    UIManager.Instance.ShowPrompt($"Generator #{currentNearbyGenerator.SequenceIndex} ({currentNearbyGenerator.ColorName}) is ONLINE");
                }
                else
                {
                    UIManager.Instance.ShowPrompt($"Press [E] to Activate Generator #{currentNearbyGenerator.SequenceIndex} ({currentNearbyGenerator.ColorName})");
                }
            }
            else
            {
                UIManager.Instance.HidePrompt();
            }
        }
    }

    private void HandleInteraction()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            if (currentNearbyGenerator != null)
            {
                GameManager.Instance.TryActivateGenerator(currentNearbyGenerator);
            }
        }
    }

    public void RespawnToStart()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        isInvulnerable = true;
        invulnerableTimer = 0.8f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHazard();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("OUCH! Hit Hazard - Returned to Start!", new Color(1f, 0.35f, 0.35f, 1f));
        }
    }

    private void HandleInvulnerabilityVisuals()
    {
        if (isInvulnerable)
        {
            invulnerableTimer -= Time.deltaTime;
            if (spriteRenderer != null)
            {
                float alpha = (Mathf.Sin(Time.time * 30f) > 0) ? 0.3f : 1f;
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f, alpha);
            }

            if (invulnerableTimer <= 0f)
            {
                isInvulnerable = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }
    }

    public void SetupSprites(Sprite dIdle, Sprite[] down, Sprite uIdle, Sprite[] up, Sprite sIdle, Sprite[] side)
    {
        downIdle = dIdle;
        downSprites = down;
        upIdle = uIdle;
        upSprites = up;
        sideIdle = sIdle;
        sideSprites = side;
    }

    public void SetupSprites(Sprite[] down, Sprite[] up, Sprite[] side)
    {
        downSprites = down;
        upSprites = up;
        sideSprites = side;
    }

    public void SetStartPosition(Vector2 pos)
    {
        startPosition = pos;
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}


