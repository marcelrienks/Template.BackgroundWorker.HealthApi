using System;

namespace HealthApi.Models
{
    public class ApplicationCancelledException : Exception
    {
        public ApplicationCancelledException(string message) : base(message)
        {

        }
    }
}
