using AlicizaX.UI.Runtime;
using Game.UI;

namespace GameLogic.UI
{
    [Window(UILayer.UI, 0)]
    public sealed class UIRecyclerViewTestHubWindow : UIWindow<ui_UIRecyclerViewTestHubWindow>
    {
        protected override void OnInitialize()
        {
            baseui.BtnLinear.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Linear));
            baseui.BtnGrid.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Grid));
            baseui.BtnChat.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Chat));
            baseui.BtnMixed.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Mixed));
            baseui.BtnGroup.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Group));
            baseui.BtnLoop.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Loop));
            baseui.BtnPage.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Page));
            baseui.BtnCircle.onClick.AddListener(() => OpenExample(RecyclerViewExampleKind.Circle));
            baseui.BtnClose.onClick.AddListener(() => CloseSelf());
        }

        private static void OpenExample(RecyclerViewExampleKind kind)
        {
            GameApp.UI.ShowUI<UIRecyclerViewExampleWindow>(kind);
        }
    }
}
