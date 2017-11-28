using System;
using Common;
using System.ServiceModel;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;
using static System.Net.Mime.MediaTypeNames;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class WorkgroupService : IWorkgroupService
    {
        DatabaseManager dbManager = new DatabaseManager();
        AccessDB access = new AccessDB();
        bool userAuthenticated = false;

        public void AddToTeam(string username, int groupId)
        {
            //Jelaciceva mudrost:
            //Ovo je metoda za dodavanje usera u odredjeni tim za koju samo admin treba da ima pristup...(nadam se)?
            //KAKO AUTORIZOVATI USERA??? LAKO!!!
            bool allow = Thread.CurrentPrincipal.IsInRole("Administrator");//ZASTO OVO MOZEMO DA URADIMO SA THREAD CURRENT PRINCIPAL??? PITAJ ME SUTRA.
            if(allow)
            {
                //USER JESTE ADMIN...URADI STA TREBA
                dbManager.AddToTeam(username, groupId);
            }
            else
            {
                throw new FaultException("You can not add to team because you are not admin");
                //BACI FAULTEXCEPTION...
                /*
                 * U OVAJ ELSE CAK NIKADA NE BI SMELO DA SE UPADNE!! JER MI USERIMA NE NUDIMO ADMIN METODE U INTERFEJSIMA...ALI ZA SVAKI SLUCAJ!!!!
                * */
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (userAuthenticated)
            {
                User userAccount = dbManager.FindUser(username);
                if(userAccount != null && userAccount.Password == oldPassword)
                {
                    userAccount.Password = newPassword;
                    if(dbManager.UpdateUser(userAccount))
                    {
                        return true;
                    }
                    else
                    {                       
                        throw new FaultException("Operation failed! Server failed to access database");
                        
                    }
                }
                else
                {
                    throw new FaultException("Invalid access! User passed wrong credentials!");        
                }
            }
            else
            {
                throw new FaultException("Invalid access! User not authenticated.");
            }
        }

        
        
        public LoginDTO Login(string username, string password, string answerOne, string answerTwo)
        {
            User userAccount = dbManager.FindUser(username);
            LoginDTO loginInformation = new LoginDTO();
            loginInformation.Authenticated = userAuthenticated;

            if(userAccount == null)
            {
                throw new FaultException("Invalid username or password");
            }

            if (userAccount.RequireSafeLogin >= 3)
            {
                if (answerOne == null || answerTwo == null)
                {
                    throw new FaultException("Provide answers to your security questions");
                }
            }

            if (userAccount != null)
            {
                if(userAccount.Password == password)
                {
                    if (userAccount.RequireSafeLogin >= 3)
                    {
                        if (userAccount.AnswerOne == answerOne && userAccount.AnswerTwo == answerTwo)
                        {
                            userAuthenticated = true;
                            loginInformation.Authenticated = true;
                            loginInformation.UserGroup = userAccount.Group;
                            userAccount.RequireSafeLogin = 0;
                            dbManager.UpdateUser(userAccount);
                        }
                    }
                    else
                    {
                        userAuthenticated = true;
                        loginInformation.Authenticated = true;
                        loginInformation.UserGroup = userAccount.Group;
                        userAccount.RequireSafeLogin = 0;
                        dbManager.UpdateUser(userAccount);
                    }

                    return loginInformation;
                }
                else
                {
                    userAccount.RequireSafeLogin++;
                    dbManager.UpdateUser(userAccount);

                    if (userAccount.RequireSafeLogin >= 3)
                    {
                        loginInformation.SecurityQuestionOne = userAccount.QuestionOne;
                        loginInformation.SecurityQuestionTwo = userAccount.QuestionTwo;
                    }

                    loginInformation.Authenticated = userAuthenticated;

                    return loginInformation;
                }
            }
            else
            {
                throw new FaultException<string>("Authentication failed", "Invalid username or password! Try again.");
            }   
        }

        public bool Logout(string username) //Client should close connection after this method is called.
        {
            if(userAuthenticated)
            {
                userAuthenticated = false;
                return true;
            }
            else
            {
                throw new FaultException<string>("Invalid access!", "Client is not authenticated");
            }
        }

        public void RequestVacation(string username, string start, string end)
        {
            if(Thread.CurrentPrincipal.IsInRole("Users") || Thread.CurrentPrincipal.IsInRole("Boss"))
            {
                User user = dbManager.FindUser(username);
                if (user.Team != -1)
                {
                    if (user.BossId != -1)
                    {
                        dbManager.AddRequest(username, start, end);
                    }
                    else
                    {
                        throw new FaultException("You do not have a boss! You can not request vacation");
                    }

                }
                else
                {
                    throw new FaultException("You are not member of any team! You can not request vacation");
                }
            }                      
        }

        public List<Request> AllRequests(string username)
        {
            User user = dbManager.FindUser(username);
            List<Request> requests = new List<Request>();
            if(Thread.CurrentPrincipal.IsInRole("Boss"))
            {
                foreach (Request r in access.ListOfRequests)
                {                  
                    string[] bossid = r.RequestManagers.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    foreach(string s in bossid)
                    {
                        if (Int32.Parse(s) == user.Id && r.Denied!=true)
                        {
                            requests.Add(r);
                        }

                    }
                }
            } 
            else if(Thread.CurrentPrincipal.IsInRole("Administrator"))
            {
                foreach (Request r in access.ListOfRequests)
                {
                    requests.Add(r);                
                }
            }     
            return requests;
        }

        public List<User> AllUsers()
        {         
            List<User> users = new List<User>();         
                foreach (User u in access.ListOfUsers)
                {
                    if(u.Group.Equals("Users"))
                    users.Add(u);
                }   
            return users;
        }

        public bool ApproveRequest(int id, string username)
        {
                
                bool success = dbManager.ApproveRequest(id, username);
                if (success == true)
                {
                    return true;
                }
                else
                {
                    throw new FaultException("You don't have right to approve this request!");
                }     
        }

        public bool DenyRequest(int id, string username)
        {
            bool success = dbManager.DenyRequest(id, username);
            if (success == true)
            {
                return true;
            }
            else
            {
                throw new FaultException("You don't have right to deny this request!");
            }
        }

        public User FindUser(string username)
        {
            User u = dbManager.FindUser(username);
            return u;
            
        }

        public void Ask(string username)
        {
            User user = dbManager.FindUser(username);
                if (user.Team == -1 && user.AskedToJoin!=true)
                {
                    dbManager.Ask(user);
                }
            else
            {
                throw new FaultException<string>("You can not ask", "You are already member of a team");
            }    
        }

        public List<User> UsersForJoin()
        {         
            return dbManager.UsersForJoin();
        }

        public void NameBoss(string username, int teamid)
        {
            if (!dbManager.NameBoss(username, teamid))
            {
                throw new FaultException("That team already has a boss or that user is already a boss of another team");
            }
        }
    }
}
