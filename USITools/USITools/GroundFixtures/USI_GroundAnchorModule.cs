using UnityEngine;

namespace USITools
{
    public class USI_GroundAnchorModule : PartModule
    {
        private const string PAW_GROUP_NAME = "usi-ground-anchor";
        private const string PAW_GROUP_DISPLAY_NAME = "USI Ground Anchor";

        private FixedJoint _anchorJoint;
        private string _notLandedMessage = "Vessel must be landed to enable ground anchor";

        #region KSP fields
        [KSPField(isPersistant = true)]
        private bool IsAnchored;
        #endregion

        #region KSP actions and events
#pragma warning disable IDE0060 // Remove unused parameter

        [KSPAction(guiName = "Disable ground anchor")]
        public void DisableAnchorAction(KSPActionParam param)
        {
            ToggleAnchor(false);
        }

        [KSPAction(guiName = "Enable ground anchor")]
        public void EnableAnchorAction(KSPActionParam param)
        {
            ToggleAnchor(true);
        }

        [KSPAction(guiName = "Toggle ground anchor")]
        public void ToggleAnchorAction(KSPActionParam param)
        {
            ToggleAnchor(!IsAnchored);
        }

        [KSPEvent(
            guiName = "Ground anchor",
            guiActive = true,
            guiActiveEditor = false,
            guiActiveUnfocused = true,
            unfocusedRange = 5.0f,
            groupName = PAW_GROUP_NAME,
            groupDisplayName = PAW_GROUP_DISPLAY_NAME)]
        public void ToggleAnchorEvent()
        {
            ToggleAnchor(!IsAnchored);
        }

        [KSPEvent(
            guiName = "Enable all ground anchors",
            guiActive = true,
            guiActiveEditor = false,
            guiActiveUnfocused = true,
            unfocusedRange = 5.0f,
            groupName = PAW_GROUP_NAME,
            groupDisplayName = PAW_GROUP_DISPLAY_NAME)]
        public void EnableAllEvent()
        {
            ToggleAllAnchors(true);
        }

        [KSPEvent(
            guiName = "Disable all ground anchors",
            guiActive = true,
            guiActiveEditor = false,
            guiActiveUnfocused = true,
            unfocusedRange = 5.0f,
            groupName = PAW_GROUP_NAME,
            groupDisplayName = PAW_GROUP_DISPLAY_NAME)]
        public void DisableAllEvent()
        {
            ToggleAllAnchors(false);
        }

#pragma warning restore IDE0060 // Remove unused parameter
        #endregion

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                ToggleJoint(IsAnchored);
                UpdatePAW(IsAnchored);
            }
        }

        private void ToggleAllAnchors(bool isActive)
        {
            ToggleAnchor(isActive);

            var anchors = vessel.FindPartModulesImplementing<USI_GroundAnchorModule>();
            for (int i = 0; i < anchors.Count; i++)
            {
                anchors[i].ToggleAnchor(isActive);
            }
        }

        public void ToggleAnchor(bool isActive)
        {
            if (IsAnchored != isActive)
            {
                ToggleJoint(isActive);
                UpdatePAW(isActive);
                IsAnchored = isActive;
            }
        }

        private void ToggleJoint(bool isActive)
        {
            if (isActive && _anchorJoint == null)
            {
                if (!vessel.LandedOrSplashed)
                {
                    ScreenMessages.PostScreenMessage(
                        _notLandedMessage,
                        5.0f,
                        ScreenMessageStyle.UPPER_CENTER);
                }
                else
                {
                    _anchorJoint = gameObject.AddComponent<FixedJoint>();
                    _anchorJoint.enableCollision = true;
                    _anchorJoint.autoConfigureConnectedAnchor = true;
                    _anchorJoint.breakForce = float.PositiveInfinity;
                    _anchorJoint.breakTorque = float.PositiveInfinity;
                }
            }
            else if (!isActive && _anchorJoint != null)
            {
                Destroy(_anchorJoint);
                _anchorJoint = null;
            }
        }

        private void UpdatePAW(bool isActive)
        {
            Events[nameof(ToggleAnchorEvent)].guiName
                = $"Ground anchor: {(isActive ? "On" : "Off")}";
            MonoUtilities.RefreshPartContextWindow(part);
        }
    }
}
