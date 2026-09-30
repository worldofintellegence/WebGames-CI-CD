using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PopIt
{
    public class UIManager : MonoBehaviour
    {
        /// <summary>
        /// UIManager handles everything related to ingame UI elements and populating data on UI at any given time.
        /// </summary>

        public static UIManager instance;

        [Header("IngameUI Elements")]
        public GameObject ingameMessagePrefab;
        public GameObject countdownPanel;
        public GameObject resultPanel;
        public Text resultPanelWinnerNameUI;
        public GameObject clickBlockerPanel;
        public GameObject turnUIMain;
        public Text turnUI;
        public GameObject popEffect;

        [Header("General Resources")]
        public Sprite[] buttonNormalSprites;
        public Sprite[] buttonSelectedSprites;
        public Sprite[] buttonFinalizedSprites;

        [Header("Turn Settings")]
        public Button turnChangeButton;

        [Header("Other Settings")]
        public GameObject canvasHolder;
        private bool canShowMessage;

        private void Awake()
        {
            instance = this;

            //Init
            canShowMessage = true;
            countdownPanel.SetActive(false);
            resultPanel.SetActive(false);
            clickBlockerPanel.SetActive(false);
            turnUIMain.SetActive(false);
        }

        void Start()
        {
            //Hidden by default
            HideTurnChangeButton();
        }

        /// <summary>
        /// Display active player turn on top of game scene
        /// </summary>
        /// <param name="_turnBy"></param>
        public void DisplayTurnByOnUI(string _turnBy)
        {
            turnUIMain.SetActive(true);
            turnUI.text = _turnBy + "'s Turn";
        }

        /// <summary>
        /// Temporarily avoid players from being able to click on buttons
        /// </summary>
        public void DisablePlayerClicksOnButtons()
        {
            clickBlockerPanel.SetActive(true);
        }

        /// <summary>
        /// Restore players ability to click on buttons
        /// </summary>
        public void RestorePlayerClicksOnButtons()
        {
            clickBlockerPanel.SetActive(false);
        }

        /// <summary>
        /// Show/Hide end turn button when its player's turn
        /// </summary>
        public void UpdateTurnButtonStatus()
        {
            if (GameManager.instance.sessionSelectedButtons.Count > 0)
                DisplayTurnChangeButton();
            else
                HideTurnChangeButton();
        }

        public void DisplayTurnChangeButton()
        {
            turnChangeButton.gameObject.SetActive(true);
            turnChangeButton.GetComponent<Animator>().Play("PopIn");
        }

        public void HideTurnChangeButton()
        {
            turnChangeButton.GetComponent<Animator>().Play("PopOut");
            Invoke("HideTurnChangeButtonDelayed", 0.4f);
        }

        public void HideTurnChangeButtonDelayed()
        {
            turnChangeButton.gameObject.SetActive(false);
        }

        public Sprite GetButtonSprite(int buttonStateID, int buttonColorID)
        {
            //print(buttonStateID + " / " + buttonColorID);

            Sprite[] selectSpriteArray = buttonNormalSprites;

            if (buttonStateID == 0)
                selectSpriteArray = buttonNormalSprites;
            else if (buttonStateID == 1)
                selectSpriteArray = buttonSelectedSprites;
            else if (buttonStateID == 2)
                selectSpriteArray = buttonFinalizedSprites;

            return selectSpriteArray[buttonColorID];
        }

        /// <summary>
        /// Display a nice looking informative message on bottom UI
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="name"></param>
        /// <param name="textToShow"></param>
        /// <param name="forceSpawn"></param>
        /// <param name="customRot"></param>
        /// <param name="makeParent"></param>
        /// <param name="parent"></param>
        public void DiplayIngameMessage(Vector3 pos, string name = "ingameText", string textToShow = "TEST", bool forceSpawn = false, float customRot = 0, bool makeParent = false, GameObject parent = null)
        {
            if (!canShowMessage && !forceSpawn)
                return;
            canShowMessage = false;

            GameObject p = Instantiate(ingameMessagePrefab, pos, Quaternion.Euler(customRot, 0, 0)) as GameObject;
            p.name = name;
            p.GetComponent<IngameMessage>().textToShow = "" + textToShow;

            if (makeParent)
                p.transform.parent = parent.transform;

            p.GetComponent<RectTransform>().anchoredPosition = pos;

            StartCoroutine(EnableShowingIngameMessage());
        }

        public IEnumerator EnableShowingIngameMessage()
        {
            yield return new WaitForSeconds(0.5f);
            canShowMessage = true;
        }

        public void DisplayCountdown()
        {
            countdownPanel.SetActive(true);
            countdownPanel.GetComponent<Animator>().Play("Countdown");
            Invoke("HideCountdownPanel", 4.1f);
        }

        public void HideCountdownPanel()
        {
            countdownPanel.SetActive(false);
        }

        public void DisplayGameoverPanel(string winnerName)
        {
            resultPanel.SetActive(true);
            resultPanel.GetComponent<Animator>().Play("ResultPanel");
            resultPanelWinnerNameUI.text = "" + winnerName;
        }

        public void LoadMenu()
        {
            FbSfxPlayer.instance.PlaySfx(0);
            SceneManager.LoadScene("Menu");
        }

        /// <summary>
        /// Display a nice haptic effect when player or AI clicks on a button
        /// </summary>
        /// <param name="go"></param>
        public void CreateButtonPopEffect(GameObject go)
        {
            GameObject p = Instantiate(popEffect, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
            p.name = "PopEffect";
            p.transform.parent = go.transform;
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            p.transform.parent = canvasHolder.transform;
            p.transform.localScale = new Vector3(1, 1, 1);
        }

    }
}