using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Localization;

namespace RealmsBattle.Client.Widgets
{
    /// <summary>
    /// </summary>
    class VBox : ListPanel
    {
        public VBox(UIContext context) : base(context)
        {
            this.WidthSizePolicy = SizePolicy.StretchToParent;
            this.HeightSizePolicy = SizePolicy.CoverChildren;
            this.MarginTop = 100;
            this.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;
        }
    }
}