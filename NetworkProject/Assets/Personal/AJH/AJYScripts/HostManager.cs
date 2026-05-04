using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

[Serializable]
public class HostManager : MonoBehaviour
{
    private static HostManager instance;
    public static HostManager Instance
    {
        get { return instance; }
        private set { }
    }

    // 플레이어 ID 키값 joincode
    public Dictionary<string, string> JoinCodes;
    // Joincode 존재확인용
    public HashSet<string> CheckJoinCodes;
    public Dictionary<string, NicknameStorage> HostStorages;
    
    
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void Init()
    { 
        JoinCodes = new Dictionary<string, string>();
        CheckJoinCodes =  new HashSet<string>();
        HostStorages =  new Dictionary<string, NicknameStorage>();
    }

    // 조인 코드 기준 저장소 생성
    public void GenerateHostStorage(string hostname, string joincode )
    {
        JoinCodes.Add(AuthenticationService.Instance.PlayerId, joincode);
        // 클라이언트 접속 시 코드 확인용
        CheckJoinCodes.Add(joincode);
        HostStorages.Add(joincode, new NicknameStorage().Initialize(hostname));
    }

    public void AddName(string clientName, string joincode)
    {
        HostStorages[joincode].Nicknames.Add(AuthenticationService.Instance.PlayerId ,clientName);
    }
}
