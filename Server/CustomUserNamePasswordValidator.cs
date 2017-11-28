using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace Server
{
    public class CustomUserNamePasswordValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if(userName == null || password == null)
            {
                throw new FaultException("Credentials null!");
            }
        }
    }
}
