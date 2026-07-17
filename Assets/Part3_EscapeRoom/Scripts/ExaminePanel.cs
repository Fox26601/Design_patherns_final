using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Part3_EscapeRoom
{
    public class ExaminePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button closeButton;

        public void Bind(GameObject panel, TMP_Text body, Button close)
        {
            root = panel;
            bodyText = body;
            closeButton = close;
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }

            Hide();
        }

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }

            Hide();
        }

        public void Show(string message)
        {
            if (bodyText != null)
            {
                bodyText.text = message;
            }

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public bool IsVisible => root != null && root.activeSelf;
    }
}
