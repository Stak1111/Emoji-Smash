using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class AIGrappleSpinner : MonoBehaviour
{
    [Header("Grappling Settings")]
    public float grappleRadius = 10f;
    public float minRopeLength = 2f;
    public LayerMask grappleLayerMask;

    [Header("Spin Settings")]
    public float rotationAcceleration = 5f;
    public float maxSpinSpeed = 20f;
    private float spinSpeedBreakpoint = 40f;

    [Header("Release Logic")]
    [Tooltip("Maximum angle in degrees between AI's velocity and the player direction for a valid release.")]
    public float releaseAngleThreshold = 10f;

    [Header("Grapple Lockout")]
    public float grappleLockoutTime = 1f;

    [Header("Grapple Duration")]
    public float minGrappleTime = 2f;
    public float maxGrappleTime = 5f;

    public Transform playerTarget;

    private Rigidbody2D rb;
    private DistanceJoint2D joint;
    private Collider2D grappleTarget;
    private LineRenderer lineRenderer;

    private bool isGrappling = false;
    private float currentSpinSpeed = 0f;
    private int spinDirection = 1;
    private bool breakpointTriggered = false;

    private bool grappleLocked = false;
    private float grappleLockTimer = 0f;

    private float initialGrappleDelay;
    private float timeSinceStart = 0f;

    private float grappleTimer = 0f;
    private float grappleDuration = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        initialGrappleDelay = Random.Range(0.3f, 1.2f); // adds natural variability
    }

    void Update()
    {
        timeSinceStart += Time.deltaTime;
        if (timeSinceStart < initialGrappleDelay)
            return;

        // Safety check — if player is missing, stop grappling and skip
        if (playerTarget == null)
        {
            if (isGrappling)
                ReleaseGrapple();
            return;
        }

        if (grappleLocked)
        {
            grappleLockTimer -= Time.deltaTime;
            if (grappleLockTimer <= 0f)
                grappleLocked = false;
        }

        if (isGrappling)
        {
            // Safety check — if grapple anchor is gone, release
            if (joint == null)
            {
                ReleaseGrapple();
                return;
            }

            AccelerateSpin();
            UpdateRope();

            grappleTimer += Time.deltaTime;

            // Direction toward player
            Vector2 toPlayer = (playerTarget.position - transform.position).normalized;
            float angleToPlayer = Vector2.Angle(rb.linearVelocity.normalized, toPlayer);

            bool shouldRelease = false;

            // Time-based auto-release
            if (grappleTimer >= grappleDuration && !breakpointTriggered)
            {
                if (angleToPlayer <= releaseAngleThreshold)
                {
                    shouldRelease = true;
                    Debug.Log($"{gameObject.name} | Timed release toward player.");
                }
                else
                {
                    Debug.Log($"{gameObject.name} | Waiting for better release angle ({angleToPlayer:F1}°).");
                }
            }

            // Spin speed breakpoint release
            Vector2 toAnchor = joint.connectedAnchor - rb.position;
            Vector2 tangent = Vector2.Perpendicular(toAnchor).normalized * spinDirection;
            float tangentialSpeed = Vector2.Dot(rb.linearVelocity, tangent);

            if (!breakpointTriggered && Mathf.Abs(tangentialSpeed) >= spinSpeedBreakpoint)
            {
                if (angleToPlayer <= releaseAngleThreshold)
                {
                    shouldRelease = true;
                    breakpointTriggered = true;
                    Debug.Log($"{gameObject.name} | Spin breakpoint release toward player.");
                }
                else
                {
                    Debug.Log($"{gameObject.name} | Spin breakpoint hit but waiting for better angle.");
                }
            }

            if (shouldRelease)
            {
                TriggerGrappleLockout();
                ReleaseGrapple();
                return;
            }
        }
        else
        {
            if (!grappleLocked)
                TryAutoGrapple();

            if (lineRenderer.positionCount != 0)
                lineRenderer.positionCount = 0;

            breakpointTriggered = false;
        }
    }

    void TryAutoGrapple()
    {
        if (grappleLocked || playerTarget == null) return;

        Vector2 aiPos = rb.position;
        Vector2 toPlayer = (playerTarget.position - transform.position).normalized;

        Collider2D[] all = Physics2D.OverlapCircleAll(aiPos, grappleRadius, grappleLayerMask);

        float closestDist = float.MaxValue;
        Collider2D closest = null;

        foreach (Collider2D col in all)
        {
            if (col.isTrigger && col != GetComponent<Collider2D>())
            {
                Vector2 toPoint = ((Vector2)col.ClosestPoint(aiPos) - aiPos).normalized;
                float angle = Vector2.Angle(toPlayer, toPoint);

                if (angle < 90f) // must be somewhat toward player
                {
                    float dist = Vector2.Distance(col.ClosestPoint(aiPos), aiPos);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = col;
                    }
                }
            }
        }

        if (closest != null)
        {
            grappleTarget = closest;

            joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = null;
            joint.connectedAnchor = grappleTarget.ClosestPoint(aiPos);
            joint.autoConfigureConnectedAnchor = false;
            joint.enableCollision = false;

            float actualDistance = Vector2.Distance(aiPos, joint.connectedAnchor);
            joint.distance = Mathf.Max(actualDistance, minRopeLength);

            isGrappling = true;

            Vector2 toAnchor = joint.connectedAnchor - aiPos;
            Vector2 tangent = Vector2.Perpendicular(toAnchor).normalized;
            spinDirection = Vector2.Dot(rb.linearVelocity, tangent) >= 0 ? 1 : -1;

            lineRenderer.positionCount = 2;

            grappleTimer = 0f;
            grappleDuration = Random.Range(minGrappleTime, maxGrappleTime);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} ⚠ No valid grapple point found near self toward player.");
        }
    }

    void AccelerateSpin()
    {
        if (joint == null) return;

        Vector2 toAnchor = joint.connectedAnchor - rb.position;
        Vector2 tangent = Vector2.Perpendicular(toAnchor).normalized * spinDirection;

        currentSpinSpeed = Mathf.Min(currentSpinSpeed + rotationAcceleration * Time.deltaTime, maxSpinSpeed);
        rb.AddForce(tangent * currentSpinSpeed, ForceMode2D.Force);
    }

    void ReleaseGrapple()
    {
        if (joint != null)
            Destroy(joint);

        isGrappling = false;
        grappleTarget = null;
        currentSpinSpeed = 0f;
        breakpointTriggered = false;

        if (lineRenderer.positionCount != 0)
            lineRenderer.positionCount = 0;

        Debug.Log($"{gameObject.name} | Grapple released.");
    }

    void TriggerGrappleLockout()
    {
        grappleLocked = true;
        grappleLockTimer = grappleLockoutTime;
        Debug.Log($"{gameObject.name} | Grapple locked out for {grappleLockoutTime} seconds.");
    }

    void UpdateRope()
    {
        if (joint == null || lineRenderer == null) return;

        if (lineRenderer.positionCount < 2)
            lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, joint.connectedAnchor);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGrappling)
        {
            Debug.Log($"{gameObject.name} | Collision detected - breaking grapple.");
            TriggerGrappleLockout();
            ReleaseGrapple();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rb.position, grappleRadius);
        }
    }
}
