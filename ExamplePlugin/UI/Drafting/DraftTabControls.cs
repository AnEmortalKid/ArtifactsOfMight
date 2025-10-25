using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
{
    /// <summary>
    /// Wraper around our controls view, just handles notifying events and receiving state updates
    /// </summary>
    public class DraftTabControls : MonoBehaviour
    {
        private Toggle modeToggle;


        /// <summary>
        /// Game object that encompasses the - # + and dice buttons
        /// </summary>
        private GameObject restrictionOptionsGroup;


        private Button decreaseButton;
        private TextMeshProUGUI limitLabel;
        private Button increaseButton;

        private TextMeshProUGUI selectionsLabel;

        private Button diceButton;

        /// <summary>
        /// Event that will fire when the restrict mode is changed
        /// </summary>
        public Action<bool> OnRestrictModeChanged;

        public Action OnDecreaseItemLimit;
        public Action OnIncreaseItemLimit;

        public Action OnRandomizeTier;

        public void BindModeToggle(Toggle modeToggle)
        {
            this.modeToggle = modeToggle;

            // TODO bind the thing

            this.modeToggle.onValueChanged.AddListener(HandleValueChanged);
        }

        public void BindRestrictionOptionsGroup(GameObject restrictionOptionsGroup)
        {
            this.restrictionOptionsGroup = restrictionOptionsGroup;
        }

        public void BindItemLimitElements(Button decreaseButton, TextMeshProUGUI limitLabel, Button increaseButton)
        {
            this.decreaseButton = decreaseButton;
            this.limitLabel = limitLabel;
            this.increaseButton = increaseButton;

            this.decreaseButton.onClick.AddListener(HandleDecreaseRequest);
            this.increaseButton.onClick.AddListener(HandleIncreaseRequest);
        }

        public void BindItemSelectionElements(TextMeshProUGUI selectionLabel)
        {
            this.selectionsLabel = selectionLabel;
        }

        public void BindDiceButton(Button diceButton)
        {
            this.diceButton = diceButton;

            this.diceButton.onClick.AddListener(HandleRandomizeRequest);
        }

        public void SetItemLimitCount(int currentCount)
        {
            limitLabel.text = currentCount.ToString();
        }

        public void SetSelectionState(int picked, int limit)
        {
            selectionsLabel.text = "Selected: " + picked.ToString() + "/" + limit.ToString();
        }

        public void HideRestrictionOptions()
        {
            restrictionOptionsGroup.gameObject.SetActive(false);
        }

        public void ShowRestrictionOptions()
        {
            restrictionOptionsGroup.gameObject.SetActive(true);
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        void OnDestroy()
        {
            // TODO
            //this.modeButton.onClick.RemoveListener(HandleModeClick);
        }

        private void HandleValueChanged(bool isChecked)
        {
            Log.Info($"[DraftArtifact] mode toggle: {isChecked}");
            // TODO flex requests then
            OnRestrictModeChanged?.Invoke(isChecked);
        }

        private void HandleIncreaseRequest()
        {
            Log.Info($"[DraftArtifact] increase item count");
            OnIncreaseItemLimit?.Invoke();
        }

        private void HandleDecreaseRequest()
        {
            Log.Info($"[DraftArtifact] decrease item count");
            OnDecreaseItemLimit?.Invoke();
        }

        private void HandleRandomizeRequest()
        {
            Log.Info($"[DraftArtifact] dice roll");
            OnRandomizeTier?.Invoke();
        }
    }
}
