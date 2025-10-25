using RoR2;
using UnityEngine;

namespace ArtifactsOfMight.DraftArtifact.Game
{
    /// <summary>
    /// Server side store that tracks the original options for a PickupPickerController
    /// So options are re-filtered fresh on each user activation
    /// </summary>
    public class PickerState : MonoBehaviour
    {
        public PickupPickerController.Option[] originalOptions;
    }
}
