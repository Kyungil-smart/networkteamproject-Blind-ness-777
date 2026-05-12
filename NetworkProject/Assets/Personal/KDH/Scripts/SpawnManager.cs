using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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
            _randomPoint += Vector3.up * 1.0f;
            GameObject _player = Instantiate(_playerPrefab, _randomPoint, Quaternion.identity);
            var controller = _player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            _player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            if (controller != null) controller.enabled = true;
        }
    }
    
    private Vector3 GetNavMeshPoint()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-20f, 20f), 1.0f, Random.Range(-20f, 20f));
        
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }
        return Vector3.zero;
    }
}
