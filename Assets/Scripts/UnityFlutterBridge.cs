using UnityEngine;

public class UnityFlutterBridge : MonoBehaviour
{
    // This method name is REQUIRED by flutter_unity_widget 2022.2.1
    public void OnUnityMessage(string message)
    {
        // Do nothing here.
        // flutter_unity_widget intercepts this call internally.
        Debug.Log("[UnityFlutterBridge] Forwarded to Flutter: " + message);
    }
}
