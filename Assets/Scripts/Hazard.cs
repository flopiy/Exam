using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public enum HazardType { HorizontalPatrol, VerticalPatrol, StationaryRotate, StationaryPulse }

    [SerializeField] private HazardType hazardType = HazardType.StationaryPulse;
    [SerializeField] private float patrolDistance = 2.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 0f;

    private Vector2 startPos;
    private float moveProgress = 0f;
    private int moveDirection = 1;
    private SpriteRenderer sr;
    private float pulseTime = 0f;

    private void Awake()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        pulseTime += Time.deltaTime;

        if (rotationSpeed != 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if (hazardType == HazardType.StationaryPulse)
        {
            if (sr != null)
            {
                float pulse = 0.85f + Mathf.Sin(pulseTime * 4f + transform.position.x) * 0.15f;
                sr.color = new Color(1f, pulse, pulse, 1f);
            }
        }
        else if (hazardType == HazardType.HorizontalPatrol)
        {
            moveProgress += moveDirection * moveSpeed * Time.deltaTime;
            if (Mathf.Abs(moveProgress) >= patrolDistance)
            {
                moveDirection *= -1;
                moveProgress = Mathf.Clamp(moveProgress, -patrolDistance, patrolDistance);
            }
            transform.position = startPos + new Vector2(moveProgress, 0f);
        }
        else if (hazardType == HazardType.VerticalPatrol)
        {
            moveProgress += moveDirection * moveSpeed * Time.deltaTime;
            if (Mathf.Abs(moveProgress) >= patrolDistance)
            {
                moveDirection *= -1;
                moveProgress = Mathf.Clamp(moveProgress, -patrolDistance, patrolDistance);
            }
            transform.position = startPos + new Vector2(0f, moveProgress);
        }
    }

    public void Setup(HazardType type, float distance = 0f, float speed = 0f, float rotSpeed = 0f)
    {
        hazardType = type;
        patrolDistance = distance;
        moveSpeed = speed;
        rotationSpeed = rotSpeed;
        startPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RespawnToStart();
            }
        }
    }
}

