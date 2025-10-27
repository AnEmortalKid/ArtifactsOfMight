using UnityEngine;

namespace ArtifactsOfMight.DraftArtifact.Game
{
    /// <summary>
    /// Client (non-host) only component used to track our expected count of items on a given pickup picker controller.
    /// 
    /// When we first interact with a controller, we will fire SetOptions from NetworkUIPromptController.OnDisplayBegin
    /// SetOptions (34) - original set
    /// 
    /// Then the network sync path will trigger PickupPickerController.OnDeserialize
    /// SetOptions (8) - filtered (7 + corrupt white)
    /// 
    /// During that first panel opening, the panel first built itself with 34 items, and then got the 8 item event,
    /// the internal model did change for us but the UI was still scaled to accomodate the bigger set
    /// 
    /// Our computed incorrect rect had Rect: w=544.0 h=1024.0 Children: 34
    /// The correct sizing on 8 should have been Rect: w=544.0 h=544.0 Children: 8
    /// 
    /// On a second open of the panel, things had the right layout. 
    /// Resizing the panel live did not work, though i did not try a hook like in LookingGlass for panel displaying
    /// 
    /// Instead we opted to do some client side prediction on the count, on the first invocation of set options,
    /// if the clients expected count does not match the server's sent count, we use our predicted items
    /// Future invocations the server will send the right count
    /// </summary>
    public class ClientPickerPanelProtection : MonoBehaviour
    {
        /// <summary>
        /// How many items we expect to receive for a picker based on our loadout
        /// </summary>
        public int ClientExpectedLength;
    }
}
