using System;
using UnityEngine;

/// <summary>
/// RAJON — EmmiMovement
/// Sadece hareket matematiği ve dodge.
/// PlayerController'dan çağrılır, başka hiçbir şeyden haberdar değildir.
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
    [SerializeField] private float _dodgeDuration  = 0.4f;  // eğilme süresi
    [SerializeField] private float _dodgeCooldown  = 0.8f;  // bekleme süresi

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private bool  _isRunning;
    private bool  _isDodging;
    private float _dodgeCooldownTimer;

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
    // Hareket — PlayerController her Update'te çağırır
    // -------------------------------------------------------------------------
    public void Move(Vector2 input)
    {
        if (_isDodging) return;

        float speed      = _isRunning ? _runSpeed : _walkSpeed;
        Vector2 velocity = input * speed;
        _rb.velocity     = velocity;

        // Sprite yönü: sağa bakıyorsa normal, sola dönünce flip
        if (input.x != 0f)
            transform.localScale = new Vector3(Mathf.Sign(input.x), 1f, 1f);
    }

    public void SetRunning(bool isRunning)
    {
        _isRunning = isRunning;
    }

    // -------------------------------------------------------------------------
    // Dodge — PlayerController callback ile çağırır, bitince haber verir
    // -------------------------------------------------------------------------
    public void Dodge(Action onComplete)
    {
        if (_isDodging || _dodgeCooldownTimer > 0f)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DodgeRoutine(onComplete));
    }

    private System.Collections.IEnumerator DodgeRoutine(Action onComplete)
    {
        _isDodging   = true;
        _rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(_dodgeDuration);

        _isDodging          = false;
        _dodgeCooldownTimer = _dodgeCooldown;
        onComplete?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Dışarıdan okunabilir
    // -------------------------------------------------------------------------
    public bool IsDodging         => _isDodging;
    public bool IsDodgeOnCooldown => _dodgeCooldownTimer > 0f;
    public bool IsMoving => _rb.velocity.sqrMagnitude > 0.01f;
}