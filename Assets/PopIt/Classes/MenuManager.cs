using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PopIt
{
    public class MenuManager : MonoBehaviour
    {
        /// <summary>
        /// A simple calss that handles everything that needs to happen inside menu scene, including showing/hiding panels, playing sound effects and playing animations
        /// </summary>

        public GameObject settingsPanel;
        public GameObject helpPanel;
        private Animator hpAnim;
        private bool canClick;

        private void Awake()
        {
            hpAnim = helpPanel.GetComponent<Animator>();
            helpPanel.SetActive(false);
            canClick = true;
        }

        public void PlaySinglePlayer()
        {
            FbSfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetInt("GameMode", 1);
            StartTheGame();
        }

        public void PlayTwoPlayers()
        {
            FbSfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetInt("GameMode", 2);
            StartTheGame();
        }

        public void StartTheGame()
        {
            if (!canClick)
                return;
            canClick = false;

            SceneManager.LoadScene("Game");
        }

        public void DisplayHelpPanel()
        {
            FbSfxPlayer.instance.PlaySfx(0);
            helpPanel.SetActive(true);
            hpAnim.Play("HelpPanelIn");
        }

        public void HideHelpPanel()
        {
            FbSfxPlayer.instance.PlaySfx(3);
            hpAnim.Play("HelpPanelOut");
            StartCoroutine(HideHelpPanelCo());
        }

        public IEnumerator HideHelpPanelCo()
        {
            yield return new WaitForSeconds(0.5f);
            helpPanel.SetActive(false);
        }

        public void DisplaySettingsPanel()
        {
            FbSfxPlayer.instance.PlaySfx(0);
            settingsPanel.SetActive(true);
            settingsPanel.GetComponent<Animator>().Play("SettingPanelIn");
        }

        public void HideSettingsPanel()
        {
            FbSfxPlayer.instance.PlaySfx(3);
            settingsPanel.GetComponent<Animator>().Play("SettingPanelOut");
            StartCoroutine(HideSettingsPanelCo());
        }

        public IEnumerator HideSettingsPanelCo()
        {
            yield return new WaitForSeconds(0.5f);
            settingsPanel.SetActive(false);
        }
    }
}