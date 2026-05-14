using Luban;

namespace Game.Config.Tables
{
    public interface ILocalizationTextEntry
    {
        string Key { get; }

        string Text { get; }
    }

    public sealed class LocalizationConstConfig : BeanBase
        , ILocalizationTextEntry
    {
        public LocalizationConstConfig(ByteBuf buf)
        {
            Id = buf.ReadInt();
            Key = buf.ReadString();
            Text = buf.ReadString();
        }

        public readonly int Id;
        public string Key { get; }
        public string Text { get; }

        public const int __ID__ = 209130001;
        public override int GetTypeId() => __ID__;

        public void ResolveRef(ConfigService tables)
        {
        }

        public override string ToString()
        {
            return "{ "
                + "id:" + Id + ","
                + "key:" + Key + ","
                + "text:" + Text + ","
                + "}";
        }
    }
}
