using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class RagdollChanger : NetworkBehaviour
{
    [SerializeField] public List<GameObject>    charObj;
    [SerializeField] public List<GameObject> ragdollObj;

    [SerializeField] public List<Rigidbody> spine;

    private void Start()
    {
        SetupSpines();
    }

    private void SetupSpines()
    {
        spine = new List<Rigidbody>();

        foreach (GameObject ragdoll in ragdollObj)
        {
            Rigidbody foundSpine = null;

            //if (foundSpine == null) Debug.Log("foundSpine이 null입니다");

            Rigidbody[] allRigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody rb in allRigidbodies)
            {
                if (rb.name.Contains("spine"))
                {
                    foundSpine = rb;
                    break;
                }
            }

            if (foundSpine != null)
            {
                spine.Add(foundSpine);
            }
            else
            {
                Debug.LogWarning($"{ragdoll.name}에서 Spine을 찾을 수 없습니다!");
                spine.Add(null);
            }
        }
    }

    public void ChangeRagdoll(int index)
    {
        CopyCharacterTransformToRagdoll(charObj[index].transform, ragdollObj[index].transform);

           charObj[index].SetActive(false);
        ragdollObj[index].SetActive(true);

        spine[index].AddForce(new Vector3(0f, 0f, -300f), ForceMode.Impulse);
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
