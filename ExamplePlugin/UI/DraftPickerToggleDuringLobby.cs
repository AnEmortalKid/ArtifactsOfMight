using RoR2.UI;
using UnityEngine;
using RoR2;
using UnityEngine.SceneManagement;
using ExamplePlugin;

public class DraftPickerToggleDuringLobby : MonoBehaviour
{
    public GameObject RootObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {

            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != "lobby")
            {
                Log.Warning("DraftPickerToggleDuringLobby in scene " + sceneName);
                return;
            }

            Log.Info("DraftPickerToggle Respond F3");
            if(!RootObject)
            {
                Log.Warning("DraftPickerToggleDuringLobby no RootObject");
                return;
            }
            var active = RootObject.gameObject.activeSelf;
            RootObject.gameObject.SetActive(!active);

            // Ensure raycasts work when shown
            // TODO figure out canvas group
            //var cg = RootCanvas.GetComponent<CanvasGroup>() ?? RootCanvas.gameObject.AddComponent<CanvasGroup>();
            //cg.interactable = !active;
            //cg.blocksRaycasts = !active;
        }
    }
}
