using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayNetworkService : MonoBehaviour
{
    public static RelayNetworkService Instance { get; private set; }

    private void Awake() => SetSingleton();

    public async Task<string> StartHostWithRelayAsync(int maxConnections = 8)
    {
        try
        {
            // Relay 서버에 공간 할당
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            // 다른 플레이어가 접속할 Join Code 생성
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // UnityTransport 에 Relay 서버 정보 주입
            RelayServerData serverData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            
            // Host 시작
            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Relay] Host 시작 실패: {e.Message}");
            throw;
        }
    }

    public async Task StartClientWithRelayAsync(string joinCode)
    {
        try
        {
            // Join Code 로 Allocation 참가
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // UnityTransport 에 Relay 서버 정보 주입
            RelayServerData serverData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            // Client 시작
            NetworkManager.Singleton.StartClient();
        } 
        catch (Exception e)
        {
            Debug.LogError($"[Relay] Client 접속 실패: {e.Message}");
            throw;
        }
    }

    private void SetSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}