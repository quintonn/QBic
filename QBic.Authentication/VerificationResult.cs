using Microsoft.AspNetCore.Http;

namespace QBic.Authentication
{
    public class VerificationResult
    {
        public bool Valid { get; set; }

        public string ErrorMessage { get; set; }

        public int StatusCode { get; set; }

        public object ResultObject { get; set; }

        private VerificationResult(bool valid, object resultObject = null, string errorMessage = null, int statusCode = StatusCodes.Status401Unauthorized)
        {
            Valid = valid;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
            ResultObject = resultObject;
        }

        public static VerificationResult Success(object resultObject = null)
        {
            return new VerificationResult(true, resultObject);
        }

        public static VerificationResult Error(string errorMessage, int statusCode = StatusCodes.Status401Unauthorized)
        {
            return new VerificationResult(false, null, errorMessage, statusCode);
        }
    }
}
