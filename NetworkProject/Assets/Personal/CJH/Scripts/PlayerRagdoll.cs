using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    [Header("Ragdoll")]
    [SerializeField] private float _hitForce    = 10f;
    [SerializeField] private float _upwardForce = 3f;

    private Rigidbody[]         _ragdollRigidbodies;
    private Collider[]          _ragdollColliders;
    private PlayerAnimator      _playerAnimator;
    private CharacterController _characterController;

    private void Awake()
    {
        _playerAnimator      = GetComponent<PlayerAnimator>();
        _characterController = GetComponent<CharacterController>();
        _ragdollRigidbodies  = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders    = GetComponentsInChildren<Collider>();

        DisableRagdoll();
    }
    
    private void Update()
    {
        if (!_characterController.enabled)
            Debug.LogWarning("[PlayerRagdoll] CharacterController 비활성화 감지");
    }

    /// <summary>
    /// PlayerController에서 사망 시 호출. attackerPosition은 공격자 위치.
    /// </summary>
    public void ActivateRagdoll(Vector3 attackerPosition)
    {
        _playerAnimator?.DisableAnimator();

        EnableRagdoll();

        Vector3 forceDir = (transform.position - attackerPosition).normalized;
        forceDir.y  = 0f;
        forceDir   += Vector3.up * _upwardForce;

        Rigidbody hipsRb = GetHipsRigidbody();
        if (hipsRb != null)
            hipsRb.AddForce(forceDir * _hitForce, ForceMode.Impulse);
    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            rb.isKinematic      = true;
            rb.detectCollisions = false;
        }

        foreach (Collider col in _ragdollColliders)
        {
            if (col is not CapsuleCollider)
                col.enabled = false;
        }
    }

    private void EnableRagdoll()
    {
        if (_characterController != null)
            _characterController.enabled = false;

        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            rb.isKinematic      = false;
            rb.detectCollisions = true;
        }

        foreach (Collider col in _ragdollColliders)
        {
            if (col is not CapsuleCollider || col.GetComponent<CharacterController>() == null)
                col.enabled = true;
        }
    }

    private Rigidbody GetHipsRigidbody()
    {
        if (_playerAnimator?.Animator == null) return null;
        Transform hips = _playerAnimator.Animator.GetBoneTransform(HumanBodyBones.Hips);
        return hips != null ? hips.GetComponent<Rigidbody>() : null;
    }
}