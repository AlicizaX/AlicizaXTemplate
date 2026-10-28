using AlicizaX;
using AlicizaX.UI.Runtime;
using Game.UI;

namespace GameLogic.UI
{
    public  sealed class UITestAWidget:UIWidget<ui_UITestAWidget>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            Log.Info("UITestAWidget.OnInitialize");
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Log.Info("UITestAWidget.OnOpen");
        }

        protected override void OnClose()
        {
            base.OnClose();
            Log.Info("UITestAWidget.OnClose");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Info("UITestAWidget.OnDestroy");
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            Log.Info("UITestAWidget.OnRefresh");
        }

        protected override void OnRegisterEvent(EventListenerProxy proxy)
        {
            base.OnRegisterEvent(proxy);
            Log.Info("UITestAWidget.OnRegisterEvent");
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            Log.Info("UITestAWidget.OnUpdate()");
        }
    }
}
