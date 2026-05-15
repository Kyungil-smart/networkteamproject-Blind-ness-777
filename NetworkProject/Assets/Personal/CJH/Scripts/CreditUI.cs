using UnityEngine;

public class CreditUI : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
            gameObject.SetActive(false);
    }
}