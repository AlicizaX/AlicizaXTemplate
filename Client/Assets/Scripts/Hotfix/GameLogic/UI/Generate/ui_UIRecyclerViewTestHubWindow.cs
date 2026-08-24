using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [UIRes(ui_UIRecyclerViewTestHubWindow.ResTag, EUIResLoadType.AssetBundle)]
    public partial class ui_UIRecyclerViewTestHubWindow : UIHolderObjectBase
    {
        public const string ResTag = "UIRecyclerViewTestHubWindow";

        [SerializeField] private UXButton mBtnLinear;
        public UXButton BtnLinear => mBtnLinear;

        [SerializeField] private UXButton mBtnGrid;
        public UXButton BtnGrid => mBtnGrid;

        [SerializeField] private UXButton mBtnChat;
        public UXButton BtnChat => mBtnChat;

        [SerializeField] private UXButton mBtnMixed;
        public UXButton BtnMixed => mBtnMixed;

        [SerializeField] private UXButton mBtnGroup;
        public UXButton BtnGroup => mBtnGroup;

        [SerializeField] private UXButton mBtnLoop;
        public UXButton BtnLoop => mBtnLoop;

        [SerializeField] private UXButton mBtnPage;
        public UXButton BtnPage => mBtnPage;

        [SerializeField] private UXButton mBtnCircle;
        public UXButton BtnCircle => mBtnCircle;

        [SerializeField] private UXButton mBtnClose;
        public UXButton BtnClose => mBtnClose;
    }
}
