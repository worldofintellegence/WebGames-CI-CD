using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopIt
{
    public class ButtonStateController : MonoBehaviour
    {
        /// <summary>
        /// This is used for Audio & Music buttons, so save their states and update their icons with new user settings.
        /// </summary>

        public Sprite[] availableStates;
        public GameObject buttonImage;
        private Image r;
        private int currentState;
        public string prefsCode = "";

        // Start is called before the first frame update
        void Start()
        {
            //currentState = 1;
            currentState = PlayerPrefs.GetInt(prefsCode, 1);

            r = buttonImage.GetComponent<Image>();
            r.sprite = availableStates[currentState];
        }

        public void ChangeButtonState()
        {
            if (currentState == 0)
                currentState = 1;
            else
                currentState = 0;

            PlayerPrefs.SetInt(prefsCode, currentState);
            r.sprite = availableStates[currentState];

            FbSfxPlayer.instance.PlaySfx(0);
        }

        public void ChangeSoundState()
        {
            FbMusicPlayer.instance.ToggleSound();
        }

        public void ChangeMusicState()
        {
            FbMusicPlayer.instance.ToggleMusic();
        }

    }
}