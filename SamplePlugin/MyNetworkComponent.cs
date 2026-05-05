using RoR2;
using UnityEngine.Networking;

namespace RiskOfRoutes
{
    internal class MyNetworkComponent : NetworkBehaviour
    {
        private static MyNetworkComponent _instance;

        private void Awake()
        {
            _instance = this;

            Log.Info("T MyNetworkComponesnt spawned on: " + (NetworkServer.active ? "Server" : "Client"));
        }
        public static void Invoke(NetworkUser user, string msg)
        {
            _instance.TargetLog(user.connectionToClient, msg);
        }

        [TargetRpc]
        private void TargetLog(NetworkConnection target, string msg)
        {
            Log.Info(msg);
        }
    }
}
