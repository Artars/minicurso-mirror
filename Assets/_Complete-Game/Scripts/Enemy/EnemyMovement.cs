using UnityEngine;
using System.Collections;
using Mirror;

namespace CompleteProject
{
    public class EnemyMovement : NetworkBehaviour
    {
        Transform player;               // Reference to the player's position.
        EnemyHealth enemyHealth;        // Reference to this enemy's health.
        UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
        public float updateTargetRate = 0.2f;


        void Awake ()
        {
            // Set up the references.
            // Mirror: inimigo só perseguirá um jogador
            enemyHealth = GetComponent <EnemyHealth> ();
            nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();

            /*
                Mirror:
                Adaptação para múltiplos jogadores
                Os inimigos irão constantemente procurar por outros alvos
            */
            // if(isServer)
            // {
            //     InvokeRepeating("UpdateTarget",0,updateTargetRate);
            // }
        }

        void Update ()
        {
            if(!isServer) return;

            UpdateTarget();
            // If the enemy and the player have health left...
            if(enemyHealth.currentHealth > 0 && player != null)
            {
                nav.enabled = true;
                // ... set the destination of the nav mesh agent to the player.
                nav.SetDestination (player.position);
            }
            // Otherwise...
            else
            {
                // ... disable the nav mesh agent.
                nav.enabled = false;
            }
        }
        
        void UpdateTarget()
        {
            player = GameManager.instance.GetNearestPlayer(transform.position);
            Debug.Log("Got player? " + player != null);
        }
    }
}