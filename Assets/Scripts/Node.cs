using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Represents a fixed navigation node in the world.
/// This script is DATA-ONLY at runtime.
/// </summary>
public class Node : MonoBehaviour
{
    // -------------------------
    // Identification
    // -------------------------
    [Header("Identification")]
    [Tooltip("Unique ID for this navigation point. No spaces! Example: N_Cafe")]
    public string nodeId = "N_0";

    [Tooltip("Friendly name for linking with Flutter (e.g., 'Cafeteria')")]
    public string displayName = "Unnamed";

    // -------------------------
    // Routing
    // -------------------------
    [Header("Routing")]
    [Tooltip("Optional waypoint children for multi-step navigation")]
    public List<Transform> waypoints = new List<Transform>();

    // -------------------------
    // Runtime access (READ-ONLY)
    // -------------------------
    public Vector3 WorldPosition { get; private set; }

    // -------------------------
    // Unity lifecycle
    // -------------------------

    /// <summary>
    /// Awake is called once when the object is initialized.
    /// Safe place to cache immutable data.
    /// </summary>
    void Awake()
    {
        WorldPosition = transform.position;
    }

    /// <summary>
    /// If the node is moved at runtime (rare, but possible),
    /// keep WorldPosition in sync.
    /// </summary>
    void OnValidate()
    {
        WorldPosition = transform.position;
    }

#if UNITY_EDITOR
    // -------------------------
    // Editor visualization only
    // -------------------------
    [Header("Gizmo Visualization")]
    public float gizmoOffset = 0.08f;
    public string iconChildName = "IconQuad";

    void OnDrawGizmos()
    {
        Vector3 gizmoPos = transform.position;

        // Try 1: renderer bounds
        Renderer childRenderer = GetComponentInChildren<Renderer>();
        if (childRenderer != null && childRenderer.gameObject != gameObject)
        {
            gizmoPos = childRenderer.bounds.center;
        }
        else
        {
            // Try 2: named icon child
            Transform iconChild = transform.Find(iconChildName);
            if (iconChild != null)
            {
                gizmoPos = iconChild.position;
            }
            else
            {
                // Try 3: fallback
                gizmoPos = transform.position + Vector3.up * gizmoOffset;
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(gizmoPos, 0.06f);

        Handles.color = Color.white;
        Handles.Label(gizmoPos + Vector3.up * 0.15f, nodeId);
    }
#endif
}