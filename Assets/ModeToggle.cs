using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ModeToggle : MonoBehaviour
{
    public bool isDeleteMode = false;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI buttonText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void ToggleMode()
    {
        isDeleteMode = !isDeleteMode;

        // Change the text based on the mode
        if (isDeleteMode)
        {
            modeText.text = "Delete Mode"; // Change the text to "Delete Mode"
            buttonText.text = "Switch to Add";
        }
        else
        {
            modeText.text = "Add Mode"; // Change the text back to "Add Mode"
            buttonText.text = "Switch to Delete";
        }

        // Log the current mode
        Debug.Log("Current mode: " + (isDeleteMode ? "Delete Mode" : "Add Mode"));
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
