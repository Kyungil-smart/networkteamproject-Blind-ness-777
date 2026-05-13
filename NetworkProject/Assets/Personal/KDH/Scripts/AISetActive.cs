using System;
using Unity.Netcode;
using UnityEngine;

public class AISetActive : MonoBehaviour
{
    private Renderer[] allRenderers;
    private Collider[] allColliders;

    private void Awake()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
        allColliders = GetComponentsInChildren<Collider>();
    }

    [ClientRpc]
    public void HideClientRPC()
    {
        foreach (var r in allRenderers) r.enabled = false;
        foreach (var c in allColliders) c.enabled = false;
    }

    [ClientRpc]
    public void ShowClientRPC()
    {
        foreach (var r in allRenderers) r.enabled = true;
        foreach (var c in allColliders) c.enabled = true;
    }

    [ClientRpc]
    public void AIDestroyClientRPC() => Destroy(gameObject);
}
