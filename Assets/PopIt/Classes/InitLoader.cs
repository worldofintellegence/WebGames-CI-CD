using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PopIt
{
    public class InitLoader : MonoBehaviour
    {
        /// <summary>
        /// We always need to start the game from Init scene in order to make sure all classes are fully inited the way we need.
        /// </summary>

        void Awake()
        {
            if (!GameObject.FindGameObjectWithTag("MusicPlayer"))
                SceneManager.LoadScene("Init");
        }
    }
}