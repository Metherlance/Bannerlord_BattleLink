using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;


namespace BattleLink.Common
{
    // just for load of assemblies (register)...
    public class SubModule : MBSubModuleBase
    {
        private static readonly Color green = Color.FromUint(0x008000);


        protected override void OnSubModuleLoad()
        {
            InformationManager.DisplayMessage(new InformationMessage("BattleLink - Common - OnSubModuleLoad", green));
        }
    }
}


