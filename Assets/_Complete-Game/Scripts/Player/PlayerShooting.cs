using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;
using Mirror;

namespace CompleteProject
{
    public class PlayerShooting : NetworkBehaviour
    {
        public int damagePerShot = 20;                  // The damage inflicted by each bullet.
        public float timeBetweenBullets = 0.15f;        // The time between each shot.
        public float range = 100f;                      // The distance the gun can fire.
        public Transform shootPosition;                 // Mirror: a posição de tiro será diferente da origem do objeto
        [SyncVar]
        public bool isShooting;                         // Mirror: o jogador vai pedir para o servidor alterar essa variável e continuar atirando


        float timer;                                    // A timer to determine when to fire.
        Ray shootRay = new Ray();                       // A ray from the gun end forwards.
        RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
        int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
        public ParticleSystem gunParticles;                    // Reference to the particle system.
        public LineRenderer gunLine;                           // Reference to the line renderer.
        public AudioSource gunAudio;                           // Reference to the audio source.
        public Light gunLight;                                 // Reference to the light component.
		public Light faceLight;								// Duh
        float effectsDisplayTime = 0.2f;                // The proportion of the timeBetweenBullets that the effects will display for.


        void Awake ()
        {
            // Create a layer mask for the Shootable layer.
            shootableMask = LayerMask.GetMask ("Shootable");

            // Set up the references.
            /*
                Mirror:
                Não será mais necessário adquirir as referências, já que estão como publica
                e fora do objeto principal
            */

        }

        /*
            Mirror:
            Será necessário separar entre as funções chamadas em cada cliente e aquelas chamadas no servidor
            
            O dono do jogador, poderá colocar se o jogador está atirando ou não
            Todos fazem a atualização de tiro
         */
        void Update ()
        {
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;

            // Outside of the defines
            if(isLocalPlayer)
            {

#if !MOBILE_INPUT
                // If the Fire1 button is being press...
                bool shouldShoot = Input.GetButton ("Fire1");
                if(shouldShoot ^ isShooting)
                {
                    // ... shoot the gun.
                    CmdSetShooting(shouldShoot);
                }
#else
                // If there is input on the shoot direction stick...
                bool shouldShoot = (CrossPlatformInputManager.GetAxisRaw("Mouse X") != 0 || CrossPlatformInputManager.GetAxisRaw("Mouse Y") != 0);
                if(shouldShoot ^ isShooting)
                {
                    // ... shoot the gun
                    CmdSetShooting(shouldShoot);
                }
#endif
            }

            // If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
            if(timer >= timeBetweenBullets * effectsDisplayTime)
            {
                // ... disable the effects.
                DisableEffects ();
            }
            if(timer >= timeBetweenBullets && Time.timeScale != 0 && isShooting)
            {
                Shoot();
            }

        }

        /*
            Mirror:
            O servidor vai atualizar essa variável, já que o jogador não pode
        */
        [Command]
        void CmdSetShooting(bool value)
        {
            isShooting = value;
        }

        public void DisableEffects ()
        {
            // Disable the line renderer and the light.
            gunLine.enabled = false;
			faceLight.enabled = false;
            gunLight.enabled = false;
        }


        /*
            Mirror:
            Será necessário separar entre o que será feito no servidor, no cliente dono do jogador e em todos os
            clientes

            O cliente dono chamará uma função no servidor para calcular o dano e para notificar todos os clientes,
            incluindo o dono

            Será separado em duas funções CmdShoot (do servidor) e RpcShoot (dos clientes)
         */
        void Shoot ()
        {
            // Reset the timer.
            timer = 0f;

            Vector3 hitPos; //Mirror: Posição de acerto de raio

            // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
            shootRay.origin = shootPosition.position;
            shootRay.direction = shootPosition.forward;

            Debug.Log("Fired ray");
            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
            {
                // Mirror: Só o servidor pode ferir os inimigos
                if(isServer)
                {
                    // Try and find an EnemyHealth script on the gameobject hit.
                    EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth> ();

                    // If the EnemyHealth component exist...
                    if(enemyHealth != null)
                    {
                        // ... the enemy should take damage.
                        enemyHealth.TakeDamage (damagePerShot, shootHit.point);
                    }
                }

                // Set the second position of the line renderer to the point the raycast hit.
                hitPos = shootHit.point;
            }
            // If the raycast didn't hit anything on the shootable layer...
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                hitPos = shootRay.origin + shootRay.direction * range;
            }

            // If it's server only, avoid extra processing
            if(!isServerOnly)
            {
                // Play the gun shot audioclip.
                gunAudio.Play ();

                // Enable the lights.
                gunLight.enabled = true;
                faceLight.enabled = true;

                // Stop the particles from playing if they were, then start the particles.
                gunParticles.Stop ();
                gunParticles.Play ();

                // Enable the line renderer and set it's first position to be the end of the gun.
                gunLine.enabled = true;
                gunLine.SetPosition (0, shootPosition.position);
                gunLine.SetPosition(1, hitPos);
            }

        }


       
    }
}