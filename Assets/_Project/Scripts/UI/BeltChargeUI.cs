using UnityEngine;

/// <summary>
/// RAJON — BeltChargeUI
/// Belt seçiliyken Emmi'nin altında görünen 3 slotlu şarj barı.
/// Her slot soldan sağa ease-out curve ile dolar (sonuna doğru yavaşlar).
/// </summary>
public class BeltChargeUI : MonoBehaviour
{
    [Header("Arka Plan Slotları")]
    [SerializeField] private GameObject _slot1BG;
    [SerializeField] private GameObject _slot2BG;
    [SerializeField] private GameObject _slot3BG;

    [Header("Filler Slotları (Pivot sol tarafta)")]
    [SerializeField] private Transform _slot1Filler;
    [SerializeField] private Transform _slot2Filler;
    [SerializeField] private Transform _slot3Filler;

    [Header("Referanslar")]
    [SerializeField] private WeaponController _weapon;
    [SerializeField] private Vector3 _offset = new Vector3(0, -0.5f, 0);

    [Header("Easing Ayarları")]
    [Tooltip("Ease-out gücü. 2 = quadratic, 3 = cubic (daha dramatik yavaşlama)")]
    [SerializeField] private float _easePower = 2.5f;

    private Transform _target;
    private Vector3 _fillerOriginalScale;

    private void Awake()
    {
        _target = _weapon.transform;
        
        // Orijinal scale'i kaydet (hepsi aynı boyutta varsayıyoruz)
        if (_slot1Filler != null)
            _fillerOriginalScale = _slot1Filler.localScale;
    }

    private void Update()
    {
        // Belt seçili değilse gizle
        if (_weapon.CurrentWeapon != WeaponType.Belt)
        {
            HideAll();
            return;
        }

        // Şarj etmiyorsa gizle
        if (!_weapon.IsBeltCharging)
        {
            HideAll();
            return;
        }

        // Göster ve pozisyonu takip et
        ShowBackgrounds();
        transform.position = _target.position + _offset;

        // Filler'ları güncelle
        UpdateFillers();
    }

    private void UpdateFillers()
    {
        float chargeRatio = _weapon.BeltChargeRatio; // 0 - 1 arası (toplam şarj)
        
        // Her slot 1/3 = 0.333 oranında
        // Slot 1: 0.00 - 0.33
        // Slot 2: 0.33 - 0.66
        // Slot 3: 0.66 - 1.00
        
        float slot1Linear = Mathf.Clamp01(chargeRatio / 0.333f);
        float slot2Linear = Mathf.Clamp01((chargeRatio - 0.333f) / 0.333f);
        float slot3Linear = Mathf.Clamp01((chargeRatio - 0.666f) / 0.334f);

        // Ease-out curve uygula: başta hızlı, sona doğru yavaşlar
        // Bu "gerçekten şarj ediliyor, dolmak üzere" hissi verir
        float slot1Fill = EaseOutPow(slot1Linear);
        float slot2Fill = EaseOutPow(slot2Linear);
        float slot3Fill = EaseOutPow(slot3Linear);

        SetFillerScale(_slot1Filler, slot1Fill);
        SetFillerScale(_slot2Filler, slot2Fill);
        SetFillerScale(_slot3Filler, slot3Fill);
    }

    /// <summary>
    /// Ease-Out Power curve: 1 - (1 - t)^power
    /// Başta hızlı dolar, sona doğru yavaşlar.
    /// power=2 → quadratic, power=3 → cubic (daha dramatik)
    /// </summary>
    private float EaseOutPow(float t)
    {
        return 1f - Mathf.Pow(1f - t, _easePower);
    }

    private void SetFillerScale(Transform filler, float fillAmount)
    {
        if (filler == null) return;
        
        Vector3 newScale = _fillerOriginalScale;
        newScale.x = _fillerOriginalScale.x * fillAmount;
        filler.localScale = newScale;
    }

    private void ShowBackgrounds()
    {
        if (_slot1BG != null) _slot1BG.SetActive(true);
        if (_slot2BG != null) _slot2BG.SetActive(true);
        if (_slot3BG != null) _slot3BG.SetActive(true);
        
        if (_slot1Filler != null) _slot1Filler.gameObject.SetActive(true);
        if (_slot2Filler != null) _slot2Filler.gameObject.SetActive(true);
        if (_slot3Filler != null) _slot3Filler.gameObject.SetActive(true);
    }

    private void HideAll()
    {
        if (_slot1BG != null) _slot1BG.SetActive(false);
        if (_slot2BG != null) _slot2BG.SetActive(false);
        if (_slot3BG != null) _slot3BG.SetActive(false);
        
        if (_slot1Filler != null) _slot1Filler.gameObject.SetActive(false);
        if (_slot2Filler != null) _slot2Filler.gameObject.SetActive(false);
        if (_slot3Filler != null) _slot3Filler.gameObject.SetActive(false);
    }
}