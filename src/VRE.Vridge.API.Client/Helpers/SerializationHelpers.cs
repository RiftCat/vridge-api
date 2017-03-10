using System;
using System.Runtime.InteropServices;
using NetMQ;
using Newtonsoft.Json;

namespace VRE.Vridge.API.Client.Helpers
{
    public static class SerializationHelpers
    {
        /// <summary>
        /// Sends object as JSON-serialized data through given socket.
        /// </summary>        
        public static void SendAsJson(this IOutgoingSocket socket, object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            socket.SendFrame(json);
        }

        /// <summary>
        /// Tries to send object as JSON-serialized data through given socket.
        /// Returns false if operation times out.
        /// </summary>
        public static bool TrySendAsJson(this IOutgoingSocket socket, object obj, int timeoutMs)
        {
            var json = JsonConvert.SerializeObject(obj);
            return socket.TrySendFrame(TimeSpan.FromMilliseconds(timeoutMs), json);
        }

        /// <summary>
        /// Awaits T object as given socket received as JSON.
        /// </summary>
        public static T ReceiveJson<T>(this IReceivingSocket socket)
        {
            var json = socket.ReceiveFrameString();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Awaits T object as given socket received as JSON.
        /// Returns default(T) if operation times out.
        /// </summary>
        public static bool TryReceiveJson<T>(this IReceivingSocket socket, out T response, int timeoutMs)
        {
            string responseStr;
            var success = socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(timeoutMs), out responseStr);

            if (string.IsNullOrEmpty(responseStr))
            {
                response = default(T);
                return false;
            }

            response = JsonConvert.DeserializeObject<T>(responseStr);
            return success;
        }

        /// <summary>
        /// Converts given structure to byte array.
        /// </summary>        
        public static byte[] StructureToByteArray(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        /// <summary>
        /// Converts given byte array to structure T.
        /// </summary>        
        public static T ByteArrayToStructure<T>(byte[] data)
        {
            GCHandle pin = GCHandle.Alloc(data, GCHandleType.Pinned);
            T packet = (T)Marshal.PtrToStructure(pin.AddrOfPinnedObject(), typeof(T));
            pin.Free();

            return packet;
        }

        /// <summary>
        /// Converts structure to pointer. Caller needs to free IntPtr.
        /// </summary>        
        public static IntPtr StructureToIntPtr(object str, out int size)
        {
            size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, false);            
            return ptr;
        }

        /// <summary>
        /// Converts array to pointer. Caller needs to free IntPtr.
        /// </summary>        
        public static IntPtr ArrayToIntPtr(byte[] array)
        {            
            IntPtr ptr = Marshal.AllocHGlobal(array.Length);
            Marshal.Copy(array, 0, ptr, array.Length);
            return ptr;
        }
    }
}
