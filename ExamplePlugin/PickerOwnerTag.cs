using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExamplePlugin
{
    /// <summary>
    /// Component that just holds a reference to which user opened a picker currently,
    /// in order to then resolve their loadout
    /// </summary>
    public class PickerOwnerTag : MonoBehaviour
    {
        public UnityEngine.Networking.NetworkInstanceId openerNetUserId;
    }
}
