using System;

namespace Strilanc.Exceptions {
    [Serializable]
    public class ArgumentNegativeException : ArgumentOutOfRangeException {
        public ArgumentNegativeException(string parameterName) : base(parameterName, "Argument is negative.") {
        }
        public ArgumentNegativeException(string parameterName, object actualValue) : base(parameterName, actualValue, "Argument is negative.") {
        }
    }
}
