using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopIt
{
    public class BackgroundSwitcher : MonoBehaviour
    {
        private Image img;

        public Sprite[] availableBackgrounds;
        private int rndBackgroundID;

        private void Awake()
        {
            img = GetComponent<Image>();
        }

        void Start()
        {
            SetNewBackground();
        }

        public void SetNewBackground()
        {
            rndBackgroundID = Random.Range(0, availableBackgrounds.Length);
            img.sprite = availableBackgrounds[rndBackgroundID];
        }
    }
}