using UnityEngine;

public class EditorTestInvoker : MonoBehaviour
{
    void Start()
    {
        var receiver = FindAnyObjectByType<FlutterUnityReceiver>();

        Debug.Log("[EDITOR TEST] Triggering navigation without AR");

        receiver.OnMessage(
            "{ \"action\": \"start_navigation\", \"start\": \"Cafeteria\", \"destination\": \"Musolla\" }"
        );
    }
}