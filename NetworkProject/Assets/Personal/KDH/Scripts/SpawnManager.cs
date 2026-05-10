using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject _aiPrefab;
    public GameObject _playerPrefab;
    
    public void SpawnAll()
    {
        SpawnAI();
        SpawnPlayer();
    }
    
    public void SpawnAI()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 _randomPoint = GetNavMeshPoint();
            GameObject _ai = Instantiate(_aiPrefab, _randomPoint, Quaternion.identity);
            _ai.GetComponent<NetworkObject>().Spawn();
        } 
    }
    
    public void SpawnPlayer()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Vector3 _randomPoint = GetNavMeshPoint();
            GameObject _player = Instantiate(_playerPrefab, _randomPoint, Quaternion.identity);
            _player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
    
    private Vector3 GetNavMeshPoint()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
        
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                return hit.position;
        }
        return Vector3.zero;
    }
}
