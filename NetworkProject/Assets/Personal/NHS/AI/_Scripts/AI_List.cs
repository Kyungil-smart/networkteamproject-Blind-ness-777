using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class AI_List : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField] private int _AI_Num = 10;
    [SerializeField] private float _mapSize = 20;

    [Header("프리팹")]
    [SerializeField] private GameObject _AI_Prefab;
    [SerializeField] private GameObject _ragdoll_Prefab;

    private List<RagdollChanger> _ragdollChangerList = new List<RagdollChanger>();

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsHost)
        {
            Init();
        }
    }

    private void Init()
    {
        for (int i = 0; i < _AI_Num; i++)
        {
            // 1. AI 생성 및 네트워크 스폰
            GameObject aiInstance = Instantiate(_AI_Prefab, GetRandomPosition(), Quaternion.identity);
            NetworkObject no = aiInstance.GetComponent<NetworkObject>();
            if (no != null) no.Spawn();

            // 2. 래그돌 생성 (비활성 상태로 시작)
            GameObject ragdollInstance = Instantiate(_ragdoll_Prefab, aiInstance.transform.position, aiInstance.transform.rotation);
            ragdollInstance.SetActive(false);

            // 3. 래그돌 체인저 연결
            RagdollChanger changer = aiInstance.GetComponent<RagdollChanger>();
            if (changer == null) changer = aiInstance.AddComponent<RagdollChanger>();

            changer.charObj = aiInstance;
            changer.ragdollObj = ragdollInstance;

            _ragdollChangerList.Add(changer);
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-_mapSize, _mapSize), 0, Random.Range(-_mapSize, _mapSize));
    }
}