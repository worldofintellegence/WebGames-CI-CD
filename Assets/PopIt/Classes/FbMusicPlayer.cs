using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopIt
{
    public class FbMusicPlayer : MonoBehaviour
    {
        public static FbMusicPlayer instance;
        public AudioClip bgm;
        public static bool globalSoundState;

        void Awake()
        {
            //globalSoundState = true;
            DontDestroyOnLoad(this);
            instance = this;

            //Enable audio & music in the first run
            if (!PlayerPrefs.HasKey("Inited"))
            {
                print("Game is Inited!");
                PlayerPrefs.SetInt("Inited", 1);
                PlayerPrefs.SetInt("SoundState", 1);
                PlayerPrefs.SetInt("MusicState", 1);
            }

            //fetch saved status
            //Sound
            if (PlayerPrefs.GetInt("SoundState") == 1)
                globalSoundState = true;
            else
                globalSoundState = false;
            //music
            if (PlayerPrefs.GetInt("MusicState") == 0)
                ToggleMusic();
        }

        void Start()
        {
            playSfx(bgm);
        }

        void Update()
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                playSfx(bgm);
            }
        }

        void playSfx(AudioClip _clip)
        {
            GetComponent<AudioSource>().clip = _clip;
            GetComponent<AudioSource>().Play();
        }

        public void ToggleMusic()
        {
            GetComponent<AudioSource>().mute = !GetComponent<AudioSource>().mute;
        }

        public void ToggleSound()
        {
            globalSoundState = !globalSoundState;
        }
    }
}