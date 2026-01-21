using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Stable NavMesh LineRenderer for AR Navigation
/// - No per-frame rendering
/// - Safe for Flutter scene switching
/// - World-space only (no camera influence)
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LineNavigator : MonoBehaviour
{
    // -------------------------
    // References
    // -------------------------
    private LineRenderer lineRenderer;
    private NavMeshPath navPath;

    // -------------------------
    // State
    // -------------------------
    private Vector3[] currentCorners = new Vector3[0];
    private bool hasValidPath = false;

    public string CurrentDestinationId { get; private set; }

    // -------------------------
    // Awake
    // -------------------------
    void Awake()
    {
        navPath = new NavMeshPath();
        lineRenderer = GetComponent<LineRenderer>();

        // LineRenderer config
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.12f;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        // URP-safe material
        if (lineRenderer.material == null ||
            !lineRenderer.material.shader.name.Contains("Universal"))
        {
            Material m = new Material(
                Shader.Find("Universal Render Pipeline/Unlit")
            );
            m.color = Color.yellow;
            lineRenderer.material = m;
        }

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.sortingOrder = 10;

        // ✅ FIX #1 (Flutter + AR): ensure line renders above AR background
        lineRenderer.material.renderQueue = 4000;
    }

    // -------------------------
    // Public API — Calculate + Draw Path
    // -------------------------
    public bool DrawPathBetween(Transform start, Transform end)
    {
        Debug.Log("[LINE DEBUG] DrawPathBetween CALLED");

        if (start == null || end == null)
        {
            Debug.LogWarning("[LineNavigator] Start or End is null");
            return false;
        }

        // ❌ FIX #2: DO NOT ClearPath() here
        // ClearPath();

        CurrentDestinationId = end.GetComponent<Node>()?.nodeId;

        // Ensure start/end are on NavMesh
        if (!NavMesh.SamplePosition(start.position, out NavMeshHit startHit, 1.5f, NavMesh.AllAreas))
        {
            Debug.LogWarning("[LineNavigator] Start not on NavMesh");
            return false;
        }

        if (!NavMesh.SamplePosition(end.position, out NavMeshHit endHit, 1.5f, NavMesh.AllAreas))
        {
            Debug.LogWarning("[LineNavigator] End not on NavMesh");
            return false;
        }

        bool found = NavMesh.CalculatePath(
            startHit.position,
            endHit.position,
            NavMesh.AllAreas,
            navPath
        );

        Debug.Log(
            "[LINE DEBUG] NavMesh found = " + found +
            ", corners = " + (navPath.corners != null ? navPath.corners.Length : 0)
        );

        if (!found || navPath.corners == null || navPath.corners.Length < 2)
        {
            Debug.LogWarning("[LineNavigator] No valid NavMesh path");
            return false;
        }

        // Cache corners
        currentCorners = (Vector3[])navPath.corners.Clone();
        hasValidPath = true;

        // Lift slightly above floor for AR
        for (int i = 0; i < currentCorners.Length; i++)
            currentCorners[i].y += 0.05f;

        // Render ONCE
        ApplyPathToLine();

        return true;
    }

    // -------------------------
    // Render line (ONE TIME ONLY)
    // -------------------------
    private void ApplyPathToLine()
    {
        if (!hasValidPath || currentCorners.Length < 2)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.positionCount = currentCorners.Length;
        lineRenderer.SetPositions(currentCorners);
        lineRenderer.enabled = true;

        Debug.Log(
            "[LINE DEBUG] ApplyPathToLine → enabled=" +
            lineRenderer.enabled +
            ", positions=" +
            lineRenderer.positionCount
        );
    }

    // -------------------------
    // Distance helper
    // -------------------------
    public float GetRemainingDistance(Vector3 fromPos)
    {
        if (!hasValidPath || currentCorners.Length < 2)
            return 0f;

        float dist = Vector3.Distance(fromPos, currentCorners[0]);

        for (int i = 0; i < currentCorners.Length - 1; i++)
            dist += Vector3.Distance(currentCorners[i], currentCorners[i + 1]);

        return dist;
    }

    // -------------------------
    // Clear path (CALLED FROM FLUTTER)
    // -------------------------
    public void ClearPath()
    {
        hasValidPath = false;
        currentCorners = new Vector3[0];
        CurrentDestinationId = null;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }

    // -------------------------
    // Debug / external access
    // -------------------------
    public Vector3[] GetCurrentPathCorners()
    {
        return (Vector3[])currentCorners.Clone();
    }
}