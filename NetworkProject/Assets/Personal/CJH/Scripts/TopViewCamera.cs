using System.Collections;
using UnityEngine;

public class TopViewCamera : MonoBehaviour, IPhaseChangeable
{
    [Header("전환")]
    [SerializeField] private float _blendDuration = 1.5f;

    private Vector3           _initPosition;
    private Quaternion        _initRotation;
    private Camera            _camera;
    private ThirdPersonCamera _thirdPersonCamera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.enabled = false;

        _initPosition = transform.position;
        _initRotation = transform.rotation;
    }

    /// <summary>
    /// PlayerController에서 오너 플레이어 카메라 스폰 시 연결.
    /// </summary>
    public void SetThirdPersonCamera(ThirdPersonCamera cam) => _thirdPersonCamera = cam;

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Shooting:
                StartCoroutine(BlendToTopView());
                break;

            case GamePhase.HideAndSeek:
                _camera.enabled = false;
                _thirdPersonCamera?.SetActive(true);
                _thirdPersonCamera?.gameObject.SetActive(true);
                break;
        }
    }

    private IEnumerator BlendToTopView()
    {
        _camera.enabled = true;

        if (_thirdPersonCamera == null)
        {
            transform.position = _initPosition;
            transform.rotation = _initRotation;
            yield break;
        }

        Vector3    startPos = _thirdPersonCamera.transform.position;
        Quaternion startRot = _thirdPersonCamera.transform.rotation;
        Vector3    endPos = _initPosition;
        Quaternion endRot = _initRotation;

        float elapsed = 0f;
        while (elapsed < _blendDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / _blendDuration);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        _thirdPersonCamera.SetActive(false);
        _thirdPersonCamera.gameObject.SetActive(false);
    }
}