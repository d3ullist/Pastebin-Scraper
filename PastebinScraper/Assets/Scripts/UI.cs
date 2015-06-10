using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Main : MonoBehaviour
{
    public Color ButtonOn = new Color(0, 1, 0.3f);
    public Color ButtonOff = new Color(1, 0, 0.3f);

    public InputField CharLimit;

    [SerializeField]
    private InputField pasteSaveLocation = null; // Assign in editor

    private void Awake()
    {
        if (IsScrapingPastebin)
            Toggle(GameObject.Find("ISP"));
        if (IsDeletingSmallFiles)
            Toggle(GameObject.Find("IDSF"));
        pasteSaveLocation.text = pastebin.SaveLocation;

        // Add listener to catch the submit
        InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
        submitEvent.AddListener(SubmitChange);
        pasteSaveLocation.onEndEdit = submitEvent;

        // Add validation
        pasteSaveLocation.characterValidation = InputField.CharacterValidation.None;
    }

    private void SubmitChange(string text)
    {
        pastebin.SaveLocation = text;
        pastebin.isInitialized = false;
    }

    string TranlateBool(bool input)
    {
        if (input)
            return "ON";
        else
            return "OFF";
    }

    public void Toggle(GameObject tx)
    {
        var text = tx.GetComponent<Text>();

        switch (tx.name)
        {
            case "ISP":
                this.IsScrapingPastebin = !this.IsScrapingPastebin;
                if (this.IsScrapingPastebin)
                {
                    text.color = ButtonOn;
                    text.text = "Scrape Pastebin|| Next scrape in: ";
                }
                else 
                {
                    text.color = ButtonOff;
                    text.text = "Scrape Pastebin";
                }
                break;
            case "IDSF":
                this.IsDeletingSmallFiles = !this.IsDeletingSmallFiles;
                if (this.IsDeletingSmallFiles) text.color = new Color(0, 1, 0.3f);
                else text.color = new Color(1, 0, 0.3f);
                break;
        }
    }
}
