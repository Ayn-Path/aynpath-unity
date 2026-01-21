using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSessionDelayedStart : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait for Flutter SurfaceView to be fully ready
        yield return new WaitForSeconds(0.5f);

        var session = FindObjectOfType<ARSession>();
        if (session == null)
        {
            Debug.LogError("[AR] ARSession not found");
            yield break;
        }

        // 🔴 FORCE RESET (THIS IS CRITICAL)
        session.enabled = false;
        yield return null;
        yield return null;

        session.enabled = true;
        Debug.Log("[AR] ARSession reset & enabled");

        // 🔴 WAIT FOR REAL TRACKING
        while (ARSession.state != ARSessionState.SessionTracking)
        {
            yield return null;
        }

        Debug.Log("[AR] SessionTracking confirmed");
        UnityMessageManager.Instance?.SendMessageToFlutter(
            JsonUtility.ToJson(new { eventType = "ar_ready", message = "ok" })
        );
    }
}