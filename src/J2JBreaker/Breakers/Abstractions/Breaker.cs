using J2JBreaker.Breakers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Breakers.Abstractions
{
    internal abstract class Breaker : IDisposable
    {
        #region ::Variables::

        protected FileStream? _fileStream = null;

        internal FileStream? FileStream
        {
            get => _fileStream;
            set => _fileStream = value;
        }

        protected BreakingMode _mode = BreakingMode.BruteForce;

        internal BreakingMode Mode
        {
            get => _mode;
            set => _mode = value;
        }     

        protected List<char> _characterSet = new List<char>();

        internal List<char> CharacterSet
        {
            get => _characterSet;
            set => _characterSet = value;
        }

        protected string _rainbowTablePath = string.Empty;

        internal string RainbowTablePath
        {
            get => _rainbowTablePath;
            set => _rainbowTablePath = value;
        }

        protected uint _length = 10;

        internal uint Length
        {
            get => _length;
            set => _length = value;
        }

        protected bool _useEnhancer = false;

        internal bool UseEnhancer
        {
            get => _useEnhancer;
            set => _useEnhancer = value;
        }

        protected bool _noExcept = false;

        internal bool NoExcept
        {
            get => _noExcept;
            set => _noExcept = value;
        }

        #endregion

        #region ::Constructors::

        internal Breaker(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        #endregion

        #region ::Methods::

        protected List<string> ExceptAbnormalStrings(List<string> origins, List<char> characterSet)
        {
            List<string> result = new List<string>();

            foreach (string origin in origins)
            {
                char[] orginChars = origin.ToCharArray();

                bool errorFlag = false;

                foreach (char originChar in orginChars)
                {
                    if (!characterSet.Contains(originChar))
                    {
                        errorFlag = true;
                    }
                }

                if (!errorFlag)
                {
                    result.Add(origin);
                }
            }

            return result;
        }

        protected List<string> ExceptLongStrings(List<string> origins)
        {
            List<string> result = new List<string>();

            foreach (string origin in origins)
            {
                if (origin.Length <= _length)
                {
                    result.Add(origin);
                }
            }

            return result;
        }

        internal abstract List<string> Break(out bool result);

        #endregion

        #region ::IDisposable Members::

        private bool disposedValue;

        protected virtual void DisposeManagedComponents()
        {
            _fileStream?.Dispose();
        }

        protected virtual void DisposeUnmanagedComponents()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeManagedComponents();
                }

                DisposeUnmanagedComponents();
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
