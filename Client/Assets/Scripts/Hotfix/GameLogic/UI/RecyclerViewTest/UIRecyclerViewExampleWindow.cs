using System.Collections.Generic;
using System.Text;
using AlicizaX.UI;
using AlicizaX.UI.Runtime;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Strings = LocalizationKey.UI;

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

            RefreshStatus();
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
            _linearList.SetData(CreateTextItems(40, Strings.RECYCLER_DATA_RECORD));
            SetPresentation(
                Strings.RECYCLER_LINEAR_TITLE, Strings.RECYCLER_LINEAR_DESCRIPTION,
                Strings.RECYCLER_ACTION_APPEND, Strings.RECYCLER_ACTION_PREPEND, Strings.RECYCLER_ACTION_TOP);
        }

        private void ShowGrid()
        {
            SetListActive(baseui.ScrollViewGrid, true);
            _gridList.SetData(CreateTextItems(80, Strings.RECYCLER_DATA_SLOT));
            SetPresentation(
                Strings.RECYCLER_GRID_TITLE, Strings.RECYCLER_GRID_DESCRIPTION,
                Strings.RECYCLER_ACTION_APPENDFOUR, Strings.RECYCLER_ACTION_REMOVE, Strings.RECYCLER_ACTION_MIDDLE);
        }

        private void ShowChat()
        {
            SetListActive(baseui.ScrollViewChat, true);
            _chatList.SetData(CreateChatItems(20));
            _chatList.StickToEnd();
            SetPresentation(
                Strings.RECYCLER_CHAT_TITLE, Strings.RECYCLER_CHAT_DESCRIPTION,
                Strings.RECYCLER_ACTION_SEND, Strings.RECYCLER_ACTION_HISTORY, Strings.RECYCLER_ACTION_RESIZE);
        }

        private void ShowMixed()
        {
            SetListActive(baseui.ScrollViewMixed, true);
            _mixedList.SetData(CreateMixedItems(30));
            SetPresentation(
                Strings.RECYCLER_MIXED_TITLE, Strings.RECYCLER_MIXED_DESCRIPTION,
                Strings.RECYCLER_ACTION_SHORT, Strings.RECYCLER_ACTION_TALL, Strings.RECYCLER_ACTION_REFRESH);
        }

        private void ShowGroup()
        {
            SetListActive(baseui.ScrollViewGroup, true);
            _groupList.SetData(CreateGroupItems());
            _groupList.Expand(0);
            SetPresentation(
                Strings.RECYCLER_GROUP_TITLE, Strings.RECYCLER_GROUP_DESCRIPTION,
                Strings.RECYCLER_ACTION_EXPAND, Strings.RECYCLER_ACTION_COLLAPSE, Strings.RECYCLER_ACTION_ADDTASK);
        }

        private void ShowLoop()
        {
            SetListActive(baseui.ScrollViewLoop, true);
            _loopList.SetData(CreateTextItems(12, Strings.RECYCLER_DATA_RECORD));
            SetPresentation(
                Strings.RECYCLER_LOOP_TITLE, Strings.RECYCLER_LOOP_DESCRIPTION,
                Strings.RECYCLER_ACTION_APPEND, Strings.RECYCLER_ACTION_FIRST, Strings.RECYCLER_ACTION_CENTER);
        }

        private void ShowPage()
        {
            SetListActive(baseui.ScrollViewPage, true);
            _pageList.SetData(CreateTextItems(8, Strings.RECYCLER_DATA_COVER));
            _pageList.ScrollToIndex(0, false);
            SetPresentation(
                Strings.RECYCLER_PAGE_TITLE, Strings.RECYCLER_PAGE_DESCRIPTION,
                Strings.RECYCLER_ACTION_NEXT, Strings.RECYCLER_ACTION_PREVIOUS, Strings.RECYCLER_ACTION_FIRSTPAGE);
        }

        private void ShowCircle()
        {
            SetListActive(baseui.ScrollViewCircle, true);
            _circleList.SetData(CreateTextItems(10, string.Empty));
            SetPresentation(
                Strings.RECYCLER_CIRCLE_TITLE, Strings.RECYCLER_CIRCLE_DESCRIPTION,
                Strings.RECYCLER_ACTION_APPEND, Strings.RECYCLER_ACTION_FOURTH, Strings.RECYCLER_ACTION_STOP);
        }

        private void OnActionA()
        {
            switch (_kind)
            {
                case RecyclerViewExampleKind.Linear:
                    _linearList.Add(NextTextItem(Strings.RECYCLER_DATA_RECORD));
                    break;
                case RecyclerViewExampleKind.Grid:
                    _gridList.AddRange(CreateTextItems(4, Strings.RECYCLER_DATA_SLOT));
                    break;
                case RecyclerViewExampleKind.Chat:
                    bool pin = _chatList.IsAtEnd;
                    _chatList.Add(NextChatItem());
                    if (pin)
                    {
                        _chatList.StickToEnd();
                    }
                    break;
                case RecyclerViewExampleKind.Mixed:
                    _mixedList.Add(new RvMixedData { TemplateId = 0, Text = Strings.RECYCLER_DATA_SHORT + "  " + ++_seq });
                    break;
                case RecyclerViewExampleKind.Group:
                    _groupList.Expand(0);
                    break;
                case RecyclerViewExampleKind.Loop:
                    _loopList.Add(NextTextItem(Strings.RECYCLER_DATA_RECORD));
                    break;
                case RecyclerViewExampleKind.Page:
                    _pageList.ScrollToIndex(Mathf.Min(_pageList.ScrollDataIndex + 1, _pageList.DataCount - 1), true);
                    break;
                case RecyclerViewExampleKind.Circle:
                    _circleList.Add(NextTextItem(string.Empty));
                    break;
            }

            RefreshStatus();
        }

        private void OnActionB()
        {
            switch (_kind)
            {
                case RecyclerViewExampleKind.Linear:
                    _linearList.Insert(0, NextTextItem(Strings.RECYCLER_DATA_RECORD));
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
                        NextChatItem(),
                        NextChatItem()
                    });
                    break;
                case RecyclerViewExampleKind.Mixed:
                    _mixedList.Add(new RvMixedData { TemplateId = 2, Text = Strings.RECYCLER_DATA_TALL + "  " + ++_seq });
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
                        first.Text += "\n" + Strings.RECYCLER_DATA_UPDATE;
                        first.DeclaredLength += 36f;
                        _chatList.RefreshItem(0, true);
                    }
                    break;
                case RecyclerViewExampleKind.Mixed:
                    _mixedList.GetItem(0).Text = Strings.RECYCLER_DATA_SHORT + "  " + ++_seq;
                    _mixedList.RefreshItem(0, true);
                    break;
                case RecyclerViewExampleKind.Group:
                    _groupList.Add(new RvGroupData { TemplateId = 1, Type = 1, Text = Strings.RECYCLER_DATA_NEWTASK + "  " + ++_seq });
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
                RecyclerViewExampleKind.Chat => _chatList.DataCount,
                RecyclerViewExampleKind.Mixed => _mixedList.DataCount,
                RecyclerViewExampleKind.Group => _groupList.DataCount,
                RecyclerViewExampleKind.Loop => _loopList.DataCount,
                RecyclerViewExampleKind.Linear => _linearList.DataCount,
                RecyclerViewExampleKind.Grid => _gridList.DataCount,
                RecyclerViewExampleKind.Page => _pageList.DataCount,
                RecyclerViewExampleKind.Circle => _circleList.DataCount,
                _ => 0
            };
            baseui.TextStatus.SetText(Strings.RECYCLER_COUNT(count));
        }

        private List<RvTextData> CreateTextItems(int count, string label)
        {
            List<RvTextData> items = new List<RvTextData>(count);
            for (int i = 0; i < count; i++)
            {
                items.Add(NextTextItem(label));
            }

            return items;
        }

        private List<RvChatData> CreateChatItems(int count)
        {
            List<RvChatData> items = new List<RvChatData>(count);
            for (int i = 0; i < count; i++)
            {
                items.Add(NextChatItem());
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
                    Text = (templateId switch
                    {
                        1 => Strings.RECYCLER_DATA_MEDIUM,
                        2 => Strings.RECYCLER_DATA_TALL,
                        _ => Strings.RECYCLER_DATA_SHORT
                    }) + "  " + ++_seq
                });
            }

            return items;
        }

        private List<RvGroupData> CreateGroupItems()
        {
            return new List<RvGroupData>
            {
                new() { TemplateId = 1, Type = 0, Text = Strings.RECYCLER_DATA_TASK0 },
                new() { TemplateId = 1, Type = 0, Text = Strings.RECYCLER_DATA_TASK1 },
                new() { TemplateId = 1, Type = 1, Text = Strings.RECYCLER_DATA_TASK2 },
                new() { TemplateId = 1, Type = 1, Text = Strings.RECYCLER_DATA_TASK3 },
                new() { TemplateId = 1, Type = 1, Text = Strings.RECYCLER_DATA_TASK4 },
                new() { TemplateId = 1, Type = 2, Text = Strings.RECYCLER_DATA_TASK5 },
                new() { TemplateId = 1, Type = 2, Text = Strings.RECYCLER_DATA_TASK6 },
                new() { TemplateId = 1, Type = 2, Text = Strings.RECYCLER_DATA_TASK7 }
            };
        }

        private RvTextData NextTextItem(string prefix)
        {
            int id = ++_seq;
            return new RvTextData
            {
                Title = string.IsNullOrEmpty(prefix) ? id.ToString("00") : $"{prefix}  {id:00}"
            };
        }

        private RvChatData NextChatItem()
        {
            int id = ++_seq;
            StringBuilder builder = new StringBuilder(Strings.RECYCLER_DATA_MESSAGE).Append(' ').Append(id);
            int lines = 1 + id % 3;
            for (int i = 1; i < lines; i++)
            {
                builder.Append('\n').Append(i == 1 ? Strings.RECYCLER_DATA_NOTE : Strings.RECYCLER_DATA_UPDATE);
            }

            return new RvChatData
            {
                Text = builder.ToString(),
                IsSelf = id % 2 == 0,
                DeclaredLength = 56f + lines * 24f
            };
        }

        private void SetPresentation(string title, string description, string actionA, string actionB, string actionC)
        {
            baseui.TextTitle.SetText(title);
            baseui.TextDescription.SetText(description);
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

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = text;
            }
        }
    }
}
