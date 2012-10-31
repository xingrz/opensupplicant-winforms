using System;

namespace OpenSupplicant
{
    public class AmtiumException : ApplicationException
    {
        public AmtiumException()
            : base()
        {
        }

        public AmtiumException(string message)
            : base(message)
        {
        }

        public AmtiumException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
