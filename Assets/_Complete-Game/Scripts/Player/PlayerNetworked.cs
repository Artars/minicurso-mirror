using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CompleteProject
{
    public class PlayerNetworked : NetworkBehaviour
    {
        // Referência para o jogador principal
        public static PlayerNetworked localPlayer = null;

        [Tooltip("Prefab da câmera a ser instanciada")]
        public GameObject cameraPrefab;
        public UnityEngine.UI.Text scoreText;

        public GameObject hudReference;

        protected CameraFollow cameraFollow;

        /*
            Mirror:
            Será chamado apenas na inicialização do jogador local
            Será usado para setar a variável estática, instanciar a câmera e atribuir o slider à vida
            Ativará a HUD só para jogador local
        */
        public override void OnStartLocalPlayer()
        {
            localPlayer = this;

            GameObject camera = GameObject.Instantiate(cameraPrefab);
            cameraFollow = camera.GetComponent<CameraFollow>();
            cameraFollow.SetTarget(transform);

            hudReference.SetActive(true);

            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            scoreManager.textUI = scoreText;

        }

        /*
            Mirror:
            Será chamado apenas no servidor.
            Adicionará o jogador aos jogadores vivos
        */
        public override void OnStartServer()
        {
            GameManager.instance.AddPlayer(this);
        }

        /*
            Mirror:
            Chamado quando é desconectado
            Precisamos retirar ele da lista de jogadores vivos
        */
        public override void OnNetworkDestroy()
        {
            if(isServer)
            {
                GameManager.instance.RemovePlayer(this);
                Debug.Log("Deletou jogador " +  connectionToClient.connectionId);
            }
        }
    }
}
