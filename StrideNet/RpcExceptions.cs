using System;
using System.Linq;

namespace StrideNet
{
    public static class RpcExceptions
    {
        public static void ThrowCalledServerAuthorative()
        {
            throw new InvalidOperationException("The RPC cannot be called from client, because it is sever authoritive.");
        }

        public static void ThrowCalledForeignEntity()
        {
            throw new InvalidOperationException("The RPC cannot be called from client that doesn't own entity.");
        }

        public static void ThrowSettingVaribleFromClient ()
        {
            throw new InvalidOperationException("Setting varible from client is not allowed.");
        }
    }
}
