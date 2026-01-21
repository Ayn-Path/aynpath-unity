using System;
using UnityEngine;
using FlutterUnityIntegration;

public class UnityMessageManager : MonoBehaviour
{
    private static UnityMessageManager _instance;

    public static UnityMessageManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<UnityMessageManager>();
            return _instance;
        }
    }

    // Flutter → Unity
    public event Action<string> OnMessageReceivedFromFlutter;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Flutter → Unity
    public void OnMessage(string json)
    {
        Debug.Log("[Flutter → Unity] " + json);
        OnMessageReceivedFromFlutter?.Invoke(json);
    }

    // Unity → Flutter (OFFICIAL WAY)
    public void SendMessageToFlutter(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}")
            return;

        Debug.Log("[Unity → Flutter] " + json);

#if UNITY_ANDROID && !UNITY_EDITOR
        FlutterUnityIntegration.UnityMessageManager.Instance
            .SendMessageToFlutter(json);
#endif
    }
}