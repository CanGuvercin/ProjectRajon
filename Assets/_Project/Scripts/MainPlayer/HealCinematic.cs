using System.Collections;
using UnityEngine;

/// <summary>
/// RAJON — HealCinematic
/// Sigara yakma sırasındaki zoom ve müzik efekti.
/// HealSystem çağırır. Kamera ve ses sistemlerini yönetir.
/// </summary>
public class HealCinematic : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Ayarlar
    // -------------------------------------------------------------------------
    [Header("Zoom")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float  _zoomInSize   = 3.5f;   // yakınlaşınca orthographic size
    [SerializeField] private float  _zoomOutSize  = 5.0f;   // normal kamera boyutu
    [SerializeField] private float  _zoomInSpeed  = 6f;
    [SerializeField] private float  _zoomOutSpeed = 4f;

    [Header("Müzik")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip   _healMusic;        // Türk klasiği

    [Header("Süre")]
    [SerializeField] private float _holdDuration = 1.2f;    // zoom'da bekleme süresi

    // -------------------------------------------------------------------------
    // İç Durum
    // -------------------------------------------------------------------------
    private Coroutine _activeRoutine;

    // -------------------------------------------------------------------------
    // HealSystem tarafından çağrılır
    // -------------------------------------------------------------------------
    public void Play()
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = StartCoroutine(CinematicRoutine());
    }

    public void Stop()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }

        // Müziği kapat, kamerayı sıfırla
        if (_musicSource != null && _musicSource.isPlaying)
            _musicSource.Stop();

        if (_camera != null)
            _camera.orthographicSize = _zoomOutSize;
    }

    // -------------------------------------------------------------------------
    // Sinematik Döngü
    // -------------------------------------------------------------------------
    private IEnumerator CinematicRoutine()
    {
        // Müzik başlat
        if (_musicSource != null && _healMusic != null)
        {
            _musicSource.clip = _healMusic;
            _musicSource.Play();
        }

        // Zoom in
        yield return StartCoroutine(ZoomTo(_zoomInSize, _zoomInSpeed));

        // Bekle
        yield return new WaitForSeconds(_holdDuration);

        // Zoom out
        yield return StartCoroutine(ZoomTo(_zoomOutSize, _zoomOutSpeed));

        // Müzik kapat
        if (_musicSource != null)
            _musicSource.Stop();

        _activeRoutine = null;
    }

    private IEnumerator ZoomTo(float targetSize, float speed)
    {
        if (_camera == null) yield break;

        while (!Mathf.Approximately(_camera.orthographicSize, targetSize))
        {
            _camera.orthographicSize = Mathf.MoveTowards(
                _camera.orthographicSize,
                targetSize,
                speed * Time.deltaTime
            );
            yield return null;
        }

        _camera.orthographicSize = targetSize;
    }
}