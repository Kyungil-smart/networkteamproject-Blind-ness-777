using Unity.Netcode;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("Start Host"))
            NetworkManager.Singleton.StartHost();
    }
}