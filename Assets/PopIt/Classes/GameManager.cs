using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PopIt
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Main Game Manager class that handles game modes, turns, and start/end events. 
        /// </summary>

        public static GameManager instance;

        public enum GameModes { SinglePlayer = 0, TwoPlayers = 1 }
        [Header("Game Mode")]
        public GameModes gameMode = GameModes.SinglePlayer;

        [Header("Game State Data")]
        public List<GameObject> sessionSelectedButtons;

        //Turns
        public static int turnCounter;
        public static bool isP1Turn;

        //Game states
        public static bool isGameStarted;
        public static bool isGameFinished;


        private void Awake()
        {
            instance = this;
            turnCounter = 0;
            isP1Turn = false;

            isGameStarted = false;
            isGameFinished = false;

            sessionSelectedButtons.Clear();

            //Get gamemode from playerprefs
            if(PlayerPrefs.GetInt("GameMode") == 1)
                gameMode = GameModes.SinglePlayer;
            else
                gameMode = GameModes.TwoPlayers;

            print("GameMode: " + gameMode);
        }


        void Start()
        {
            //Randomize first turn for p1 & p2/AI
            RandomizeFirstTurn();

            //Display countdown
            UIManager.instance.DisplayCountdown();

            //Then start the game
            Invoke("TurnManager", 4.1f);
        }

        void Update()
        {
            //Debug - quick reload
            if (Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        /// <summary>
        /// Use this if you want the game to give random turns to players & AI.
        /// </summary>
        public void RandomizeFirstTurn()
        {
            if (Random.value > 0.5f)
                turnCounter++;
        }


        /// <summary>
        /// Assing turns to sides based on a counter.
        /// </summary>
        public void TurnManager()
        {
            FbSfxPlayer.instance.PlaySfx(2);

            //Hide turn button if visible
            UIManager.instance.HideTurnChangeButton();

            //First of all, we need to finalize the selected buttons
            foreach (GameObject g in sessionSelectedButtons)
            {
                g.GetComponent<ButtonManager>().MarkButtonAsFinalized();
            }

            //Clear the session array
            sessionSelectedButtons.Clear();

            //Check if the game is finished
            if (BoardManager.instance.finalizedButtons.Count == BoardManager.instance.boardButtons.Count &&
                BoardManager.instance.finalizedButtons.Count > 0)
            {
                //Game is finished
                Gameover();
                return;
            }

            //Update button states
            BoardManager.instance.UpdateButtonStates();

            //Update the turn
            turnCounter++;
            print("TurnCounter: " + turnCounter);

            if (turnCounter % 2 == 0)
            {
                //P1 turn
                isP1Turn = true;
                print("P1's turn");

                //Update UI
                UIManager.instance.DisplayTurnByOnUI("P1");

                //UI message
                UIManager.instance.DiplayIngameMessage(new Vector3(0, 0, 0), "ingame-message", "Player 1's Turn", true);

                //Restore player ability to click on buttons
                UIManager.instance.RestorePlayerClicksOnButtons();
            }
            else
            {
                //P2/AI turn
                isP1Turn = false;
                print("P2/AI's turn");

                //Resolve new turn
                if (gameMode == GameModes.SinglePlayer)
                {
                    //Force AI to play its turn
                    AIPlayer.instance.Play();

                    //Update UI
                    UIManager.instance.DisplayTurnByOnUI("AI");

                    //UI message
                    UIManager.instance.DiplayIngameMessage(new Vector3(0, 0, 0), "ingame-message", "AI's Turn", true);

                    //We need to temporarily disable player's ability to click on buttons till AI's turn in over
                    UIManager.instance.DisablePlayerClicksOnButtons();
                }
                else
                {
                    //P2 should play now - reflect this via Game UI

                    //Update UI
                    UIManager.instance.DisplayTurnByOnUI("P2");

                    //UI message
                    UIManager.instance.DiplayIngameMessage(new Vector3(0, 0, 0), "ingame-message", "Player 2's Turn", true);
                }
            }
        }


        /// <summary>
        /// Keep track of all buttons that has been pressed in this turn
        /// </summary>
        /// <param name="btn"></param>
        public void AddSessionSelectedButton(GameObject btn)
        {
            sessionSelectedButtons.Add(btn);
        }

        /// <summary>
        /// Keep track of all buttons that has been pressed in this turn
        /// </summary>
        /// <param name="btn"></param>
        public void RemoveSessionSelectedButton(GameObject btn)
        {
            sessionSelectedButtons.Remove(btn);
        }


        public int GetFirstSelectedButtonColorID()
        {
            int id = -1;
            if (sessionSelectedButtons.Count > 0)
            {
                id = sessionSelectedButtons[0].GetComponent<ButtonManager>().buttonColorID;
            }
            return id;
        }

        /// <summary>
        /// When the game ends, we do required stuff in here
        /// </summary>
        public void Gameover()
        {
            //Simple strings for UI usage
            string winner = "";
            string loser = "";

            if (isP1Turn)
            {
                //Play win sfx if p2 is winner and lose sfx if AI won the game
                if(gameMode == GameModes.SinglePlayer)
                {
                    winner = "AI";
                    loser = "Player 1";
                    FbSfxPlayer.instance.PlaySfx(4);
                }
                else
                {
                    winner = "Player 1";
                    loser = "AI";
                    FbSfxPlayer.instance.PlaySfx(5);
                }
            }
            else
            {
                if (gameMode == GameModes.TwoPlayers)
                {
                    winner = "Player 2";
                    loser = "Player 1";
                    FbSfxPlayer.instance.PlaySfx(5);
                }
                else
                {
                    winner = "Player 1";
                    loser = "Player 2";
                    FbSfxPlayer.instance.PlaySfx(5);
                }
            }

            print($"Gameover. Winner/Loser ==> {winner}/{loser}");

            //Display gameover panel
            UIManager.instance.DisplayGameoverPanel(winner);
        }
    }

}