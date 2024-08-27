using System.Collections;
using System.Reflection;


namespace AnyRes
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class AR : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "AnyRes"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "AnyRes"; } }
        public override string DisplaySection { get { return "AnyRes"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomParameterUI("Use KSP Skin")]
        public bool useKSPSkin = true;

        [GameParameters.CustomParameterUI("Save Game Window position",
            toolTip = "Adds a button to save current window position, will be restored at KSP restart")]
        public bool saveWinPos = true;

        [GameParameters.CustomParameterUI("Modify UI Scale",
            toolTip = "Adds the option to configure and save the UI Scale alongside the screen resolution")]
        public bool useUIScale = false;



        public override void SetDifficultyPreset(GameParameters.Preset preset) { }

        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }

        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }

        public override IList ValidValues(MemberInfo member) { return null; }
    }
}
