using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class SwingController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 1; // 1–4, set in prefab or when spawning

    [Header("Grappling Settings")]
    public float grappleRadius = 10f;
    public float minRopeLength = 1f;
    public LayerMask grappleLayerMask;

    [Header("Spin Settings")]
    public float rotationAcceleration = 4f;
    public float maxSpinSpeed = 500f;

    [Header("Break Conditions")]
    public bool breakOnContact = true;
    public bool breakOnSpinForce = false;
    public float spinForceBreakThreshold = 50f;

    [Header("Cooldown Settings")]
    public float grappleLockoutTime = 0.6f; // seconds
    private float grappleCooldownTimer = 0f;

    private Rigidbody2D rb;
    private DistanceJoint2D joint;
    private LineRenderer lineRenderer;

    private bool isGrappling = false;
    private float currentSpinSpeed = 0f;
    private int spinDirection = 1;

    private PlayerInputActions inputActions;

    // Movement history
    private const int historyLength = 5;
    private Queue<Vector2> positionHistory = new Queue<Vector2>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        inputActions = new PlayerInputActions();
        BindPlayerInput();
    }

    void OnEnable()
    {
        if (inputActions != null)
            inputActions.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
    }

    void OnDestroy()
    {
        UnbindPlayerInput();
        if (inputActions != null)
        {
            inputActions.Dispose();
            inputActions = null;
        }
    }

    private void BindPlayerInput()
    {
        UnbindPlayerInput(); // avoid double binding

        switch (playerIndex)
        {
            case 1:
                inputActions.Player1.Tap.performed += OnTapPerformed;
                break;
            case 2:
                inputActions.Player2.Tap.performed += OnTapPerformed;
                break;
            case 3:
                inputActions.Player3.Tap.performed += OnTapPerformed;
                break;
            case 4:
                inputActions.Player4.Tap.performed += OnTapPerformed;
                break;
        }
    }

    private void UnbindPlayerInput()
    {
        if (inputActions == null) return;

        inputActions.Player1.Tap.performed -= OnTapPerformed;
        inputActions.Player2.Tap.performed -= OnTapPerformed;
        inputActions.Player3.Tap.performed -= OnTapPerformed;
        inputActions.Player4.Tap.performed -= OnTapPerformed;
    }

    private void OnTapPerformed(InputAction.CallbackContext context)
    {
        ToggleGrapple();
    }

    void Update()
    {
        if (grappleCooldownTimer > 0)
            grappleCooldownTimer -= Time.deltaTime;

        if (isGrappling)
        {
            if (breakOnSpinForce && rb.linearVelocity.magnitude > spinForceBreakThreshold)
            {
                ReleaseGrapple();
                return;
            }

            AccelerateSpin();
            UpdateRope();
        }
        else if (lineRenderer.positionCount != 0)
        {
            lineRenderer.positionCount = 0;
        }
    }

    void LateUpdate()
    {
        if (positionHistory.Count >= historyLength)
            positionHistory.Dequeue();

        positionHistory.Enqueue(rb.position);
    }

    void ToggleGrapple()
    {
        if (isGrappling)
            ReleaseGrapple();
        else
            TryStartGrapple();
    }

    void TryStartGrapple()
    {
        if (grappleCooldownTimer > 0)
            return;

        Vector2 pos = rb.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, grappleRadius, grappleLayerMask);

        Collider2D bestAnchor = null;
        float bestScore = float.NegativeInfinity;
        Vector2 inputDir = Vector2.up;

        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger && col != GetComponent<Collider2D>())
            {
                Vector2 toAnchor = col.ClosestPoint(pos) - pos;
                float align = Vector2.Dot(toAnchor.normalized, inputDir);

                float distance = toAnchor.magnitude;
                float normalizedDistance = Mathf.Clamp01(distance / grappleRadius);

                float distanceWeight = 0.7f;
                float alignmentWeight = 0.3f;

                float score = alignmentWeight * align + distanceWeight * (1f - normalizedDistance);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAnchor = col;
                }
            }
        }

        if (bestAnchor != null)
        {
            joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedAnchor = bestAnchor.ClosestPoint(pos);
            joint.autoConfigureConnectedAnchor = false;
            joint.enableCollision = false;

            float dist = Vector2.Distance(pos, joint.connectedAnchor);
            joint.distance = Mathf.Max(dist, minRopeLength);

            isGrappling = true;

            Vector2 toAnchor = joint.connectedAnchor - rb.position;
            Vector2 tangent = Vector2.Perpendicular(toAnchor).normalized;
            spinDirection = Vector2.Dot(rb.linearVelocity, tangent) >= 0 ? 1 : -1;

            lineRenderer.positionCount = 2;
        }
    }

    void AccelerateSpin()
    {
        if (joint == null) return;

        Vector2 toAnchor = joint.connectedAnchor - rb.position;
        Vector2 tangent = Vector2.Perpendicular(toAnchor).normalized * spinDirection;

        currentSpinSpeed = Mathf.Min(currentSpinSpeed + rotationAcceleration * Time.deltaTime, maxSpinSpeed);
        rb.AddForce(tangent * currentSpinSpeed, ForceMode2D.Force); // continuous force
    }

    void ReleaseGrapple()
    {
        if (joint != null)
            Destroy(joint);

        isGrappling = false;
        currentSpinSpeed = 0f;
        grappleCooldownTimer = grappleLockoutTime;

        if (lineRenderer.positionCount != 0)
            lineRenderer.positionCount = 0;
    }

    void UpdateRope()
    {
        if (joint == null || lineRenderer == null) return;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, joint.connectedAnchor);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (breakOnContact && (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle")))
        {
            ReleaseGrapple();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rb.position, grappleRadius);
        }
    }
}
