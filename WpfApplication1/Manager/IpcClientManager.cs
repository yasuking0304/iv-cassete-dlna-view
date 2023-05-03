using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using View.EventArgs;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace View.Manager
{
    class IpcClientManager
    {
        public IpcRemoteObject remoteObject;

        public IpcClientManager()
        {
            // クライアントチャンネル生成
            IpcClientChannel channel = new IpcClientChannel();

            // チャンネル登録
            ChannelServices.RegisterChannel(channel, true);

            // リモートオブジェクト取得
            remoteObject = Activator.GetObject(typeof(IpcRemoteObject),
                "ipc://ivdr/ipc") as IpcRemoteObject;
        }
    }

}
