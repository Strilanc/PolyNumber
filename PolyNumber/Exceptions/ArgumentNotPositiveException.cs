using System;

namespace Strilanc.Exceptions {
    [Serializable]
    public class ArgumentNotPositiveException : ArgumentOutOfRangeException {
        public ArgumentNotPositiveException(string parameterName) : base(parameterName, "Argument is not positive.") {
        }
        public ArgumentNotPositiveException(string parameterName, object actualValue) : base(parameterName, actualValue, "Argument is not positive.") {
        }
    }
}
