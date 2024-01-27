using UnityEngine;

namespace Characters.Player
{
    public class StealthComponent : MonoBehaviour
    {
        [SerializeField]
        private const int AcceptableNoticeability = 20;
        
        public bool IsNoticeable => IsInRestrictedArea || Noticeability > AcceptableNoticeability;
        public bool IsInRestrictedArea { get; private set; }
        public int Noticeability { get; set; }
        
        private Rigidbody2D _rigidbody;

        // Start is called before the first frame update
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("RestrictedArea"))
            {
                IsInRestrictedArea = true;
                Debug.Log("Entered restricted area");
                // Display notification about entering a restricted area
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("RestrictedArea"))
            {
                IsInRestrictedArea = false;
                Debug.Log("Left restricted area");
                // Remove the notification
            }
        }
    }
}
