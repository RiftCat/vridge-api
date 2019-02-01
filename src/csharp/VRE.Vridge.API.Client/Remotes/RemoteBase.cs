using System;
using System.Collections.Generic;
using System.Text;
using VRE.Vridge.API.Client.Proxy;

namespace VRE.Vridge.API.Client.Remotes
{
    public abstract class RemoteBase<T> where T : ClientProxyBasePB
    {
        public bool IsDisposed { get; private set; }
        protected T Proxy;        

        protected RemoteBase(T proxy)
        {
            this.Proxy = proxy;
            //EnsureSocketExists();
        }

        internal virtual void Dispose()
        {
            IsDisposed = true;
            Proxy?.Dispose();
        }

        protected T WrapTimeouts<T>(Func<T> action) 
        {
            if (!Proxy.IsSocketOpen)
            {
                Dispose();
                return default(T);
            }

            try
            {
                return action();
            }
            catch (Exception x)
            {
                Dispose();
                return default(T);
            }
        }

        protected void WrapTimeouts(Action action)
        {
            try
            {
                action();
            }
            catch (Exception x)
            {
                Dispose();
            }
        }



        /*internal void EnsureSocketExists()
        {
            // This can't fail because opening socket doesn't actually open the socket but only creates abstraction. 
            // It is created on first send.
            if(!Proxy.IsSocketOpen)
            {                
                Proxy?.Dispose();
                Proxy = (T)Activator.CreateInstance(typeof(T), endpoint, true);
            }            
        } */
    }
}
