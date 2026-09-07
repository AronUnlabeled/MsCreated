using TMPro;
using UnityEngine;

public class InteractUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private Color textColor;

    private void OnEnable() {
        PlayerInteraction.OnInteract += ShowPrompt;
        PlayerInteraction.OnHide += HidePrompt;
    }
    private void OnDisable() {
        PlayerInteraction.OnInteract -= ShowPrompt;
        PlayerInteraction.OnHide -= HidePrompt;
    }
    private void Start() => HidePrompt();
    public void ShowPrompt(string message) {

        promptText.text = message;
        promptText.color = textColor;
        promptText.gameObject.SetActive(true);
    }
    public void HidePrompt() {

        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}