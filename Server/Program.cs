using Common;
using System;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Message;

            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

            //Console.WriteLine("Input server IP...");
            //string ip = Console.ReadLine();
            string address = "net.tcp://localhost:8080/WorkgroupService";
            //string address = "net.tcp://" + ip + ":8080/WorkgroupService";
            ServiceHost workgroupService = new ServiceHost(typeof(WorkgroupService));
            
            workgroupService.AddServiceEndpoint(typeof(IWorkgroupService), binding, address);
            
            workgroupService.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, "wcfservice");
            workgroupService.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
            workgroupService.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUserNamePasswordValidator();

            workgroupService.Authorization.PrincipalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.Custom;
            workgroupService.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();

            ServiceHost registrationService = new ServiceHost(typeof(RegistrationService));

            string regAddress = "net.tcp://localhost:8080/RegistrationService";
            //string regAddress = "net.tcp://" + ip + ":8080/RegistrationService";
            NetTcpBinding binding2 = new NetTcpBinding();
            binding2.Security.Mode = SecurityMode.Message;

            binding2.Security.Message.ClientCredentialType = MessageCredentialType.None;

            registrationService.AddServiceEndpoint(typeof(IRegistrationService), binding2, regAddress);

            registrationService.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, "wcfservice");
            registrationService.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;

            try
            {
                workgroupService.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            try
            {
                registrationService.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("Services ready and listening...");
            Console.ReadLine();

            workgroupService.Close();
            registrationService.Close();
        }
    }
}
