using UnityEngine;
using TMPro;

public class LorePanel : MonoBehaviour
{
    public TextMeshProUGUI textField;

    public void SetText(string text)
    {
        textField.text = text;
    }
}
