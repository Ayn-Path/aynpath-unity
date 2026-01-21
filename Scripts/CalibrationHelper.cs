using UnityEngine;
using UnityEngine.XR.ARFoundation;

public static class CalibrationHelper
{
    private static bool _isCalibrated;

    public static bool IsCalibrated => _isCalibrated;

    /// <summary>
    /// Aligns ARSessionOrigin so that the given nodeId matches the AR camera position & facing.
    /// MUST be called BEFORE path calculation.
    /// </summary>
    public static void AlignNodeToCamera(
        string nodeId,
        Camera arCamera,
        ARSessionOrigin arOrigin
    )
    {
        if (_isCalibrated)
        {
            Debug.Log("[CalibrationHelper] Already calibrated. Skipping.");
            return;
        }

        if (!arCamera || !arOrigin)
        {
            Debug.LogError("[CalibrationHelper] AR Camera or ARSessionOrigin is null.");
            return;
        }

        // Find node
        NodeDatabase db = Object.FindObjectOfType<NodeDatabase>();
        Node node = db?.FindNodeById(nodeId);

        if (!node)
        {
            Debug.LogError("[CalibrationHelper] Node not found: " + nodeId);
            return;
        }

        // =========================
        // 1️⃣ POSITION ALIGNMENT
        // =========================
        Vector3 camPos = arCamera.transform.position;
        Vector3 nodePos = node.transform.position;

        Vector3 offset = camPos - nodePos;
        offset.y = 0f; // keep floor level stable

        // 🔴 MOVE AR WORLD, NOT NAVMESH
        arOrigin.transform.position += offset;

        // =========================
        // 2️⃣ ROTATION ALIGNMENT
        // =========================
        Vector3 camForward = arCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 nodeForward = node.transform.forward;
        nodeForward.y = 0f;
        nodeForward.Normalize();

        Quaternion deltaRotation =
            Quaternion.FromToRotation(nodeForward, camForward);

        // 🔴 ROTATE AR WORLD, NOT NAVMESH
        arOrigin.transform.rotation =
            deltaRotation * arOrigin.transform.rotation;

        _isCalibrated = true;

        Debug.Log(
            $"[CalibrationHelper] Calibrated ARSessionOrigin using node '{nodeId}'.\n" +
            $"Distance check: {Vector3.Distance(arCamera.transform.position, node.transform.position):F3} m"
        );
    }

    /// <summary>
    /// Call this BEFORE starting a new navigation session.
    /// </summary>
    public static void ResetCalibration()
    {
        _isCalibrated = false;
        Debug.Log("[CalibrationHelper] Calibration reset.");
    }
}
