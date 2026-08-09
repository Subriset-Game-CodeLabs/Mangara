using Item;
using Manager;
using UnityEngine;
using UnityEngine.InputSystem;

public class MangrovePickup : MonoBehaviour
{
    [SerializeField] private ItemMangroveSO mangroveSeedData;
    
    private bool isPlayerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UIManager.Instance.ShowText("Tekan 'E' untuk mengambil bibit", transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UIManager.Instance.HideText();
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickupSeed();
        }
    }

    private void PickupSeed()
    {
        BestiaryManager.Instance.UnlockMangrove(mangroveSeedData.MangroveType);
        UIManager.Instance.HideText();
        Destroy(gameObject);
    }
}