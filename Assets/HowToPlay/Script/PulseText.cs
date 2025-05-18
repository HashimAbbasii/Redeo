using UnityEngine;
using TMPro;
public class PulseText : MonoBehaviour
{
        public TextMeshProUGUI tmpText;
        public float pulseSpeed = 2f;
        public float pulseAmount = 0.1f;
    
        private Vector3 originalScale;
    
        void Start()
        {
            originalScale = tmpText.transform.localScale;
        }
    
        void Update()
        {
            float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            tmpText.transform.localScale = originalScale * scale;
        }
}
