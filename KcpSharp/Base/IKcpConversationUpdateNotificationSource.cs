namespace NahidaImpact.KcpSharp.Base;

internal interface IKcpConversationUpdateNotificationSource
{
    ReadOnlyMemory<byte> Packet { get; }
    void Release();
}