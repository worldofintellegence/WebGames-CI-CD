using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopIt
{
    public class IngameMessage : MonoBehaviour
    {
        /// <summary>
        /// A simple UI message system that shows informative data to players using simple animations.
        /// </summary>

        public GameObject holder;
        public Text messageUI;
        private Animator ani;
        internal string textToShow = "...";
        private GameObject hud;
        private RectTransform rt;
        private float lifetime = 2f;


        private void Awake()
        {
            holder.SetActive(false);
            ani = GetComponent<Animator>();

            rt = GetComponent<RectTransform>();
            hud = GameObject.FindGameObjectWithTag("BottomHUD");
            if (hud && transform.parent == null)
            {
                transform.SetParent(hud.transform, false);
                rt.anchoredPosition = new Vector2(0, 0);
            }
        }


        void Start()
        {
            UpdateText();
            RunTextAnimation();
            Destroy(gameObject, lifetime);
        }

        public void UpdateText()
        {
            messageUI.text = "" + textToShow;
        }

        public void RunTextAnimation()
        {
            ani.Play("IngameMessage");
            holder.SetActive(true);
        }

    }
}