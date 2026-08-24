using System.Collections.Generic;
using System.Text;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{
    public enum RecyclerViewExampleKind
    {
        Linear,
        Grid,
        Chat,
        Mixed,
        Group,
        Loop,
        Page,
        Circle
    }

    [Window(UILayer.UI, 0)]
    public sealed class UIRecyclerViewExampleWindow : UIWindow<ui_UIRecyclerViewExampleWindow>
    {
        private static readonly Color[] Palette =
        {
            new(0.16f, 0.42f, 0.48f, 1f),
            new(0.22f, 0.36f, 0.22f, 1f),
            new(0.42f, 0.32f, 0.16f, 1f),
            new(0.32f, 0.22f, 0.42f, 1f)
        };

        private RecyclerViewExampleKind _kind;
        private UGList<RvTextData> _linearList;
        private UGList<RvTextData> _gridList;
        private UGList<RvTextData> _pageList;
        private UGList<RvTextData> _circleList;
        private UGList<RvChatData> _chatList;
        private UGMixedList<RvMixedData> _mixedList;
        private UGGroupList<RvGroupData> _groupList;
        private UGLoopList<RvTextData> _loopList;
        private bool _listsBound;
        private int _seq;
        private static UIRecyclerViewExampleWindow _current;

        protected override void OnInitialize()
        {
            baseui.BtnClose.onClick.AddListener(() => CloseSelf());
            baseui.BtnActionA.onClick.AddListener(OnActionA);
            baseui.BtnActionB.onClick.AddListener(OnActionB);
            baseui.BtnActionC.onClick.AddListener(OnActionC);
        }

        protected override void OnOpen()
        {
            _current = this;
            _kind = UserData is RecyclerViewExampleKind kind ? kind : RecyclerViewExampleKind.Linear;
            BindListsOnce();
            ShowExample();
        }

        protected override void OnClose()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        public static void ActivateGroup(int index)
        {
            _current?._groupList?.Activate(index);
        }

        private void BindListsOnce()
        {
            if (_listsBound)
            {
                return;
            }

            _linearList = UGListCreateHelper.Create<RvTextData>(baseui.ScrollViewLinear);
            _gridList = UGListCreateHelper.Create<RvTextData>(baseui.ScrollViewGrid);
            _pageList = UGListCreateHelper.Create<RvTextData>(baseui.ScrollViewPage);
            _circleList = UGListCreateHelper.Create<RvTextData>(baseui.ScrollViewCircle);
            _chatList = UGListCreateHelper.Create<RvChatData>(baseui.ScrollViewChat);
            _chatList.ApplyUpdateOptions(new ListUpdateOptions
            {
                ApplyRefreshMode = true,
                RefreshMode = ListRefreshMode.AfterSettle
            });
            _mixedList = UGListCreateHelper.CreateMixed<RvMixedData>(baseui.ScrollViewMixed);
            _groupList = UGListCreateHelper.CreateGroup<RvGroupData>(baseui.ScrollViewGroup, 0);
            _loopList = UGListCreateHelper.CreateLoop<RvTextData>(baseui.ScrollViewLoop);
            _listsBound = true;
        }

        private void ShowExample()
        {
            HideAllLists();
            SetTitle(_kind.ToString());
            switch (_kind)
            {
                case RecyclerViewExampleKind.Linear:
                    ShowLinear();
                    break;
                case RecyclerViewExampleKind.Grid:
                    ShowGrid();
                    break;
                case RecyclerViewExampleKind.Chat:
                    ShowChat();
                    break;
                case RecyclerViewExampleKind.Mixed:
                    ShowMixed();
                    break;
                case RecyclerViewExampleKind.Group:
                    ShowGroup();
                    break;
                case RecyclerViewExampleKind.Loop:
                    ShowLoop();
                    break;
                case RecyclerViewExampleKind.Page:
                    ShowPage();
                    break;
                case RecyclerViewExampleKind.Circle:
                    ShowCircle();
                    break;
            }
        }

        private void HideAllLists()
        {
            SetListActive(baseui.ScrollViewLinear, false);
            SetListActive(baseui.ScrollViewGrid, false);
            SetListActive(baseui.ScrollViewChat, false);
            SetListActive(baseui.ScrollViewMixed, false);
            SetListActive(baseui.ScrollViewGroup, false);
            SetListActive(baseui.ScrollViewLoop, false);
            SetListActive(baseui.ScrollViewPage, false);
            SetListActive(baseui.ScrollViewCircle, false);
        }

        private static void SetListActive(RecyclerView list, bool active)
        {
            if (list != null)
            {
                list.gameObject.SetActive(active);
            }
        }

        private void ShowLinear()
        {
            SetListActive(baseui.ScrollViewLinear, true);
            _linearList.SetData(CreateTextItems(40));
            SetActions("追加", "顶部插入", "滚到顶部");
            SetStatus("Linear 定高列表。");
        }

        private void ShowGrid()
        {
            SetListActive(baseui.ScrollViewGrid, true);
            _gridList.SetData(CreateTextItems(80));
            SetActions("追加", "删末尾", "滚到中间");
            SetStatus("Grid 2 列背包格。");
        }

        private void ShowChat()
        {
            SetListActive(baseui.ScrollViewChat, true);
            _chatList.SetData(CreateChatItems(20));
            _chatList.StickToEnd();
            SetActions("发消息", "加载历史", "改高首条");
            SetStatus("Chat 左右气泡。贴底发消息，顶部插入历史。");
        }

        private void ShowMixed()
        {
            SetListActive(baseui.ScrollViewMixed, true);
            _mixedList.SetData(CreateMixedItems(30));
            SetActions("追加短项", "追加长项", "刷新第1条");
            SetStatus("Mixed 多模板。短/中/长三种格子。");
        }

        private void ShowGroup()
        {
            SetListActive(baseui.ScrollViewGroup, true);
            _groupList.SetData(CreateGroupItems());
            _groupList.Expand(0);
            SetActions("展开第1组", "折叠第1组", "追加子项");
            SetStatus("Group 任务列表。点分类头展开/折叠。");
        }

        private void ShowLoop()
        {
            SetListActive(baseui.ScrollViewLoop, true);
            _loopList.SetData(CreateTextItems(12));
            SetActions("追加", "滚到第0条", "滚到中心");
            SetStatus("Loop 循环。滑过边缘会重锚。");
        }

        private void ShowPage()
        {
            SetListActive(baseui.ScrollViewPage, true);
            List<RvTextData> covers = new List<RvTextData>(8);
            for (int i = 0; i < 8; i++)
            {
                covers.Add(NextTextItem("Cover"));
            }

            _pageList.SetData(covers);
            SetActions("下一项", "上一项", "回到第0条");
            SetStatus("Page 翻页。近大远小。");
        }

        private void ShowCircle()
        {
            SetListActive(baseui.ScrollViewCircle, true);
            _circleList.SetData(CreateTextItems(10));
            SetActions("追加", "滚到第3条", "停滚");
            SetStatus("Circle 环形。可连续旋转。");
        }

        private void OnActionA()
        {
            switch (_kind)
            {
                case RecyclerViewExampleKind.Linear:
                    _linearList.Add(NextTextItem("Append"));
                    break;
                case RecyclerViewExampleKind.Grid:
                    _gridList.Add(NextTextItem("Append"));
                    _gridList.Add(NextTextItem("Append"));
                    _gridList.Add(NextTextItem("Append"));
                    _gridList.Add(NextTextItem("Append"));
                    break;
                case RecyclerViewExampleKind.Chat:
                    bool pin = _chatList.IsAtEnd;
                    _chatList.Add(NextChatItem("Send"));
                    if (pin)
                    {
                        _chatList.StickToEnd();
                    }
                    break;
                case RecyclerViewExampleKind.Mixed:
                    _mixedList.Add(new RvMixedData { TemplateId = 0, Text = "Short " + ++_seq });
                    break;
                case RecyclerViewExampleKind.Group:
                    _groupList.Expand(0);
                    break;
                case RecyclerViewExampleKind.Loop:
                    _loopList.Add(NextTextItem("Loop"));
                    break;
                case RecyclerViewExampleKind.Page:
                    _pageList.ScrollToIndex(Mathf.Min(_pageList.ScrollDataIndex + 1, _pageList.DataCount - 1), true);
                    break;
                case RecyclerViewExampleKind.Circle:
                    _circleList.Add(NextTextItem("Circle"));
                    break;
            }

            RefreshStatus();
        }

        private void OnActionB()
        {
            switch (_kind)
            {
                case RecyclerViewExampleKind.Linear:
                    _linearList.Insert(0, NextTextItem("Prepend"));
                    break;
                case RecyclerViewExampleKind.Grid:
                    if (_gridList.DataCount > 0)
                    {
                        _gridList.RemoveAt(_gridList.DataCount - 1);
                    }
                    break;
                case RecyclerViewExampleKind.Chat:
                    _chatList.InsertRange(0, new[]
                    {
                        NextChatItem("History A"),
                        NextChatItem("History B")
                    });
                    break;
                case RecyclerViewExampleKind.Mixed:
                    _mixedList.Add(new RvMixedData { TemplateId = 2, Text = "Tall " + ++_seq });
                    break;
                case RecyclerViewExampleKind.Group:
                    _groupList.Collapse(0);
                    break;
                case RecyclerViewExampleKind.Loop:
                    _loopList.ScrollToIndex(0, true);
                    break;
                case RecyclerViewExampleKind.Page:
                    _pageList.ScrollToIndex(Mathf.Max(_pageList.ScrollDataIndex - 1, 0), true);
                    break;
                case RecyclerViewExampleKind.Circle:
                    _circleList.ScrollToIndex(3, true);
                    break;
            }

            RefreshStatus();
        }

        private void OnActionC()
        {
            switch (_kind)
            {
                case RecyclerViewExampleKind.Linear:
                    _linearList.ScrollToIndex(0, true);
                    break;
                case RecyclerViewExampleKind.Grid:
                    _gridList.ScrollToIndex(_gridList.DataCount / 2, true);
                    break;
                case RecyclerViewExampleKind.Chat:
                    if (_chatList.DataCount > 0)
                    {
                        RvChatData first = _chatList.GetItem(0);
                        first.Text += "\nresized";
                        first.DeclaredLength += 36f;
                        _chatList.RefreshItem(0, true);
                    }
                    break;
                case RecyclerViewExampleKind.Mixed:
                    _mixedList.RefreshItem(1, true);
                    break;
                case RecyclerViewExampleKind.Group:
                    _groupList.Add(new RvGroupData { TemplateId = 1, Type = 1, Text = "调查附近的地脉异常 " + ++_seq });
                    break;
                case RecyclerViewExampleKind.Loop:
                    _loopList.ScrollToCenter(0, 0f, true);
                    break;
                case RecyclerViewExampleKind.Page:
                    _pageList.ScrollToIndex(0, false);
                    break;
                case RecyclerViewExampleKind.Circle:
                    _circleList.StopScroll();
                    break;
            }

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            int count = _kind switch
            {
                RecyclerViewExampleKind.Chat => _chatList != null ? _chatList.DataCount : 0,
                RecyclerViewExampleKind.Mixed => _mixedList != null ? _mixedList.DataCount : 0,
                RecyclerViewExampleKind.Group => _groupList != null ? _groupList.DataCount : 0,
                RecyclerViewExampleKind.Loop => _loopList != null ? _loopList.DataCount : 0,
                RecyclerViewExampleKind.Linear => _linearList != null ? _linearList.DataCount : 0,
                RecyclerViewExampleKind.Grid => _gridList != null ? _gridList.DataCount : 0,
                RecyclerViewExampleKind.Page => _pageList != null ? _pageList.DataCount : 0,
                RecyclerViewExampleKind.Circle => _circleList != null ? _circleList.DataCount : 0,
                _ => 0
            };
            SetStatus(_kind + "  count=" + count);
        }

        private List<RvTextData> CreateTextItems(int count)
        {
            List<RvTextData> items = new List<RvTextData>(count);
            for (int i = 0; i < count; i++)
            {
                items.Add(NextTextItem("Item"));
            }

            return items;
        }

        private List<RvChatData> CreateChatItems(int count)
        {
            List<RvChatData> items = new List<RvChatData>(count);
            for (int i = 0; i < count; i++)
            {
                items.Add(NextChatItem("Msg"));
            }

            return items;
        }

        private List<RvMixedData> CreateMixedItems(int count)
        {
            List<RvMixedData> items = new List<RvMixedData>(count);
            for (int i = 0; i < count; i++)
            {
                int templateId = i % 3;
                items.Add(new RvMixedData
                {
                    TemplateId = templateId,
                    Text = templateId switch
                    {
                        1 => "Medium " + i,
                        2 => "Tall " + i,
                        _ => "Short " + i
                    }
                });
            }

            return items;
        }

        private List<RvGroupData> CreateGroupItems()
        {
            return new List<RvGroupData>
            {
                new() { TemplateId = 1, Type = 0, Text = "寻找刻晴" },
                new() { TemplateId = 1, Type = 0, Text = "拜访师父" },
                new() { TemplateId = 1, Type = 1, Text = "为甘雨采摘10朵清心" },
                new() { TemplateId = 1, Type = 1, Text = "收集3份慕风蘑菇" },
                new() { TemplateId = 1, Type = 1, Text = "击败丘丘人射手" },
                new() { TemplateId = 1, Type = 2, Text = "参演芙宁娜的舞台剧" },
                new() { TemplateId = 1, Type = 2, Text = "调查旧日的神骸" },
                new() { TemplateId = 1, Type = 2, Text = "寻找散失的乐谱" }
            };
        }

        private RvTextData NextTextItem(string prefix)
        {
            int id = ++_seq;
            return new RvTextData
            {
                Title = prefix + " " + id,
                Color = Palette[id % Palette.Length]
            };
        }

        private RvChatData NextChatItem(string prefix)
        {
            int id = ++_seq;
            StringBuilder builder = new StringBuilder(prefix).Append(' ').Append(id);
            int lines = 1 + id % 3;
            for (int i = 1; i < lines; i++)
            {
                builder.Append('\n').Append("line ").Append(i);
            }

            return new RvChatData
            {
                Text = builder.ToString(),
                IsSelf = id % 2 == 0,
                DeclaredLength = 56f + lines * 24f
            };
        }

        private void SetActions(string actionA, string actionB, string actionC)
        {
            SetButtonText(baseui.BtnActionA, actionA);
            SetButtonText(baseui.BtnActionB, actionB);
            SetButtonText(baseui.BtnActionC, actionC);
        }

        private static void SetButtonText(UXButton button, string text)
        {
            if (button == null)
            {
                return;
            }

            UXTextMeshPro label = button.GetComponentInChildren<UXTextMeshPro>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        private void SetTitle(string title)
        {
            if (baseui.TextTitle != null)
            {
                baseui.TextTitle.text = title;
            }
        }

        private void SetStatus(string status)
        {
            if (baseui.TextStatus != null)
            {
                baseui.TextStatus.text = status;
            }
        }
    }
}
