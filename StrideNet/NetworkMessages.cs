namespace StrideNet
{
    internal enum ClientNetworkMessages : ushort
    {
        Rpc = 0,
        SpawnEntity,
        DespawnEntity
    }

    internal enum ServerNetworkMessages : ushort
    {
        Rpc = 0
    }
}
