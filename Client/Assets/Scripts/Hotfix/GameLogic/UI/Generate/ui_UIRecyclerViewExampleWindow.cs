using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [UIRes(ui_UIRecyclerViewExampleWindow.ResTag, EUIResLoadType.AssetBundle)]
    public partial class ui_UIRecyclerViewExampleWindow : UIHolderObjectBase
    {
        public const string ResTag = "UIRecyclerViewExampleWindow";

        [SerializeField] private UXButton mBtnClose;
        public UXButton BtnClose => mBtnClose;

        [SerializeField] private UXButton mBtnActionA;
        public UXButton BtnActionA => mBtnActionA;

        [SerializeField] private UXButton mBtnActionB;
        public UXButton BtnActionB => mBtnActionB;

        [SerializeField] private UXButton mBtnActionC;
        public UXButton BtnActionC => mBtnActionC;

        [SerializeField] private UXTextMeshPro mTextTitle;
        public UXTextMeshPro TextTitle => mTextTitle;

        [SerializeField] private UXTextMeshPro mTextStatus;
        public UXTextMeshPro TextStatus => mTextStatus;

        [SerializeField] private RecyclerView mScrollViewList;
        public RecyclerView ScrollViewList => mScrollViewList;

        [SerializeField] private RecyclerView mScrollViewLinear;
        public RecyclerView ScrollViewLinear => mScrollViewLinear;

        [SerializeField] private RecyclerView mScrollViewGrid;
        public RecyclerView ScrollViewGrid => mScrollViewGrid;

        [SerializeField] private RecyclerView mScrollViewChat;
        public RecyclerView ScrollViewChat => mScrollViewChat;

        [SerializeField] private RecyclerView mScrollViewMixed;
        public RecyclerView ScrollViewMixed => mScrollViewMixed;

        [SerializeField] private RecyclerView mScrollViewGroup;
        public RecyclerView ScrollViewGroup => mScrollViewGroup;

        [SerializeField] private RecyclerView mScrollViewLoop;
        public RecyclerView ScrollViewLoop => mScrollViewLoop;

        [SerializeField] private RecyclerView mScrollViewPage;
        public RecyclerView ScrollViewPage => mScrollViewPage;

        [SerializeField] private RecyclerView mScrollViewCircle;
        public RecyclerView ScrollViewCircle => mScrollViewCircle;
    }
}
