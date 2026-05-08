using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class AI_List : NetworkBehaviour
{
    [Header("Numbers of AI")]
    [SerializeField] private int _AI_Num = 10;

    [Header("Prefabs")]
    [SerializeField] private GameObject      _AI_Prefab;
    [SerializeField] private GameObject _ragdoll_Prefab;

    private List<RagdollChanger> _ragdollChangerList = new List<RagdollChanger>();

    [SerializeField] private float _mapSize = 20;

    //public override void OnNetworkSpawn()
    //{
    //    if (!IsServer) return;
    //
    //    Debug.Log("스폰시작");
    //
    //    Init();
    //}

    //private void Start()
    //{
    //    // 1. 네트워크 매니저가 있는지 확인 (에러 방지)
    //    if (NetworkManager.Singleton == null)
    //    {
    //        Debug.LogError("NetworkManager가 씬에 없습니다!");
    //        return;
    //    }
    //
    //    // 2. 일단 로그가 찍히는지 확인
    //    Debug.Log("Start() 호출됨 - 네트워크 상태와 상관없이 실행");
    //
    //    // 3. 만약 버튼을 눌러 Host/Server를 시작한 상태라면 실행
    //    if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
    //    {
    //        Debug.Log("서버/호스트 상태 확인됨. AI 생성 시작");
    //        Init();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("현재 서버나 호스트가 아닙니다. Start Host 버튼을 눌렀는지 확인하세요.");
    //    }
    //}

    private void Start()
    {
        if (_AI_Prefab == null || _ragdoll_Prefab == null)
        {
            Debug.LogError("프리팹이 할당되지 않았습니다! 인스펙터를 확인하세요.");
            return;
        }

        Init();
    }

    private void Init()
    {
        for(int i=0;i<_AI_Num;i++)
        {
            GameObject      aiInstance = Instantiate(_AI_Prefab, GetRandomPosition(), Quaternion.identity);
            GameObject ragdollInstance = Instantiate(_ragdoll_Prefab, aiInstance.transform.position, aiInstance.transform.rotation);
            ragdollInstance.SetActive(false);

            RagdollChanger changer = aiInstance.GetComponent<RagdollChanger>();
            if (changer == null) changer = aiInstance.AddComponent<RagdollChanger>();

            changer.   charObj =      aiInstance;
            changer.ragdollObj = ragdollInstance;

            _ragdollChangerList.Add(changer);

            NetworkObject no = aiInstance.GetComponent<NetworkObject>();

            /*
            if (no != null)
            {
                no.Spawn();
            }
            */
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-_mapSize, _mapSize), 0, Random.Range(-_mapSize, _mapSize));
    }

    public void SetRagdoll(int index) => _ragdollChangerList[index].ChangeRagdoll();
}