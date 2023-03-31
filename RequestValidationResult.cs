using System;

namespace CallBackHandler
{
    public class RequestValidationResult
    {
        public bool IsValid { get; }

        public RequestValidationResult(bool isValid)
        {
            this.IsValid = isValid;
        }
    }
}