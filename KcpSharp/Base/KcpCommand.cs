namespace NahidaImpact.KcpSharp.Base;

internal enum KcpCommand : byte
{
    Push = 81,
    Ack = 82,
    WindowProbe = 83,
    WindowSize = 84
}