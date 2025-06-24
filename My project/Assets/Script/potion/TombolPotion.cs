using UnityEngine;

public class TombolPotion : MonoBehaviour
{
    public void OnPotionButtonClick()
    {
        PotionManager.Instance.UsePotion();
    }
}
