
// ItemPickup.cs
// Pickup behavior for inventory items
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - world pickup

// ItemPickup.cs
// Pickup behavior for inventory items
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - world pickup
using UnityEngine;

namespace Code.Lavos.Core
{
    public class ItemPickup : MonoBehaviour
    {
    [Header("Item Settings")]
    [SerializeField] private ItemData item;
    [SerializeField] private int quantity = 1;
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private bool autoPickup = false;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;

    private Vector3 _startPosition;
    private bool _canPickup = true;
    private Renderer _renderer;
    private Collider _collider;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _startPosition = transform.position;
    }

    private void Start()
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemPickup] No item assigned to pickup!");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogWarning("[ItemPickup] No Inventory found!");
            return;
        }
    }

    private void Update()
    {
        if (!_canPickup) return;

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (autoPickup)
        {
            TryAutoPickup();
        }
    }

    private void TryAutoPickup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= pickupRadius)
        {
            Pickup(player);
        }
    }

    public void Pickup(GameObject picker)
    {
        if (!_canPickup || item == null) return;

        if (Inventory.Instance.AddItem(item, quantity))
        {
            item.OnPickup(picker);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Debug.Log($"[ItemPickup] Picked up {quantity}x {item.itemName}");

            if (respawnTime > 0)
                StartCoroutine(RespawnRoutine());
            else
                Destroy(gameObject);
        }
        else
        {
            Debug.Log("[ItemPickup] Inventory full!");
        }
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        _canPickup = false;
        if (_renderer != null) _renderer.enabled = false;
        if (_collider != null) _collider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        _canPickup = true;
        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup(other.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
