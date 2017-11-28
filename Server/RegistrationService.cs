using Common;
using System;
using System.ServiceModel;

namespace Server
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class RegistrationService : IRegistrationService
    {
        DatabaseManager dbManager = new DatabaseManager();

        public bool Register(User user)
        {
            bool success = false;
            user.RequireSafeLogin = 0;
            user.BossId = -1;
            user.Team = -1;
            user.Group = "Users";
            try
            {
                success = dbManager.AddUser(user);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw new FaultException(e.Message);
            }

            return true;
        }
    }
}
