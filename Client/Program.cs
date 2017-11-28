using Common;
using System;
using System.ServiceModel;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            UserInterface userInterface = new UserInterface();
            ClientAccount clientAccount = new ClientAccount();
            ConnectionHandler connectionHandler = new ConnectionHandler();
            int userCommand;

            IWorkgroupService workgroupProxy = null; //Null as we're creating it upon login...
            IRegistrationService registrationProxy = connectionHandler.registrationChannel.CreateChannel();

            do
            {
                userCommand = userInterface.AnonymousUserInterface(ref workgroupProxy, registrationProxy, connectionHandler, clientAccount);
                if(userCommand == 0)
                {
                    connectionHandler.CloseRegistrationChannel();
                    connectionHandler.CloseWorkgroupChannel();
                    return;
                }
            } while (!clientAccount.Authenticated);

            Console.WriteLine("Succesffully authenticated!");

            connectionHandler.CloseRegistrationChannel(); //Registration channel is no longer needed, close it.
            registrationProxy = null;

            if (clientAccount.ClientGroup == "Administrator") //Napravite enumeraciju za Grupe u Common biblioteci...
            {
                userInterface.AdminWindow(clientAccount, workgroupProxy);
            }
            else //User ? Boss? Director? 
            {
                userInterface.WorkerWindow(clientAccount, workgroupProxy, connectionHandler);
            }

            Console.ReadLine();
        }
    }
}
