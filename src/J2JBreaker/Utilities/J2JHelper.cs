using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Utilities
{
    internal class J2JHelper : IDisposable
    {
        #region ::Variables::

        private FileStream? _fileStream = null;

        internal FileStream? FileStream
        {
            get => _fileStream;
            set => _fileStream = value;
        }

        #endregion

        #region ::Constructors::

        internal J2JHelper(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        #endregion

        #region ::Methods::

        internal byte[] GetHeader()
        {
            if (_fileStream != null)
            {
                byte[] buffer = new byte[32];
                _fileStream.Position = (_fileStream.Length - 32);
                _fileStream.Read(buffer, 0, 32);

                return buffer;
            }
            else
            {
                throw new NullReferenceException("The file stream is null.");
            }
        }

        internal bool IsValid()
        {
            if (_fileStream != null)
            {
                byte[] buffer = new byte[8];
                _fileStream.Position = (_fileStream.Length - 8);
                _fileStream.Read(buffer, 0, 8);

                string headerString = Encoding.UTF8.GetString(buffer, 0, 8);

                if (headerString == "L3000009")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new NullReferenceException("The file stream is null.");
            }
        }

        #endregion

        #region ::IDisposable Members::

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileStream?.Dispose();
                }

                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~Breaker()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
