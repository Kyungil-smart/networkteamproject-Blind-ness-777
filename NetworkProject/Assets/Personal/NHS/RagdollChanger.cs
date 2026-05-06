using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class RagdollChanger : NetworkBehaviour
{
    [SerializeField] public GameObject charObj;
    [SerializeField] public GameObject ragdollObj;

    [SerializeField] public Rigidbody spineRigidbody;

    private void Start()
    {
        SetupSpine();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ChangeRagdoll();
        }
    }

    private void SetupSpine()
    {
        if (ragdollObj == null) return;

        Rigidbody[] allRigidbodies = ragdollObj.GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in allRigidbodies)
        {
            if (rb.name.ToLower().Contains("spine"))
            {
                spineRigidbody = rb;
                break;
            }
        }

        if (spineRigidbody == null)
        {
            Debug.LogWarning($"{ragdollObj.name}에서 Spine을 찾을 수 없습니다!");
        }
    }

    public void ChangeRagdoll()
    {
        if (charObj == null || ragdollObj == null) return;

        CopyCharacterTransformToRagdoll(charObj.transform, ragdollObj.transform);

        charObj.SetActive(false);
        ragdollObj.SetActive(true);

        if (spineRigidbody != null)
        {
            spineRigidbody.AddForce(new Vector3(0f, -1.0f, 0f), ForceMode.Impulse);
        }
    }

    private void CopyCharacterTransformToRagdoll(Transform origin, Transform ragdoll)
    {
        ragdoll.localPosition = origin.localPosition;
        ragdoll.localRotation = origin.localRotation;

        for (int i = 0; i < origin.childCount; i++)
        {
            if (i < ragdoll.childCount)
            {
                CopyCharacterTransformToRagdoll(origin.GetChild(i), ragdoll.GetChild(i));
            }
        }
    }
}