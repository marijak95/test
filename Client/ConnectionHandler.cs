using Common;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
namespace Client
{
    public class ConnectionHandler
    {
        public ChannelFactory<IWorkgroupService> workgroupChannel;
        public ChannelFactory<IRegistrationService> registrationChannel;
        //string ip;
        public ConnectionHandler()
        {
            //Console.WriteLine("Input server IP...");
            //this.ip = Console.ReadLine();
            OpenWorkgroupChannel();
            OpenRegistrationChannel();
        }

        public void OpenWorkgroupChannel()
        {
            EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("wcfservice");
            NetTcpBinding workgroupBinding = new NetTcpBinding();
            workgroupBinding.Security.Mode = SecurityMode.Message;
            workgroupBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:8080/WorkgroupService"), identity);
            //EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + ip + ":8080/WorkgroupService"), identity);

            workgroupChannel = new ChannelFactory<IWorkgroupService>(workgroupBinding, address);
            workgroupChannel.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            workgroupChannel.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
        }

        public void OpenRegistrationChannel()
        {
            EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("wcfservice");

            NetTcpBinding registrationChannelBinding = new NetTcpBinding();
            registrationChannelBinding.Security.Mode = SecurityMode.Message;
            registrationChannelBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            EndpointAddress regAddress = new EndpointAddress(new Uri("net.tcp://localhost:8080/RegistrationService"), identity);
            //EndpointAddress regAddress = new EndpointAddress(new Uri("net.tcp://" + ip + ":8080/RegistrationService"), identity);

            registrationChannel = new ChannelFactory<IRegistrationService>(registrationChannelBinding, regAddress);
            registrationChannel.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            registrationChannel.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
        }
        
        public bool CloseRegistrationChannel()
        {
            try
            {
                registrationChannel.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                registrationChannel.Abort();
                return false;
            }

            registrationChannel = null;
            return true;
        }

        public bool CloseWorkgroupChannel()
        {
            try
            {
                workgroupChannel.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                workgroupChannel.Abort();
                return false;
            }

            workgroupChannel = null;
            return true;
        }
    }
}
