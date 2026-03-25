using System;
using UnityEngine;

/// <summary>
/// RAJON — EmmiMovement
/// Sadece hareket matematiği ve dodge.
/// Dodge süresi timer ile değil, Animation Event ile biter.
/// CrouchingNova animasyonunun son frame'ine EndDodge() event'i ekle.
/// </summary>
public class EmmiMovement : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Ayarlar
    // -------------------------------------------------------------------------
    [Header("Hız")]
    [SerializeField] private float _walkSpeed = 3.5f;
    [SerializeField] private float _runSpeed  = 5.5f;

    [Header("Dodge")]
    [SerializeField] private float _dodgeCooldown = 0.8f;

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private bool  _isRunning;
    private bool  _isDodging;
    private bool  _isAttackLocked;
    private float _dodgeCooldownTimer;
    private Action _dodgeCompleteCallback;

    private Rigidbody2D _rb;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_dodgeCooldownTimer > 0f)
            _dodgeCooldownTimer -= Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // Hareket
    // -------------------------------------------------------------------------
    public void Move(Vector2 input)
    {
        // Dodge veya attack lock aktifse hareket yok
        if (_isDodging || _isAttackLocked)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        float speed      = _isRunning ? _runSpeed : _walkSpeed;
        _rb.velocity     = input * speed;

        if (input.x != 0f)
            transform.localScale = new Vector3(Mathf.Sign(input.x), 1f, 1f);
    }

    public void SetRunning(bool isRunning)
    {
        _isRunning = isRunning;
    }

    // -------------------------------------------------------------------------
    // Attack Lock — Belt charge, heavy attack vs. sırasında hareket engellenir
    // -------------------------------------------------------------------------
    public void SetAttackLocked(bool locked)
    {
        _isAttackLocked = locked;
        if (locked)
        {
            _rb.velocity = Vector2.zero;
        }
    }

    public bool IsAttackLocked => _isAttackLocked;

    // -------------------------------------------------------------------------
    // Dodge Başlat — PlayerController çağırır
    // -------------------------------------------------------------------------
    public void Dodge(Action onComplete)
    {
        if (_isDodging || _dodgeCooldownTimer > 0f)
        {
            onComplete?.Invoke();
            return;
        }

        _isDodging             = true;
        _dodgeCompleteCallback = onComplete;
        _rb.velocity           = Vector2.zero;
        _rb.constraints        = RigidbodyConstraints2D.FreezePosition 
                               | RigidbodyConstraints2D.FreezeRotation;
    }

    // -------------------------------------------------------------------------
    // Dodge Bitir — CrouchingNova animasyonunun SON FRAME'ine
    // Animation Event olarak ekle. Method adı: EndDodge
    // -------------------------------------------------------------------------
    public void EndDodge()
    {
        _rb.constraints     = RigidbodyConstraints2D.FreezeRotation;
        _isDodging          = false;
        _dodgeCooldownTimer = _dodgeCooldown;
        _dodgeCompleteCallback?.Invoke();
        _dodgeCompleteCallback = null;
    }

    // -------------------------------------------------------------------------
    // Dışarıdan okunabilir
    // -------------------------------------------------------------------------
    public bool IsDodging         => _isDodging;
    public bool IsDodgeOnCooldown => _dodgeCooldownTimer > 0f;
    public bool IsMoving          => _rb.velocity.sqrMagnitude > 0.01f;
}//