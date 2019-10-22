using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mirror;

namespace CompleteProject
{
    public class EnemyAttack : NetworkBehaviour
    {
        public float timeBetweenAttacks = 0.5f;     // The time in seconds between each attack.
        public int attackDamage = 10;               // The amount of health taken away per attack.


        Animator anim;                              // Reference to the animator component.
        // Mirror: manter registro de todos os jogadores
        List<GameObject> playersInReach;            // Reference to players in reach.
        EnemyHealth enemyHealth;                    // Reference to this enemy's health.
        bool playerInRange;                         // Whether player is within the trigger collider and can be attacked.
        float timer;                                // Timer for counting up to the next attack.


        void Awake ()
        {
            // Setting up the references.
            playersInReach = new List<GameObject>();
            enemyHealth = GetComponent<EnemyHealth>();
            anim = GetComponent <Animator> ();
        }

        /*
            Mirror: 
            rodará apenas no servidor, adiciona todos os jogadores no alcance
        */
        void OnTriggerEnter (Collider other)
        {
            if(!isServer) return;
            // If the entering collider is the player...
            if(other.tag == "Player")
            {
                playersInReach.Add(other.gameObject);
                // ... the player is in range.
                playerInRange = true;
            }
        }

        /*
            Mirror:
            rodará apenas no servidor, removerá qualquer jogador que sair do alcance
         */
        void OnTriggerExit (Collider other)
        {
            if(!isServer) return;
            // If the exiting collider is the player...
            if(other.tag == "Player")
            {
                playersInReach.Remove(other.gameObject);
                // ... the player is no longer in range.
                playerInRange = playersInReach.Count > 0;
            }
        }

        [Server]
        void Update ()
        {
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;

            // If the timer exceeds the time between attacks, the player is in range and this enemy is alive...
            if(timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0)
            {
                // ... attack.
                Attack ();
            }

            // If the player has zero or less health...
            if(!GameManager.instance.ArePlayersAlive())
            {
                // ... tell the animator the player is dead.
                anim.SetTrigger ("PlayerDead");
            }
        }


        void Attack ()
        {
            // Reset the timer.
            timer = 0f;

            //Make it for every player in range
            foreach (var player in playersInReach)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                // If the player has health to lose...
                if(playerHealth != null && playerHealth.currentHealth > 0)
                {
                    // ... damage the player.
                    playerHealth.TakeDamage (attackDamage);
                }
                
            }
        }
    }
}