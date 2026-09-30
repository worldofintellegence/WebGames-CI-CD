using UnityEngine;
using System.Collections;

namespace PopIt
{
    public class CamShake : MonoBehaviour
    {
        public static CamShake instance;
        private Transform camTransform;
        internal float decreaseFactor = 1.0f;
        private Vector3 originalPos;
        private float p = 0;
        private bool isShaking;

        void Awake()
        {
            instance = this;
            isShaking = false;
            camTransform = GetComponent(typeof(Transform)) as Transform;
            originalPos = camTransform.localPosition;
        }

        void Start()
        {
            //StartCoroutine(Shake (0.8f, 0.1f));
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.S))
            //    PublicShake(0.8f, 0.3f);
            //ConstantShake();
        }

        public void ConstantShake()
        {
            float power = 0.1f;
            //for static shake
            transform.localPosition = new Vector3(
                originalPos.x + Random.insideUnitSphere.x * power,
                originalPos.y + Random.insideUnitSphere.y * power,
                transform.localPosition.z
                );
        }

        /// <summary>
        /// this shake is Static/Dynamic.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public IEnumerator Shake(float duration, float power)
        {
            if (isShaking)
                yield break;
            isShaking = true;

            originalPos = camTransform.localPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime * decreaseFactor;
                p = Mathf.SmoothStep(power, 0.01f, t / duration);
                //print("Time/P: " + t + "-" + p);

                //for static shake
                transform.localPosition = new Vector3(
                    originalPos.x + Random.insideUnitSphere.x * p,
                    originalPos.y + Random.insideUnitSphere.y * p,
                    transform.localPosition.z
                    );

                yield return 0;
            }

            if (t >= duration)
            {
                transform.localPosition = new Vector3(originalPos.x, originalPos.y, transform.localPosition.z);
                isShaking = false;
            }
        }

        public void PublicShake(float d, float p)
        {
            StartCoroutine(Shake(d, p));
        }

        public void ForceShake(float d, float p)
        {
            isShaking = false;
            StartCoroutine(Shake(d, p));
        }
    }
}