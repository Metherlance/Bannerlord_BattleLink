using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;


namespace BattleLink.CommonSvMp
{
    // just for load of assemblies (register)... for load Handler
    public class SubModule : MBSubModuleBase
    {
        private static readonly Color green = Color.FromUint(0x008000);


        protected override void OnSubModuleLoad()
        {
            InformationManager.DisplayMessage(new InformationMessage("BattleLink - CommonSvMp - OnSubModuleLoad", green));
        }
    }
}


