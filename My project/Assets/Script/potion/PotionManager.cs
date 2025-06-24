using UnityEngine;
using TMPro;

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance; // Tambahkan ini

    public PlayerMovement player;
    public TMP_Text jumlahPotionText;
    private int totalPotion = 10;

    private void Awake()
    {
        // Inisialisasi Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UsePotion()
    {
        if (totalPotion > 0 && player.currentHealth < player.maxHealth)
        {
            int healAmount = 50;
            player.Heal(healAmount);   // ⬅ Pakai fungsi Heal
            totalPotion--;
            UpdatePotionUI();
        }
    }

    public void UpdatePotionUI()
    {
        jumlahPotionText.text = totalPotion.ToString();
    }
}
