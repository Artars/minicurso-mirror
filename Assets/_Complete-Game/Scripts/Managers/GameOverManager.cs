using UnityEngine;

namespace CompleteProject
{
    public class GameOverManager : MonoBehaviour
    {
        public PlayerHealth playerHealth = null;       // Reference to the player's health.


        Animator anim;                          // Reference to the animator component.


        void Awake ()
        {
            // Set up the reference.
            anim = GetComponent <Animator> ();
        }


        void Update ()
        {
            /*
                Mirror:
                Fazer a vida ser observada apenas do jogador principal
            */
            if(playerHealth == null)
            {
                if(PlayerNetworked.localPlayer != null)
                    playerHealth = PlayerNetworked.localPlayer.GetComponent<PlayerHealth>();
            }
            // If the player has run out of health...
            else if(playerHealth.currentHealth <= 0)
            {
                // ... tell the animator the game is over.
                anim.SetTrigger ("GameOver");
            }
        }
    }
}