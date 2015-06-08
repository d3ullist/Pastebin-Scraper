using UnityEngine;
using UnityEngine.UI;

public partial class Main : MonoBehaviour
{
    public InputField CharLimit;

    [SerializeField]
    private InputField pasteSaveLocation = null; // Assign in editor

    private void Awake()
    {
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

    private int menuState = 1;

    void OnGUI()
    {
        GUI.Label(new Rect(675, 177.5f, 115, 30), "      Next Scrape:");
        GUI.Label(new Rect(675, 190, 115, 30), "         " + nextScrape);

        if (menuState == 4)
        {
            GUI.enabled = false;
            GUI.TextField(new Rect(5, 5, 660, 585),
                "I as the developer accept no liability for the uses and/or the consequences of any actions taken by the user \n" +
                "These consequences might include IP bans from given providers, and or termination of services by the providers \n" +
                "Which may be both temporary and/or permanent, incompliance with their terms of service \n" +
                "I as the developer accept no liability for damage caused by downloaded content or use of downloaded content \n" +
                "All content of this program should be used under the users discression, preferrably in a virtual enviroment \n" +
                "Do not rapidly reboot the program, not even if it crashes. Please maintain a 3+ minute of cooldown time \n" +
                "For questions regarding usage of this program, please contact the developer. \n\n" +
                "TL:DR USE AT YOUR OWN BLOODY RISK IDIOT");
            GUI.enabled = true;
        }
    }

    public void Toggle(GameObject tx)
    {
        var text = tx.GetComponent<Text>();

        switch (tx.name)
        {
            case "ISP":
                this.IsScrapingPastebin = !this.IsScrapingPastebin;
                if (this.IsScrapingPastebin) text.color = new Color(0, 1, 0.3f);
                else text.color = new Color(1, 0, 0.3f);
                break;
            case "IDSF":
                this.IsDeletingSmallFiles = !this.IsDeletingSmallFiles;
                if (this.IsDeletingSmallFiles) text.color = new Color(0, 1, 0.3f);
                else text.color = new Color(1, 0, 0.3f);
                break;
        }
    }
}
