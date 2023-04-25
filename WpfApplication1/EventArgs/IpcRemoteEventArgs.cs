using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace View.EventArgs {

    public class IpcRemoteObject : MarshalByRefObject {
        public class IpcRemoteObjectEventArg {
            public string command { get; set; }
            public IpcRemoteObjectEventArg(string cmd) {
                command = cmd;
            }
        }

        public delegate void CallEventHandler(IpcRemoteObjectEventArg e);
        public event CallEventHandler getData;

        public void sendData(string cmd) {
            if (getData != null) {
                getData(new IpcRemoteObjectEventArg(cmd));
            }
        }
        public override object InitializeLifetimeService() {
            return null;
        }
    }
}
