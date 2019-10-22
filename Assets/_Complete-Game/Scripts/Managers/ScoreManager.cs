using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;

namespace CompleteProject
{
    public class ScoreManager : NetworkBehaviour
    {
        public static ScoreManager instance;
        [SyncVar]
        public int score;        // The player's score.

        public Text textUI;                      // Reference to the Text component.


        void Awake ()
        {
            if(instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Set up the reference.
            textUI = GetComponent <Text> ();

            // Reset the score.
            score = 0;
        }


        void Update ()
        {
            // Set the displayed text to be the word "Score" followed by the score value.
            if(textUI != null)
                textUI.text = "Score: " + score;
        }
    }
}