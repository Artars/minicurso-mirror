using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CompleteProject
{
    
    public class GameManager : MonoBehaviour
    {
        // Instancia de singleton
        public static GameManager instance = null;

        public List<PlayerNetworked> alivePlayers;

        void Awake()
        {
            // Construtor de Singleton para garantir única instância
            if(instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Atribuir valor a variaveis
            alivePlayers = new List<PlayerNetworked>();
        }

        public void AddPlayer(PlayerNetworked player)
        {
            alivePlayers.Add(player);
        }

        // Removerá o jogador da lista de jogadores vivos
        public void RemovePlayer(PlayerNetworked player)
        {
            if(alivePlayers.Contains(player))
            {
                alivePlayers.Remove(player);

                if(alivePlayers.Count < 1)
                {
                    RestartGame();
                }
            }
        }

        // Abrirá a mesma cena e moverá todos jogadores para ela
        void RestartGame()
        {
            NetworkManager.singleton.ServerChangeScene(NetworkManager.networkSceneName);
        }

        public bool ArePlayersAlive()
        {
            return alivePlayers.Count > 0;
        }

        /*
            Procurará pelo jogador mais próximo do ponto
        */
        public Transform GetNearestPlayer(Vector3 origin)
        {
            float smallestDistance = float.PositiveInfinity;
            int playerIndex = -1;

            if(ArePlayersAlive())
            {
                for (int i = 0; i < alivePlayers.Count; i++)
                {
                    Vector3 dif = origin - alivePlayers[i].transform.position;
                    float distance = dif.sqrMagnitude;
                    if(distance <  smallestDistance)
                    {
                        smallestDistance = distance;
                        playerIndex = i;
                    }
                }

                return alivePlayers[playerIndex].transform;
            }
            else
            {
                return null;
            }
        }
    }

}