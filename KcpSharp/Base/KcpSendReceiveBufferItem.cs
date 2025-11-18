namespace NahidaImpact.KcpSharp.Base;

internal struct KcpSendReceiveBufferItem
{
    public KcpBuffer Data;
    public KcpPacketHeader Segment;
    public KcpSendSegmentStats Stats;
}