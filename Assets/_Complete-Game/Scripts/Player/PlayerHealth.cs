using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Mirror;

namespace CompleteProject
{
    public class PlayerHealth : NetworkBehaviour
    {
        public int startingHealth = 100;                            // The amount of health the player starts the game with.
        /*
            Mirror
            Syncvars são variaveis sincronizadas do servidor para o cliente (o servidor deve colocar o valor)
            Podem ser atribuidos hooks, que são funções chamadas quando o valor muda.
            Essas funções deve ter como parametro o mesmo tipo da variavel
            Eles não podem mudar o valor
         */
        [SyncVar(hook=nameof(OnChangeHealth))]
        public int currentHealth;                                   // The current health the player has.
        public Slider healthSlider;                                 // Reference to the UI's health bar.
        public Image damageImage;                                   // Reference to an image to flash on the screen on being hurt.
        public AudioClip deathClip;                                 // The audio clip to play when the player dies.
        public float flashSpeed = 5f;                               // The speed the damageImage will fade at.
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     // The colour the damageImage is set to, to flash.


        Animator anim;                                              // Reference to the Animator component.
        AudioSource playerAudio;                                    // Reference to the AudioSource component.
        PlayerMovement playerMovement;                              // Reference to the player's movement.
        PlayerShooting playerShooting;                              // Reference to the PlayerShooting script.
        [SyncVar]
        bool isDead;                                                // Whether the player is dead.
        bool damaged;                                               // True when the player gets damaged.


        void Awake ()
        {
            // Setting up the references.
            anim = GetComponent <Animator> ();
            playerAudio = GetComponent <AudioSource> ();
            playerMovement = GetComponent <PlayerMovement> ();
            //Mirror: colocar player shooting na root
            playerShooting = GetComponent <PlayerShooting> ();

            // Set the initial health of the player.
            // Mirror: isso só será feito se for o servidor
            if(isServer)
                currentHealth = startingHealth;
        }

        /*
            Mirror
            O jogador pode entrar no meio da partida. Se isso acontecer, ele precisa verificar se o jogador está morto

            As SyncVars já são atualizadas antes do objeto estar disponível para o script
         */
        void Start()
        {
            if(isDead)
                Death();
        }


        /*
            Mirror
            Será mantido inalterado.
            É lógica comum a todos os jogadores
         */ 
        void Update ()
        {
            // If the player has just been damaged...
            if(damaged)
            {
                // ... set the colour of the damageImage to the flash colour.
                damageImage.color = flashColour;
            }
            // Otherwise...
            else
            {
                // ... transition the colour back to clear.
                damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }

            // Reset the damaged flag.
            damaged = false;
        }


        /*
            Mirror
            Vai rodar apenas no servidor
            Deve atualizar a vida e chamar a morte para todos os jogadores se necessário
         */
        public void TakeDamage (int amount)
        {
            // Set the damaged flag so the screen will flash.
            damaged = true;

            // Reduce the current health by the damage amount.
            currentHealth -= amount;

            // Set the health bar's value to the current health.
            healthSlider.value = currentHealth;

            // Play the hurt sound effect.
            playerAudio.Play ();

            // If the player has lost all it's health and the death flag hasn't been set yet...
            if(currentHealth <= 0 && !isDead)
            {
                // ... it should die.
                Death ();
                RpcDeath(); // Mirror: chamar morte para todos os jogadores
            }
        }

        /*
            Mirror
            Essa função será chamada em todos os clientes do jogo
            É uma Remote Procedure Call
            Será apenas uma cópia do original
         */
        [ClientRpc]
        void RpcDeath()
        {
            // Set the death flag so this function won't be called again.
            isDead = true;

            // Turn off any remaining shooting effects.
            playerShooting.DisableEffects ();

            // Mirror: o network animator não sincroniza trigger
            // Tell the animator that the player is dead.
            anim.SetTrigger ("Die");

            // Set the audiosource to play the death clip and play it (this will stop the hurt sound from playing).
            playerAudio.clip = deathClip;
            playerAudio.Play ();

            // Turn off the movement and shooting scripts.
            playerMovement.enabled = false;
            playerShooting.enabled = false;
        }


        void Death ()
        {
            // Set the death flag so this function won't be called again.
            isDead = true;

            // Turn off any remaining shooting effects.
            playerShooting.DisableEffects ();

            // Tell the animator that the player is dead.
            anim.SetTrigger ("Die");

            // Set the audiosource to play the death clip and play it (this will stop the hurt sound from playing).
            playerAudio.clip = deathClip;
            playerAudio.Play ();

            // Turn off the movement and shooting scripts.
            playerMovement.enabled = false;
            playerShooting.enabled = false;
        }


        public void RestartLevel ()
        {

            if(isServer)
            {
                // Mirror: remover jogador morto da lista de jogadores vivos
                GameManager.instance.RemovePlayer(GetComponent<PlayerNetworked>());
            }
        }

        /*
            Mirror
            O valor será atribuido no final pelo Mirror. Devemos apenas realizar parte da função "TakeDamage" 
            se a vida nova é menor que a antiga
            A morte será contabilizada apenas pelo servidor e não por cada jogador

         */
        protected void OnChangeHealth(int newHealth)
        {
            if(currentHealth > newHealth)
            {
                // Set the damaged flag so the screen will flash.
                damaged = true;

                /*
                    Mirror
                    A vida só será atualizada se for o jogador principal
                */
                if(isLocalPlayer)
                {
                    healthSlider.value = currentHealth;
                }

                // Play the hurt sound effect.
                playerAudio.Play ();
            }
        }
    }
}