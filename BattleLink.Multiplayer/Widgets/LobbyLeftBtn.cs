using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Layout;

namespace BattleLink.Client.Widgets
{
    /// <summary>
    /// </summary>
    class LobbyLeftBtn : ButtonWidget
    {
        public LobbyLeftBtn(UIContext context, string label, string brush, Action<Widget> action) : base(context)
        {
            ClickEventHandlers.Add(action);
            DoNotPassEventsToChildren = true;
            UpdateChildrenStates = true;
            WidthSizePolicy = SizePolicy.CoverChildren;
            HeightSizePolicy = SizePolicy.CoverChildren;
            SuggestedWidth = 97;
            SuggestedHeight = 74;
            HorizontalAlignment = HorizontalAlignment.Center;
            PositionYOffset = 15;

            var listPanel = new ListPanel(context);
            listPanel.WidthSizePolicy = SizePolicy.CoverChildren;
            listPanel.HeightSizePolicy = SizePolicy.CoverChildren;
            listPanel.MarginTop = 45;
            listPanel.UpdateChildrenStates = true;
            listPanel.HorizontalAlignment = HorizontalAlignment.Center;
            // LayoutImp.LayoutMethod -> StackLayout.LayoutMethod
            listPanel.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;

            var btn = new ButtonWidget(context);
            btn.WidthSizePolicy = SizePolicy.Fixed;
            btn.HeightSizePolicy = SizePolicy.Fixed;
            btn.SuggestedWidth = 97;
            btn.SuggestedHeight = 74;
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.PositionYOffset = 15;
            btn.Brush = context.GetBrush(brush);

            var txt = new TextWidget(context);
            txt.WidthSizePolicy = SizePolicy.CoverChildren;
            txt.HeightSizePolicy = SizePolicy.Fixed;
            txt.SuggestedHeight = 50;
            //txt.Text = TextUtils.translate(label);
            txt.HorizontalAlignment = HorizontalAlignment.Center;
            //txt.Brush = context.GetBrush("MPLobby.Matchmaking.LeftMenuText");

            // place elements

            AddChild(listPanel);
            listPanel.AddChild(btn);
            listPanel.AddChild(txt);

        }
    }
}