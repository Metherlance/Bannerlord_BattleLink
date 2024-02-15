using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Layout;

namespace BattleLink.Client.Widgets
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