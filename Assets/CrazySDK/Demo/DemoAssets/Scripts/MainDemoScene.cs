using UnityEngine;
using UnityEngine.UI;

namespace CrazyGames
{
    public class MainDemoScene : MonoBehaviour
    {
        public Text adblockText;
        public Text infoText;
        public GameObject initializedPanel, notInitializedPanel;


        private void Start()
        {
            initializedPanel.SetActive(false);
            notInitializedPanel.SetActive(true);
            CrazySDK.Init(() =>
            {
                infoText.text = $"SDK Version: {CrazySDK.Version} \n";
                infoText.text += $"SDK Initialized: {CrazySDK.IsInitialized} \n";
                infoText.text += $"Is QA Tool: {CrazySDK.IsQaTool} \n";
                infoText.text += $"Is user account available: {CrazySDK.User.IsUserAccountAvailable} \n";
                infoText.text += $"Environment: {CrazySDK.Environment} \n";
                infoText.text += $"Is on CrazyGames: {CrazySDK.IsOnCrazyGames} \n";

                CrazySDK.Ad.HasAdblock((adblockPresent) => { adblockText.text = "Has adblock: " + adblockPresent; });

                initializedPanel.SetActive(true);
                notInitializedPanel.SetActive(false);
            });
        }
    }
}