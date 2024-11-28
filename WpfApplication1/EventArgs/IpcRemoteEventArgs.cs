using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace View.EventArgs {

    public class IpcRemoteObject : MarshalByRefObject {
        public class IpcRemoteObjectEventArg {
            public string Command { get; set; }
            public IpcRemoteObjectEventArg(string cmd) {
                Command = cmd;
            }
        }

        public delegate void CallEventHandler(IpcRemoteObjectEventArg e);
        public event CallEventHandler ReceivedData;

        public void SendData(string cmd) {
            ReceivedData?.Invoke(new IpcRemoteObjectEventArg(cmd));
        }
        public override object InitializeLifetimeService() {
            return null;
        }
    }
}
