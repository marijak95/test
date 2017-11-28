using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IRegistrationService
    {
        [OperationContract]
        bool Register(User user);
    }
}
