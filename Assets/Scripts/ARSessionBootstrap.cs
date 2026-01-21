using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARSessionBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("ARSessionBootstrap: Checking availability...");
        yield return ARSession.CheckAvailability();

        Debug.Log("ARSessionBootstrap: State after check = " + ARSession.state);

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            Debug.Log("ARSessionBootstrap: Installing ARCore...");
            yield return ARSession.Install();
        }

        Debug.Log("ARSessionBootstrap: Final state = " + ARSession.state);
    }
}