using Domain.Contracts;

namespace Client.Services
{
    public class ClientService : IClientContract
    {
        public string TestClientMethod()
        {
            return "TestClient";
        }
    }
}
