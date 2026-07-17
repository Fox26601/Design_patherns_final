using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Part3_EscapeRoom
{
    public class MessageLogUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;
        [SerializeField] private int maxLines = 8;

        private readonly Queue<string> _lines = new();

        public void Bind(TMP_Text text)
        {
            logText = text;
        }

        public void Clear()
        {
            _lines.Clear();
            if (logText != null)
            {
                logText.text = string.Empty;
            }
        }

        public void AddMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            _lines.Enqueue(message);
            while (_lines.Count > maxLines)
            {
                _lines.Dequeue();
            }

            if (logText == null)
            {
                return;
            }

            var builder = new StringBuilder();
            foreach (var line in _lines)
            {
                builder.AppendLine("• " + line);
            }

            logText.text = builder.ToString().TrimEnd();
        }
    }
}
