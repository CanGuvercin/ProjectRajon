using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _lifetime = 2f;
    [SerializeField] private int _damage = 1;
    
    private Vector2 _direction = Vector2.right;
    
    public void SetDirection(Vector2 dir)
    {
        _direction = dir.normalized;
        if (dir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
    
    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }
    
    private void Update()//
    {
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // TODO: Damage sistemi
            // other.GetComponent<Health>()?.TakeDamage(_damage);
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}