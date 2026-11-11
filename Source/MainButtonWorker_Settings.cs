using RimWorld;
using Verse;

namespace Blues
{
    /// <summary>
    /// Optional shortcut to the mod's own settings dialog. Visibility is controlled solely by
    /// MainButtonDef.buttonVisible, so customization mods can reveal it and persist that choice.
    /// </summary>
    public class MainButtonWorker_Settings : MainButtonWorker
    {
        public override void Activate()
        {
            Find.WindowStack.Add(new Dialog_ModSettings(LoadedModManager.GetMod<BluesJoys>()));
        }
    }
}
