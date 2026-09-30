using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace PopIt
{
    public class InitManager : MonoBehaviour
    {
        public void Awake()
        {
            print("<b>INITED!</b>");
            //PlayerPrefs.DeleteAll();
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.02f);
            SceneManager.LoadScene("Menu");
        }
    }
}