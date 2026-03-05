using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// RAJON — PlayerController
/// Koordinatör. Input okur, state tutar, atomik sistemlere delege eder.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Sistemler")]
    [SerializeField] private EmmiMovement      _movement;
    [SerializeField] private StaminaSystem     _stamina;
    [SerializeField] private WeaponController  _weapon;
    [SerializeField] private HealSystem        _heal;
    [SerializeField] private EmmiAnimator      _animator;
    [SerializeField] private InteractionSystem _interaction;
    [SerializeField] private HealCinematic     _cinematic;

    public bool IsRunning   { get; private set; }
    public bool IsDodging   { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsHealing   { get; private set; }
    public bool IsDead      { get; private set; }
    public bool IsReloading { get; private set; }
    public bool IsPickingUp { get; private set; }

    private RajonInputActions _input;
    private Vector2           _moveInput;
    private bool              _runHeld      = false;
    private float             _runStopTimer = 0f;
    private const float       RunStopDelay  = 0.08f;

    private void Awake() => _input = new RajonInputActions();

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

    private void RegisterInputCallbacks()
    {
        _input.Gameplay.LightAttack.performed    += OnLightAttack;
        _input.Gameplay.HeavyAttack.performed    += OnHeavyAttack;
        _input.Gameplay.HeavyAttack.canceled     += OnHeavyAttackReleased;
        _input.Gameplay.Reload.performed         += OnReload;
        _input.Gameplay.ThrowPickup.performed    += OnThrowPickup;
        _input.Gameplay.Crouching.performed      += OnDodge;
        _input.Gameplay.Heal.performed           += OnHeal;
        _input.Gameplay.Interaction.performed    += OnInteract;
        _input.Gameplay.Zippo.performed          += OnZippo;
        _input.Gameplay.Run.started              += OnRunStarted;
        _input.Gameplay.Run.canceled             += OnRunCanceled;
        _input.Gameplay.RevolverButton.performed += OnRevolverSelect;
        _input.Gameplay.FistButton.performed     += OnFistSelect;
        _input.Gameplay.KnifeButton.performed    += OnKnifeSelect;
        _input.Gameplay.BeltButton.performed     += OnBeltSelect;
    }

    private void UnregisterInputCallbacks()
    {
        _input.Gameplay.LightAttack.performed    -= OnLightAttack;
        _input.Gameplay.HeavyAttack.performed    -= OnHeavyAttack;
        _input.Gameplay.HeavyAttack.canceled     -= OnHeavyAttackReleased;
        _input.Gameplay.Reload.performed         -= OnReload;
        _input.Gameplay.ThrowPickup.performed    -= OnThrowPickup;
        _input.Gameplay.Crouching.performed      -= OnDodge;
        _input.Gameplay.Heal.performed           -= OnHeal;
        _input.Gameplay.Interaction.performed    -= OnInteract;
        _input.Gameplay.Zippo.performed          -= OnZippo;
        _input.Gameplay.Run.started              -= OnRunStarted;
        _input.Gameplay.Run.canceled             -= OnRunCanceled;
        _input.Gameplay.RevolverButton.performed -= OnRevolverSelect;
        _input.Gameplay.FistButton.performed     -= OnFistSelect;
        _input.Gameplay.KnifeButton.performed    -= OnKnifeSelect;
        _input.Gameplay.BeltButton.performed     -= OnBeltSelect;
    }

    private void OnRunStarted(InputAction.CallbackContext ctx)  => _runHeld = true;
    private void OnRunCanceled(InputAction.CallbackContext ctx) => _runHeld = false;

    private void OnRevolverSelect(InputAction.CallbackContext ctx) => OnWeaponSelect(WeaponType.Revolver);
    private void OnFistSelect(InputAction.CallbackContext ctx)     => OnWeaponSelect(WeaponType.Fist);
    private void OnKnifeSelect(InputAction.CallbackContext ctx)    => OnWeaponSelect(WeaponType.Knife);
    private void OnBeltSelect(InputAction.CallbackContext ctx)     => OnWeaponSelect(WeaponType.Belt);

    private void ReadMovementInput()
{
    _moveInput = _input.Gameplay.Movement.ReadValue<Vector2>();
    _movement.Move(CanMove() ? _moveInput : Vector2.zero);
    _animator.SetMoving(_moveInput.sqrMagnitude > 0.01f && CanMove()); // ← CanMove() ekle
}

    private void HandleRun()
    {
        bool wantsToRun = _runHeld && _stamina.HasStamina() && CanMove();

        if (wantsToRun)
        {
            _runStopTimer = RunStopDelay;
            IsRunning = true;
        }
        else
        {
            _runStopTimer -= Time.deltaTime;
            if (_runStopTimer <= 0f) IsRunning = false;
        }

        _movement.SetRunning(IsRunning);
        _stamina.SetConsuming(StaminaConsumer.Run, IsRunning);
        _animator.SetRunning(IsRunning);
    }

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
        if (_weapon.IsBeltCharging) _weapon.ReleaseBelt();
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

    private void OnWeaponSelect(WeaponType type)
    {
        if (!CanAct()) return;
        _weapon.SwitchWeapon(type);
    }

    private void OnHeal(InputAction.CallbackContext ctx)
{
    if (!CanAct() || IsHealing) return;
    if (!_heal.CanHeal()) return; // sigara yoksa hiç girme
    
    IsHealing = true;
    _cinematic.OnFinished += OnHealComplete;
    _heal.UseHeal(null);
}

    private void OnHealComplete()
    {
        _cinematic.OnFinished -= OnHealComplete;
        IsHealing = false;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!CanAct() || IsPickingUp) return;
        if (_interaction == null) return;

        var target = _interaction.CurrentTarget;
        if (target == null) return;

        if (target.GetInteractionType() == InteractionType.Pickup)
        {
            IsPickingUp = true;
            _animator.PlayPickup();
            target.Interact(this);
            Invoke(nameof(EndPickup), 0.667f);
        }
        else
        {
            target.Interact(this);
        }
    }

    private void EndPickup() => IsPickingUp = false;

    private void OnZippo(InputAction.CallbackContext ctx) { /* TODO */ }

    private void OnDodge(InputAction.CallbackContext ctx)
    {
        if (!CanAct() || IsDodging) return;
        IsDodging = true;
        _animator.SetDodging(true);
        _movement.Dodge(() =>
        {
            IsDodging = false;
            _animator.SetDodging(false);
        });
    }

    private bool CanMove()   => !IsDead && !IsHealing && !IsDodging && !IsPickingUp;
    private bool CanAttack() => !IsDead && !IsHealing && !IsReloading && !IsDodging;
    private bool CanAct()    => !IsDead && !IsHealing;

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
    }
}