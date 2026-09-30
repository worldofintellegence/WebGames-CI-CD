using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PopIt
{
    public class BoardManager : MonoBehaviour
    {
        /// <summary>
        /// Board manager handles everything board related individually. It calculates and prepares all the data here, so other classes can access and use it the way they need.
        /// </summary>

        public static BoardManager instance;

        [Header("Board Settings")]
        public Vector2 boardSize = new Vector2(6, 6);   //X=Rows, Y=Cols
        private int totalBoardButtons;                  //Simply a multiplication of rows * cols

        [Header("Board Data")]
        public List<GameObject> boardButtons;           //cached list of all buttons available in this board
        public List<GameObject> finalizedButtons;       //cachec list of all buttons that has been pressed and can no longer be played.

        private void Awake()
        {
            instance = this;

            boardButtons.Clear();
            finalizedButtons.Clear();

            totalBoardButtons = (int)(boardSize.x * boardSize.y);
        }

        void Start()
        {
            CacheAllButtons();
        }

        /// <summary>
        /// Find and cache all buttons available in this board
        /// </summary>
        public void CacheAllButtons()
        {
            GameObject[] bb;

            //Making sure the array is sorted by name
            bb = GameObject.FindGameObjectsWithTag("BoardButton").OrderBy(go => go.name).ToArray();

            foreach (GameObject g in bb)
            {
                boardButtons.Add(g);
            }
        }

        void Update()
        {

        }

        public void UpdateFinalizedButtonsArray(GameObject g)
        {
            finalizedButtons.Add(g);
        }

        public GameObject GetBoardButton(int btnID)
        {
            return boardButtons[btnID];
        }

        /// <summary>
        /// Check if the selected button is finalized or not.
        /// </summary>
        /// <param name="btnID"></param>
        /// <returns></returns>
        public bool IsBoardButtonFinalized(int btnID)
        {
            //Bug prevention. if for any reason a bad id is given, we should gracefully handle it here.
            if (btnID <= 0 || btnID >= (boardSize.x * boardSize.y))
            {
                print("<color=red>Wrong array index is provided: </color>" + btnID);
                return true;
            }

            if (boardButtons[btnID].GetComponent<ButtonManager>().buttonState == ButtonManager.ButtonStates.Finalized)
                return true;
            else
                return false;
        }

        public int GetBoardButtonColorID(int btnID)
        {
            return boardButtons[btnID].GetComponent<ButtonManager>().buttonColorID;
        }

        public void UpdateButtonStates()
        {
            foreach (GameObject g in boardButtons)
            {
                g.GetComponent<ButtonManager>().UpdateButtonLinks();
            }
        }
    }
}