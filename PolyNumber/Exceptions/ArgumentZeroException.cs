using System;

namespace Strilanc.Exceptions {
    [Serializable]
    public class ArgumentZeroException : ArgumentOutOfRangeException {
        public ArgumentZeroException(string parameterName) : base(parameterName, "Argument is zero.") {
        }
    }
}
