using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Part3_EscapeRoom
{
    public class CodeInputPopup : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text titleText;

        public event Action<string> OnCodeSubmitted;

        public void Bind(GameObject panel, TMP_InputField input, Button ok, Button cancel, TMP_Text title)
        {
            root = panel;
            inputField = input;
            okButton = ok;
            cancelButton = cancel;
            titleText = title;
            WireButtons();
            Hide();
        }

        private void Awake()
        {
            WireButtons();
            Hide();
        }

        private void WireButtons()
        {
            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(Submit);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(Hide);
            }
        }

        public void Show(string title = "Enter safe code")
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (inputField != null)
            {
                inputField.text = string.Empty;
                inputField.Select();
                inputField.ActivateInputField();
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

        private void Submit()
        {
            var code = inputField != null ? inputField.text : string.Empty;
            Hide();
            OnCodeSubmitted?.Invoke(code);
        }
    }
}
