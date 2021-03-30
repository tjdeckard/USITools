using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace USITools
{
    public class USI_BaseLeveler : PartModule
    {
        private const string PAW_GROUP_NAME = "usi-base-leveler";
        private const string PAW_GROUP_DISPLAY_NAME = "USI Leveler";

        private USI_GroundAnchorModule _groundAnchor;
        private Transform _groundContactTransform;
        private string _noGroundAnchorMessage
            = "Vessel must have a part with ground anchor to activate leveler";
        private string _notLandedMessage = "Vessel must be landed to activate leveler";

        #region KSP fields
        private string GroundContactTransformName;
        #endregion

        #region KSP actions and events
        [KSPAction(guiName = "Level now")]
        public void LevelAction(KSPActionParam param)
        {
            Level();
        }

        [KSPEvent(
            guiName = "Level now",
            guiActive = true,
            guiActiveEditor = false,
            guiActiveUnfocused = true,
            unfocusedRange = 5.0f,
            groupName = PAW_GROUP_NAME,
            groupDisplayName = PAW_GROUP_DISPLAY_NAME)]
        public void LevelEvent()
        {
            Level();
        }
        #endregion

        private void Level()
        {
            if (_groundAnchor == null)
            {
                ScreenMessages.PostScreenMessage(
                    _noGroundAnchorMessage,
                    5.0f,
                    ScreenMessageStyle.UPPER_CENTER);
            }
            else if (!vessel.LandedOrSplashed)
            {
                ScreenMessages.PostScreenMessage(
                    _notLandedMessage,
                    5.0f,
                    ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                _groundAnchor.DisableAllEvent();

            }
        }

        public override void OnAwake()
        {
            base.OnAwake();

            if (!string.IsNullOrEmpty(GroundContactTransformName))
            {
                var groundContactGO = GameObject.Find(GroundContactTransformName);
                _groundContactTransform = groundContactGO.transform ?? part.transform;
            }
            else
            {
                _groundContactTransform = part.transform;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                _groundAnchor = vessel.FindPartModulesImplementing<USI_GroundAnchorModule>()
                    .FirstOrDefault();
            }
        }
    }
}
