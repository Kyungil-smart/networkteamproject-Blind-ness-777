using System;
using UnityEngine;

public class AISetActive : MonoBehaviour
{
    public void Hide() => gameObject.SetActive(false);
    
    public void Show() => gameObject.SetActive(true);

    public void AIDestroy() => Destroy(gameObject);
}
