using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopIt
{
    public class ButtonManager : MonoBehaviour
    {
        /// <summary>
        /// This class should be attached to all pop buttons. It handles all the events that should be performed by the buttons. 
        /// Important. You need to make sure the color sequence of buttons (aka rows) are exactly following the rule stated below:
        /// Button color code IDs:
        /// Red = 0
        /// Orange = 1
        /// Yellow = 2
        /// Green = 3
        /// Blue = 4
        /// Purple = 5
        /// </summary>

        public enum ButtonStates { Selectable = 0, Selected = 1, Finalized = 2 }
        public ButtonStates buttonState = ButtonStates.Selectable;

        [Header("Children & Components")]
        private Image buttonSprite;

        [Header("IDs & Indexs")]
        public int buttonColorID = 0;
        public int buttonPositionID = 0;

        [Header("AI Data")]
        public int buttonLinks;


        private void Awake()
        {
            buttonSprite = GetComponent<Image>();
        }


        void Start()
        {
            InitButtonState();
        }


        public void InitButtonState()
        {
            buttonState = ButtonStates.Selectable;
            UpdateButtonLook(buttonState);

            Invoke("UpdateButtonLinks", 0.1f);          //Important. needs to be done with a slight delay.
        }


        /// <summary>
        /// To assisst AI with decision making, we need to assign some sort of importance to each button.
        /// So we count the links of each button, ie, the number of consecutive links that can be made with other buttons using this button.
        /// </summary>
        public void UpdateButtonLinks()
        {
            //Reset
            buttonLinks = 0;

            if (buttonState == ButtonStates.Finalized)
            {
                buttonLinks = 0;
                return;
            }

            //Count itself
            buttonLinks++;

            //Look forward in row
            for (int i = buttonPositionID + 1; i < BoardManager.instance.boardSize.y; i++)
            {
                if (BoardManager.instance.GetBoardButton((int)(buttonColorID * BoardManager.instance.boardSize.x) + i).GetComponent<ButtonManager>().buttonState != ButtonStates.Finalized)
                {
                    buttonLinks++;
                }
                else
                    break;
            }

            //Look backward in row
            for (int i = buttonPositionID - 1; i >= 0; i--)
            {
                if (BoardManager.instance.GetBoardButton((int)(buttonColorID * BoardManager.instance.boardSize.x) + i).GetComponent<ButtonManager>().buttonState != ButtonStates.Finalized)
                {
                    buttonLinks++;
                }
                else
                    break;
            }
        }

        void Update()
        {
            //Debug
            if (Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    UpdateButtonLinks();
                }
            }
        }


        public void UpdateButtonLook(ButtonStates bState)
        {
            buttonSprite.sprite = UIManager.instance.GetButtonSprite((int)buttonState, buttonColorID);
        }


        /// <summary>
        /// Button click outcomes:
        /// 1. if button can be selected, make it seleced.
        /// 2. if button is selected already, make it selectable (unselected)
        /// 3. if button is finalized, do nothing.
        /// </summary>
        public void ButtonClicked()
        {
            //print("<b>Clicked on: " + gameObject.name + "</b>");

            if (buttonState == ButtonStates.Finalized)
            {
                print("This button is final and not interactable anymore. exit");
                return;
            }

            if (buttonState == ButtonStates.Selectable)
            {
                SelectButton();
                return;
            }

            if (buttonState == ButtonStates.Selected)
            {
                UnselectButton();
                return;
            }
        }


        public void SelectButton()
        {
            //first of all check if this button can be selected.
            //Steps:
            /*
            1. all buttons must belong to a row, aka, be of same color
            2. buttons must be connected. 
            */
            if (!IsButtonSeletable())
                return;

            FbSfxPlayer.instance.PlaySfx(7);
            UIManager.instance.CreateButtonPopEffect(this.gameObject);

            buttonState = ButtonStates.Selected;
            UpdateButtonLook(buttonState);

            //Update session selected array
            GameManager.instance.AddSessionSelectedButton(gameObject);

            //Update turn button status 
            if (GameManager.instance.gameMode == GameManager.GameModes.TwoPlayers || (GameManager.instance.gameMode == GameManager.GameModes.SinglePlayer && GameManager.isP1Turn))
                UIManager.instance.UpdateTurnButtonStatus();
        }


        public bool IsButtonSeletable()
        {
            if (GameManager.instance.sessionSelectedButtons.Count == 0)
            {
                //This is the first button, so every button is selectable
                return true;
            }
            else
            {
                //If this is not the first button:

                //Check if it is of the same color, aka same row
                int firstButtonColorID = GameManager.instance.GetFirstSelectedButtonColorID();
                if (buttonColorID != firstButtonColorID)
                {
                    print("New button color does not match!");
                    return false;
                }

                //This needs to be connected to one of previously selected buttons
                bool caseIsMet = false;
                foreach (GameObject g in GameManager.instance.sessionSelectedButtons)
                {
                    if (buttonPositionID == g.GetComponent<ButtonManager>().buttonPositionID + 1 || buttonPositionID == g.GetComponent<ButtonManager>().buttonPositionID - 1)
                        caseIsMet = true;
                }

                if (caseIsMet)
                    return true;
                else
                    return false;
            }
        }


        public void UnselectButton()
        {
            //first of all check if this button can be unselected.
            //Steps:
            /*
            1. all buttons must be connected. so an unselection should not result into separation of two groups of buttons.
            2. first and last buttons can always be unselected.
            */
            if (!IsButtonUnseletable())
                return;

            FbSfxPlayer.instance.PlaySfx(6);
            UIManager.instance.CreateButtonPopEffect(this.gameObject);

            buttonState = ButtonStates.Selectable;
            UpdateButtonLook(buttonState);

            //Update session selected array
            GameManager.instance.RemoveSessionSelectedButton(gameObject);

            //Update turn button status
            UIManager.instance.UpdateTurnButtonStatus();
        }


        public bool IsButtonUnseletable()
        {
            //This is the only button, or its the first or last one, it can be freely unselected
            if (GameManager.instance.sessionSelectedButtons.Count <= 2 || buttonPositionID == 0 || buttonPositionID == 5)
            {
                return true;
            }
            else
            {
                //If this is not the only button (we have 3 or more):
                //check if removal of this buttons led to separation of two groups of buttons
                bool leftCaseIsMet = false;
                bool rightCaseIsMet = false;
                foreach (GameObject g in GameManager.instance.sessionSelectedButtons)
                {
                    if (g.GetComponent<ButtonManager>().buttonPositionID == buttonPositionID - 1)
                        leftCaseIsMet = true;

                    if (g.GetComponent<ButtonManager>().buttonPositionID == buttonPositionID + 1)
                        rightCaseIsMet = true;
                }

                if (leftCaseIsMet && rightCaseIsMet)    //if there are selcted buttons at the two sides of this button, it can not be unselected!
                    return false;
                else
                    return true;
            }
        }


        public void MarkButtonAsFinalized()
        {
            buttonState = ButtonStates.Finalized;
            UpdateButtonLook(buttonState);
            GetComponent<Button>().enabled = false;

            //Update BoardManager with finalized buttons
            BoardManager.instance.UpdateFinalizedButtonsArray(gameObject);
        }

    }
}