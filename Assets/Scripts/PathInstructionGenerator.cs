using UnityEngine;
using System;

public class PathInstructionGenerator : MonoBehaviour
{
    // -------------------------
    // Public tuning parameters
    // -------------------------
    public Transform user;
    public float turnThresholdDeg = 25f;
    public float defaultArriveDistance = 1.2f;
    public float updateInterval = 0.25f;
    public float advanceCornerDistance = 1.0f;

    [Header("Distance update control")]
    public float distanceUpdateStep = 5f;

    [Header("Anti flip-flop stabilization")]
    public float hysteresisAngle = 10f;
    public float flipDebounceTime = 0.4f;

    [Header("Quick workaround (flip left/right if your instructions are reversed)")]
    public bool invertTurnDirections = true;

    // -------------------------
    // Internal state
    // -------------------------
    private Vector3[] corners;
    private int currentIndex;
    private float timer;
    private float arriveDistance;
    private bool running;
    private string lastInstructionSent;
    private float lastDistanceSentValue;

    private string pendingInstruction;
    private float pendingSince;

    // Guards
    private string activeDestinationId;
    private int activeCornerHash;

    // -------------------------
    // Unity lifecycle
    // -------------------------

    void Awake()
    {
        HardReset();
    }

    void Update()
    {
        if (!running || corners == null || corners.Length < 2 || user == null)
            return;

        timer += Time.deltaTime;
        if (timer < updateInterval)
            return;

        timer = 0f;
        GenerateInstruction();
    }

    // -------------------------
    // PUBLIC API
    // -------------------------

    public void StartNavigation(
        Transform userCamera,
        Vector3[] navCorners,
        string destNodeId,
        float arriveDist = -1f)
    {
        if (userCamera == null || navCorners == null || navCorners.Length < 2)
            return;

        int newHash = ComputeCornerHash(navCorners);

        // Prevent restarting same navigation
        if (running &&
            destNodeId == activeDestinationId &&
            newHash == activeCornerHash)
            return;

        // -------- Initialize NEW session --------
        user = userCamera;
        corners = (Vector3[])navCorners.Clone();

        activeDestinationId = destNodeId;
        activeCornerHash = newHash;

        arriveDistance = (arriveDist > 0f) ? arriveDist : defaultArriveDistance;

        currentIndex = 1;
        timer = 0f;
        running = true;

        lastInstructionSent = null;
        lastDistanceSentValue = -1f;
        pendingInstruction = null;
        pendingSince = 0f;

        Debug.Log("[PathInstructionGenerator] Navigation started. Dest=" + destNodeId);

        GenerateInstruction(forceSend: true);
    }

    public void StopNavigation()
{
    if (!running)
        return;

    HardReset();
}

    // -------------------------
    // CORE LOGIC (unchanged)
    // -------------------------

    private void GenerateInstruction(bool forceSend = false)
    {
        Vector3 userFlat = new Vector3(user.position.x, 0f, user.position.z);
        float remaining = ComputeRemainingDistance(userFlat);

        Vector3 destFlat = new Vector3(
            corners[corners.Length - 1].x,
            0f,
            corners[corners.Length - 1].z
        );

        if (Vector3.Distance(userFlat, destFlat) <= arriveDistance)
        {
            SendEventImmediate("arrived", "You have arrived at your destination.");
            SendEventImmediate("distance_update", "0.0");
            //HardReset();
            return;
        }

        if (currentIndex < corners.Length - 1)
        {
            Vector3 c = corners[currentIndex];
            if (Vector3.Distance(userFlat, new Vector3(c.x, 0f, c.z)) < advanceCornerDistance)
            {
                currentIndex++;
                pendingInstruction = null;
            }
        }

        Vector3 nextFlat = new Vector3(
            corners[currentIndex].x,
            0f,
            corners[currentIndex].z
        );

        float distToNext = Vector3.Distance(userFlat, nextFlat);

        Vector3 forwardFlat = Vector3.ProjectOnPlane(user.forward, Vector3.up).normalized;
        Vector3 toNextFlat = (nextFlat - userFlat).normalized;

        float angle = Vector3.SignedAngle(forwardFlat, toNextFlat, Vector3.up);

        string instruction;
        if (Mathf.Abs(angle) < turnThresholdDeg)
        {
            instruction = $"Walk straight for {distToNext:0.0} meters.";
        }
        else
        {
            bool left = angle > 0f;
            if (invertTurnDirections) left = !left;
            instruction = left
                ? $"Turn left in {distToNext:0.0} meters."
                : $"Turn right in {distToNext:0.0} meters.";
        }

        if (forceSend)
        {
            SendInstructionNow(instruction, remaining);
            return;
        }

        if (!string.Equals(instruction, lastInstructionSent))
        {
            if (pendingInstruction != instruction)
            {
                pendingInstruction = instruction;
                pendingSince = Time.time;
            }

            if ((Time.time - pendingSince) >= flipDebounceTime)
            {
                SendInstructionNow(instruction, remaining);
                pendingInstruction = null;
            }
        }

        if (lastDistanceSentValue < 0f ||
            (lastDistanceSentValue - remaining) >= distanceUpdateStep)
        {
            SendEventImmediate("distance_update", remaining.ToString("0.0"));
            lastDistanceSentValue = remaining;
        }
    }

    // -------------------------
    // HELPERS
    // -------------------------

    private void HardReset()
    {
        running = false;
        user = null;
        corners = null;
        currentIndex = 0;
        timer = 0f;

        activeDestinationId = null;
        activeCornerHash = 0;

        lastInstructionSent = null;
        lastDistanceSentValue = -1f;
        pendingInstruction = null;
        pendingSince = 0f;

        NavigationState.Instruction = "";
        NavigationState.RemainingDistance = -1f;
        NavigationState.Arrived = false;
    }

    private int ComputeCornerHash(Vector3[] pts)
    {
        unchecked
        {
            int hash = 17;
            foreach (var p in pts)
                hash = hash * 23 + p.GetHashCode();
            return hash;
        }
    }

    private float ComputeRemainingDistance(Vector3 userFlat)
    {
        int idx = Mathf.Clamp(currentIndex, 0, corners.Length - 1);
        float total = Vector3.Distance(userFlat,
            new Vector3(corners[idx].x, 0f, corners[idx].z));

        for (int i = idx; i < corners.Length - 1; i++)
        {
            Vector3 a = new Vector3(corners[i].x, 0f, corners[i].z);
            Vector3 b = new Vector3(corners[i + 1].x, 0f, corners[i + 1].z);
            total += Vector3.Distance(a, b);
        }
        return total;
    }

    private void SendInstructionNow(string instruction, float remaining)
    {
        SendEventImmediate("instruction", instruction);
        SendEventImmediate("distance_update", remaining.ToString("0.0"));
        lastInstructionSent = instruction;
        lastDistanceSentValue = remaining;
    }

private void SendEventImmediate(string type, string message)
{
    Debug.Log($"[SendEventImmediate] type={type}, msg={message}");

    if (type == "instruction")
    {
        NavigationState.Instruction = message;
        Debug.Log("[NAV STATE] Instruction set");
    }
    else if (type == "distance_update")
    {
        if (float.TryParse(message, out float d))
        {
            NavigationState.RemainingDistance = d;
            Debug.Log("[NAV STATE] Distance set: " + d);
        }
    }
    else if (type == "arrived")
    {
        NavigationState.Arrived = true;
        Debug.Log("[NAV STATE] Arrived set TRUE");
    }
}
}