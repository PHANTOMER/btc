using System.Collections.Generic;
using System.Linq;

namespace Btc.Api.Infrastructure
{
    public class ServiceResponse<T>
    {
        public ServiceResponse()
        {
        }

        protected ServiceResponse(bool isSuccess, T data)
        {
            this.IsSuccess = isSuccess;
            this.Data = data;
        }

        public bool IsSuccess { get; set; }
        

        public T Data { get; set; }

        public bool HasMessages => Messages.Any();

        public List<string> Messages { get; set; } = new List<string>();

        public static ServiceResponse<T> New(bool isSuccess, T data = default(T))
        {
            return new ServiceResponse<T>(isSuccess, data);
        }

        public void AddMessage(string message)
        {
            this.Messages.Add(message);
        }
    }
}
