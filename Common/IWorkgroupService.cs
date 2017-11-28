using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IWorkgroupService
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        [PrincipalPermission(SecurityAction.Demand, Unrestricted = true)]
        LoginDTO Login(string username, string password, string answerOne, string answerTwo);

        [OperationContract]
        [FaultContract(typeof(string))]
        User FindUser(string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool Logout(string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool ChangePassword(string username, string oldPassword, string newPassword);

        [OperationContract]
        [FaultContract(typeof(string))]
        void RequestVacation(string username, string start, string end);

        [OperationContract]
        [FaultContract(typeof(string))]
        List<Request> AllRequests(string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        void AddToTeam(string username, int groupId);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool ApproveRequest(int id, string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool DenyRequest(int id, string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        void Ask(string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        List<User> UsersForJoin();

        [OperationContract]
        [FaultContract(typeof(string))]
        List<User> AllUsers();

        [OperationContract]
        [FaultContract(typeof(string))]
        void NameBoss(string username, int teamid);
    }
}
