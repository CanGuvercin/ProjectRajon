using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmmoUI : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private WeaponController _weapon;
    [SerializeField] private TMPro.TextMeshProUGUI _magazineText;
    
    [Header("Slotlar (sırayla 1-8)")]
    [SerializeField] private GameObject[] _slots = new GameObject[8];
    [SerializeField] private GameObject[] _bullets = new GameObject[8];
    
    [Header("Reload Animasyonu")]
    [SerializeField] private float _reloadBulletDelay = 0.12f;
    [SerializeField] private float _bulletDropDuration = 0.08f;
    [SerializeField] private float _bulletDropOffset = 15f;
    
    private int _magazines = 3;
    private int _maxSlots = 5;
    private Vector2[] _originalPositions;
    private bool _isReloadAnimating = false;

    private void OnEnable()
    {
        _weapon.OnAmmoChanged += UpdateAmmoDisplay;
        _weapon.OnWeaponChanged += OnWeaponChanged;
    }

    private void OnDisable()
    {
        _weapon.OnAmmoChanged -= UpdateAmmoDisplay;
        _weapon.OnWeaponChanged -= OnWeaponChanged;
    }

    private void Start()
    {
        CacheOriginalPositions();
        InitializeSlots();
        UpdateAmmoDisplay(_weapon.CurrentAmmo, _maxSlots);
        UpdateMagazineText();
    }

    private void CacheOriginalPositions()
    {
        _originalPositions = new Vector2[_bullets.Length];
        for (int i = 0; i < _bullets.Length; i++)
        {
            RectTransform rect = _bullets[i].GetComponent<RectTransform>();
            _originalPositions[i] = rect.anchoredPosition;
        }
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].SetActive(i < _maxSlots);
        }
    }

    private void OnWeaponChanged(WeaponType type)
    {
        gameObject.SetActive(type == WeaponType.Revolver);
    }

    private void UpdateAmmoDisplay(int current, int max)
    {
        // Reload animasyonu sırasında normal update'i atla
        if (_isReloadAnimating) return;
        
        if (max > _maxSlots)
        {
            for (int i = _maxSlots; i < max && i < _slots.Length; i++)
            {
                _slots[i].SetActive(true);
            }
            _maxSlots = max;
        }

        for (int i = 0; i < _maxSlots; i++)
        {
            _bullets[i].SetActive(i < current);
        }
    }

    private void UpdateMagazineText()
    {
        _magazineText.text = $"x {_magazines}";
    }

    // -------------------------------------------------------------------------
    // Reload Animasyonu
    // -------------------------------------------------------------------------
    public void PlayReloadAnimation(int bulletCount)
    {
        StartCoroutine(ReloadAnimationRoutine(bulletCount));
    }

    private IEnumerator ReloadAnimationRoutine(int bulletCount)
    {
        _isReloadAnimating = true;
        
        // Önce tüm mermileri gizle
        for (int i = 0; i < _maxSlots; i++)
        {
            _bullets[i].SetActive(false);
        }

        // Soldan sağa sırayla doldur
        for (int i = 0; i < bulletCount && i < _maxSlots; i++)
        {
            StartCoroutine(DropBullet(i));
            yield return new WaitForSeconds(_reloadBulletDelay);
        }

        // Son mermi animasyonunun bitmesini bekle
        yield return new WaitForSeconds(_bulletDropDuration);
        
        _isReloadAnimating = false;
    }

    private IEnumerator DropBullet(int index)
    {
        GameObject bullet = _bullets[index];
        RectTransform rect = bullet.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = bullet.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = bullet.AddComponent<CanvasGroup>();

        Vector2 originalPos = _originalPositions[index];
        Vector2 startPos = originalPos + Vector2.up * _bulletDropOffset;
        
        rect.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;
        bullet.SetActive(true);

        float elapsed = 0f;
        while (elapsed < _bulletDropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _bulletDropDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic
            
            rect.anchoredPosition = Vector2.Lerp(startPos, originalPos, easeT);
            canvasGroup.alpha = easeT;
            
            yield return null;
        }

        rect.anchoredPosition = originalPos;
        canvasGroup.alpha = 1f;
    }

    // -------------------------------------------------------------------------
    // Şarjör Yönetimi
    // -------------------------------------------------------------------------
    public void AddMagazine(int count = 1)
    {
        _magazines += count;
        UpdateMagazineText();
    }

    public bool UseMagazine()
    {
        if (_magazines <= 0) return false;
        _magazines--;
        UpdateMagazineText();
        return true;
    }

    public bool HasMagazine() => _magazines > 0;
    public int MagazineCount => _magazines;
}