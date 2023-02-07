using System;
using BinSvip.Standalone.Model;

namespace BinSvip.Standalone.Nrbf
{

    public class NrbfStream : IDisposable
    {
        private bool _disposed;

        public NrbfStream()
        {
            // Load library
            NrbfLibrary.Load();

            _impl = new NrbfStreamImpl();
        }

        ~NrbfStream()
        {
            Dispose();
        }

        /// <summary>
        /// 在离开实例的作用域后，应当立即调用此方法释放内存
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Unload library
                NrbfLibrary.Unload();

                _disposed = true;
            }
        }

        /// <summary>
        /// 将MS-NRBF数据反序列化为XStudio工程类
        /// </summary>
        /// <param name="data">不包含版本号前缀的SVIP二进制数据</param>
        /// <returns>XStudio工程类</returns>
        public AppModel Read(byte[] data)
        {
            return _impl.Read(data);
        }

        /// <summary>
        /// 将XStudio工程类序列化为MS-NRBF数据（需要注意某些参数不能为Null）
        /// </summary>
        /// <param name="appModel">XStudio工程类</param>
        /// <returns>不包含版本号前缀的SVIP二进制数据</returns>
        public byte[] Write(AppModel appModel)
        {
            return _impl.Write(appModel);
        }

        /// <summary>
        /// 重置实例状态
        /// </summary>
        public void Reset()
        {
            _impl.ErrorMessage = "";
            _impl.Status = StatusType.Ok;
        }

        public enum StatusType
        {
            Ok,
            ReadPastEnd,
            ReadCorruptData,
            WriteFailed,
        }

        /// <summary>
        /// 上一次序列化或反序列化操作的错误信息
        /// </summary>
        public string ErrorMessage => _impl.ErrorMessage;

        /// <summary>
        /// 进行一次序列化或反序列化操作操作后，本实例的状态，如果为非正常则会提供错误信息
        /// </summary>
        public StatusType Status => _impl.Status;

        private NrbfStreamImpl _impl;
    }

}