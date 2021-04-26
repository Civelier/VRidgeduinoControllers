using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRE.Vridge.API.Client.Remotes;
using VRidgeduinoControllers.MathUtilities;

namespace VRidgeduinoControllers.Remotes
{
    public class SafeHeadRemote
    {
        private readonly VridgeRemote _remote;

        public Vector3 Position { get; set; }

        public SafeHeadRemote(VridgeRemote remote)
        {
            _remote = remote;
        }

        bool TryGetHead(out HeadRemote head)
        {
            try
            {
                if (_remote != null && _remote.Head != null)
                {
                    head = _remote.Head;
                    return true;
                }
            }
            catch (ObjectDisposedException) { }
            head = null;
            return false;
        }

        public bool TryUpdateHead()
        {
            try
            {
                if (TryGetHead(out HeadRemote head))
                {
                    head.SetPosition(Position.X, Position.Y, Position.Z);
                    return true;
                }
            }
            catch (ObjectDisposedException)
            {

            }
            return false;
        }
    }
}
