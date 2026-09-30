using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopIt
{
    public class FastDestroy : MonoBehaviour
    {
        public float delay = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            Destroy(gameObject, delay);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}