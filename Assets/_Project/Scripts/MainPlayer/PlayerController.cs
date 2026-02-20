using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// RAJON — PlayerController
/// Koordinatör. Input okur, state tutar, atomik sistemlere delege eder.
/// Kural: Hiçbir method 15 satırı geçmez. Geçiyorsa yanlış yerde.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Atomik Sistem Referansları
    // -------------------------------------------------------------------------
    [Header("Sistemler")]
    [SerializeField] private EmmiMovement     _movement;
    [SerializeField] private StaminaSystem    _stamina;
    [SerializeField] private WeaponController _weapon;
    [SerializeField] private HealSystem       _heal;
    [SerializeField] private EmmiAnimator     _animator;

    // -------------------------------------------------------------------------
    // State Flags — sadece burada tanımlanır, sadece buradan değiştirilir
    // -------------------------------------------------------------------------
    public bool IsRunning   { get; private set; }
    public bool IsDodging   { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsHealing   { get; private set; }
    public bool IsDead      { get; private set; }
    public bool IsReloading { get; private set; }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------
    private RajonInputActions _input;
    private Vector2           _moveInput;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _input = new RajonInputActions();
    }

    private void OnEnable()
    {
        _input.Gameplay.Enable();
        RegisterInputCallbacks();
    }

    private void OnDisable()
    {
        _input.Gameplay.Disable();
        UnregisterInputCallbacks();
    }

    private void Update()
    {
        ReadMovementInput();
        HandleRun();
    }

    // -------------------------------------------------------------------------
    // Input Kaydı
    // -------------------------------------------------------------------------
    private void RegisterInputCallbacks()
    {
        _input.Gameplay.LightAttack.performed  += OnLightAttack;
        _input.Gameplay.HeavyAttack.performed  += OnHeavyAttack;
        _input.Gameplay.Reload.performed       += OnReload;
        _input.Gameplay.ThrowPickup.performed  += OnThrowPickup;
        _input.Gameplay.Crouching.performed    += OnDodge;
        _input.Gameplay.Heal.performed         += OnHeal;
        _input.Gameplay.Interaction.performed  += OnInteract;
        _input.Gameplay.Zippo.performed        += OnZippo;

        _input.Gameplay.RevolverButton.performed += ctx => OnWeaponSelect(WeaponType.Revolver);
        _input.Gameplay.FistButton.performed     += ctx => OnWeaponSelect(WeaponType.Fist);
        _input.Gameplay.KnifeButton.performed    += ctx => OnWeaponSelect(WeaponType.Knife);
        _input.Gameplay.BeltButton.performed     += ctx => OnWeaponSelect(WeaponType.Belt);

        _input.Gameplay.HeavyAttack.canceled    += OnHeavyAttackReleased;
    }

    private void UnregisterInputCallbacks()
    {
        _input.Gameplay.LightAttack.performed  -= OnLightAttack;
        _input.Gameplay.HeavyAttack.performed  -= OnHeavyAttack;
        _input.Gameplay.HeavyAttack.canceled   -= OnHeavyAttackReleased;
        _input.Gameplay.Reload.performed       -= OnReload;
        _input.Gameplay.ThrowPickup.performed  -= OnThrowPickup;
        _input.Gameplay.Crouching.performed    -= OnDodge;
        _input.Gameplay.Heal.performed         -= OnHeal;
        _input.Gameplay.Interaction.performed  -= OnInteract;
        _input.Gameplay.Zippo.performed        -= OnZippo;

        _input.Gameplay.RevolverButton.performed -= ctx => OnWeaponSelect(WeaponType.Revolver);
        _input.Gameplay.FistButton.performed     -= ctx => OnWeaponSelect(WeaponType.Fist);
        _input.Gameplay.KnifeButton.performed    -= ctx => OnWeaponSelect(WeaponType.Knife);
        _input.Gameplay.BeltButton.performed     -= ctx => OnWeaponSelect(WeaponType.Belt);
    }

    // -------------------------------------------------------------------------
    // Hareket
    // -------------------------------------------------------------------------
    private void ReadMovementInput()
{
    _moveInput = _input.Gameplay.Movement.ReadValue<Vector2>();
    if (CanMove())
        _movement.Move(_moveInput);

    _animator.SetMoving(_movement.IsMoving);
}

private void HandleRun()
{
    bool runPressed = _input.Gameplay.Run.IsPressed();
    bool canRun     = runPressed && _moveInput != Vector2.zero && _stamina.HasStamina() && CanMove();

    IsRunning = canRun;
    _movement.SetRunning(IsRunning);
    _stamina.SetConsuming(StaminaConsumer.Run, IsRunning);
    _animator.SetRunning(IsRunning);
}

    // -------------------------------------------------------------------------
    // Combat Callbacks
    // -------------------------------------------------------------------------
    private void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!CanAttack()) return;
        IsAttacking = true;
        _weapon.LightAttack(() => IsAttacking = false);
    }

    private void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (!CanAttack()) return;
        IsAttacking = true;
        _weapon.HeavyAttack(() => IsAttacking = false);
    }

    private void OnHeavyAttackReleased(InputAction.CallbackContext ctx)
    {
        if (_weapon.IsBeltCharging)
            _weapon.ReleaseBelt();
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (!CanAct()) return;
        IsReloading = true;
        _weapon.Reload(() => IsReloading = false);
    }

    private void OnThrowPickup(InputAction.CallbackContext ctx)
    {
        if (!CanAct()) return;
        _weapon.ThrowOrPickup();
    }

    private void OnDodge(InputAction.CallbackContext ctx)
    {
        if (!CanAct() || IsDodging) return;
        IsDodging = true;
        _movement.Dodge(() => IsDodging = false);
    }

    private void OnWeaponSelect(WeaponType type)
    {
        if (!CanAct()) return;
        _weapon.SwitchWeapon(type);
    }

    // -------------------------------------------------------------------------
    // Utility Callbacks
    // -------------------------------------------------------------------------
    private void OnHeal(InputAction.CallbackContext ctx)
    {
        if (!CanAct() || IsHealing) return;
        IsHealing = true;
        _heal.UseHeal(() => IsHealing = false);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!CanAct()) return;
        // TODO: InteractionSystem henüz yazılmadı
    }

    private void OnZippo(InputAction.CallbackContext ctx)
    {
        // TODO: ZippoSystem henüz yazılmadı
    }

    // -------------------------------------------------------------------------
    // Guard Kontrolleri — "şu an yapabilir mi?" kararları burada
    // -------------------------------------------------------------------------
    private bool CanMove()    => !IsDead && !IsHealing;
    private bool CanAttack()  => !IsDead && !IsHealing && !IsReloading && !IsDodging;
    private bool CanAct()     => !IsDead && !IsHealing;

    // -------------------------------------------------------------------------
    // Dışarıdan çağrılır (düşman, hasar sistemi vs.)
    // -------------------------------------------------------------------------
    public void OnHit()
    {
        if (IsDead) return;
        IsAttacking = false;
        IsReloading = false;
        _animator.PlayHit();
    }

    public void OnDeath()
    {
        IsDead = true;
        _animator.PlayDeath();
        // TODO: GameManager.OnPlayerDeath()
    }
}