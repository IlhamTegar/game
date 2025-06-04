using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Musuh : MonoBehaviour
{
    public int damage = 20;
    public float knockbackForce = 5f;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player != null)
        {
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
            player.TakeDamage(damage, knockbackDir);
            
        }
        
    }
}
