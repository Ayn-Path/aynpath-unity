using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARPermissionAndSessionFix : MonoBehaviour
{
    IEnumerator Start()
    {
        // Request camera permission EARLY
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);

            // Wait until user responds
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                yield return null;
        }

        Debug.Log("Camera permission granted.");

        // Give Android time to release camera
        yield return new WaitForSeconds(0.5f);

        // Now start ARSession
        yield return ARSession.CheckAvailability();

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
        }

        Debug.Log("ARSession final state: " + ARSession.state);
    }
}