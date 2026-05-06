using System;
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

    public void Hide()
    {
        foreach (var r in allRenderers) r.enabled = false;
        foreach (var c in allColliders) c.enabled = false;
    }

    public void Show()
    {
        foreach (var r in allRenderers) r.enabled = true;
        foreach (var c in allColliders) c.enabled = true;
    }

    public void AIDestroy() => Destroy(gameObject);
}
