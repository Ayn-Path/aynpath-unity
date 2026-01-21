using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.AI;

public class FlutterUnityReceiver : MonoBehaviour
{
    [Header("References")]
    public NodeDatabase nodeDatabase;
    public LineNavigator lineNavigator;
    public Camera arCamera;
    public PathInstructionGenerator instructor;
    public ARSessionOrigin arOrigin;

    private bool _subscribed = false;

    // -------------------------
    // Unity lifecycle
    // -------------------------

    void Awake()
    {
        if (!nodeDatabase) nodeDatabase = FindObjectOfType<NodeDatabase>();
        if (!lineNavigator) lineNavigator = FindObjectOfType<LineNavigator>();
        if (!instructor) instructor = FindObjectOfType<PathInstructionGenerator>();
        if (!arCamera) arCamera = Camera.main;
        if (!arOrigin) arOrigin = FindObjectOfType<ARSessionOrigin>();
    }

    void OnEnable()
    {
        StartCoroutine(EnsureSubscribed());
    }

    void OnDisable()
    {
        if (UnityMessageManager.Instance != null)
        {
            UnityMessageManager.Instance.OnMessageReceivedFromFlutter -= OnFlutterMessage;
            Debug.Log("[FlutterUnityReceiver] Unsubscribed");
        }
        _subscribed = false;
    }

    // -------------------------
    // 🔴 CRITICAL FIX: delayed safe subscription
    // -------------------------

    private IEnumerator EnsureSubscribed()
    {
        while (UnityMessageManager.Instance == null)
            yield return null;

        if (_subscribed)
            yield break;

        UnityMessageManager.Instance.OnMessageReceivedFromFlutter -= OnFlutterMessage;
        UnityMessageManager.Instance.OnMessageReceivedFromFlutter += OnFlutterMessage;

        _subscribed = true;
        Debug.Log("[FlutterUnityReceiver] Subscribed to UnityMessageManager ✅");
    }

    // -------------------------
    // Flutter → Unity entry
    // -------------------------

    private void OnFlutterMessage(string json)
    {
        Debug.Log("[FlutterUnityReceiver] OnMessage: " + json);
        OnMessage(json);
    }

    public void OnMessage(string json)
    {
        Debug.Log("[Flutter→Unity] " + json);

        Command cmd;
        try
        {
            cmd = JsonUtility.FromJson<Command>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FlutterUnityReceiver] JSON parse error: " + ex);
            return;
        }

        if (cmd == null || string.IsNullOrEmpty(cmd.action))
            return;

        switch (cmd.action)
        {
            case "calibrate":
                StartCoroutine(RunWhenARReady(() => HandleCalibrate(cmd)));
                break;

            case "start_navigation":
                StartCoroutine(RunWhenARReady(() =>
                    StartNavigation(cmd.start, cmd.destination)));
                break;

            case "stop_navigation":
                StopNavigation();
                break;
            
            case "get_navigation_state":
                RespondNavigationState();
                break;

            default:
                Debug.LogWarning("[FlutterUnityReceiver] Unknown action: " + cmd.action);
                break;
        }
    }

    // -------------------------
    // AR READY GATE
    // -------------------------

    private IEnumerator RunWhenARReady(Action action)
    {
        ARSession session = null;
        while (session == null)
        {
            session = FindObjectOfType<ARSession>();
            yield return null;
        }

        while (!session.enabled)
            yield return null;

        while (ARSession.state != ARSessionState.SessionTracking)
            yield return null;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Debug.Log("[AR] RunWhenARReady → invoking action");
        action?.Invoke();
    }

    // -------------------------
    // Calibration
    // -------------------------

    private void HandleCalibrate(Command cmd)
    {
        if (string.IsNullOrEmpty(cmd.nodeId))
        {
            return;
        }

        // CalibrationHelper.ResetCalibration();
        CalibrationHelper.AlignNodeToCamera(cmd.nodeId, arCamera, arOrigin);
    }

    // -------------------------
    // Navigation
    // -------------------------

    private void StartNavigation(string startName, string destName)
    {
        Debug.Log($"[FlutterUnityReceiver] StartNavigation: {startName} -> {destName}");

        if (!CalibrationHelper.IsCalibrated)
            return;

        Node destNode = nodeDatabase.FindByNameOrId(destName);
        if (!destNode)
        {
            return;
        }

        Node startNode = nodeDatabase.FindByNameOrId(startName)
                         ?? nodeDatabase.NearestNode(arCamera.transform.position);

        StartCoroutine(StartNavigationWhenReady(startNode, destNode));
    }

    private IEnumerator StartNavigationWhenReady(Node startNode, Node destNode)
    {
        while (!NavMesh.SamplePosition(startNode.transform.position, out _, 1.5f, NavMesh.AllAreas))
            yield return null;

        Debug.Log("[NAV] NavMesh ready, calculating path");

        if (!lineNavigator.DrawPathBetween(startNode.transform, destNode.transform))
        {
            yield break;
        }

        instructor.StartNavigation(
            arOrigin.camera.transform,
            lineNavigator.GetCurrentPathCorners(),
            destNode.nodeId,
            -1f
        );
    }

    private void StopNavigation()
    {
        lineNavigator?.ClearPath();
        instructor?.StopNavigation();
        CalibrationHelper.ResetCalibration();
    }

    private void RespondNavigationState()
{
    var data = new NavigationStateDTO
    {
        instruction = NavigationState.Instruction,
        distance = NavigationState.RemainingDistance,
        arrived = NavigationState.Arrived
    };

    string json = JsonUtility.ToJson(data);
    UnityMessageManager.Instance?.SendMessageToFlutter(json);

    // IMPORTANT:
    // Do NOT push to Flutter
    // Flutter will receive this as the response to its poll
    Debug.Log("[NAV STATE] " + json);
}

    [Serializable]
public class NavigationStateDTO
{
    public string instruction;
    public float distance;
    public bool arrived;
}

[Serializable]
public class Command
{
    public string action;

    // calibration
    public string nodeId;

    // navigation
    public string start;
    public string destination;
}
}