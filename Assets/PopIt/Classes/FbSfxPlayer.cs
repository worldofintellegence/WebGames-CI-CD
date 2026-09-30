using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopIt
{
    public class FbSfxPlayer : MonoBehaviour
    {
        /// <summary>
        /// A general purpose audio class that is able to play different audio sfxs that are being requested by other classes.
        /// </summary>

        public static FbSfxPlayer instance;
        public AudioClip[] availableSfx;
        public AudioClip[] availableAnnounce;

        private AudioSource aso;

        private void Awake()
        {
            instance = this;
            aso = GetComponent<AudioSource>();

            DontDestroyOnLoad(this.gameObject);
        }

        public void PlaySfx(int sfxID = -1, float vol = 1f)
        {
            int clipID = sfxID;
            if (clipID == -1)
                clipID = Random.Range(0, availableSfx.Length);

            if (FbMusicPlayer.globalSoundState)
                aso.PlayOneShot(availableSfx[clipID], vol);
        }

        public void PlayAnnounce(int sfxID = -1, float vol = 0.6f)
        {
            int clipID = sfxID;
            if (clipID == -1)
                clipID = Random.Range(0, availableAnnounce.Length);

            if (FbMusicPlayer.globalSoundState)
                aso.PlayOneShot(availableAnnounce[clipID], vol);
        }

    }
}