using Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.ServiceModel;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Client
{
    public class UserInterface
    {
        public void InputCredentials(ClientAccount clientAccount)
        {
            Console.WriteLine("=============================");
            Console.WriteLine("Input your credentials:");
            Console.WriteLine("Input username:");
            clientAccount.Username = Console.ReadLine();
            Console.WriteLine("Input password:");
            clientAccount.Password = Console.ReadLine();

        }

        public User InputUserInfo()
        {
            Console.WriteLine("=============================");
            Console.WriteLine("Register new account:");
            Console.WriteLine("Input username:");
            string username = Console.ReadLine();
            Console.WriteLine("Input password:");
            string password = Console.ReadLine();
            Console.WriteLine("Input first name:");
            string firstName = Console.ReadLine();
            Console.WriteLine("Input last name:");
            string lastName = Console.ReadLine();

            Console.WriteLine("Input security question one: ");
            string questionOne = Console.ReadLine();
            Console.WriteLine("Input security answer one:");
            string answerOne = Console.ReadLine();

            Console.WriteLine("Input security question two: ");
            string questionTwo = Console.ReadLine();
            Console.WriteLine("Input security answer two:");
            string answerTwo = Console.ReadLine();

            User newUserAccount = new User(username, firstName, lastName, password, questionOne, questionTwo, answerOne, answerTwo);

            return newUserAccount;
        }

        public int AnonymousUserInterface(ref IWorkgroupService workgroupProxy, IRegistrationService registrationProxy, ConnectionHandler connectionHandler, ClientAccount clientAccount)
        {
            User user = new User();
            bool operationSuccess;
            int choice = 0;
            bool ok = false;

            Console.WriteLine("---AUTHENTICATION SERVICE---\n");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");

            do
            {
                Console.Write("Choice: ");

                if (!Int32.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Choice is a number!");
                }
                ok = true;
            } while (ok != true);

            switch (choice)
            {
                case 1:
                    {
                        string answerOne = null;
                        string answerTwo = null;
                        LoginDTO loginInformation = new LoginDTO();

                        InputCredentials(clientAccount);
                        if(clientAccount.RequireSafeLogin)
                        {
                            Console.WriteLine("Server demands safe login. Input answers to security questions in order to log in");
                            Console.WriteLine(clientAccount.SecurityQuestionOne);
                            answerOne = Console.ReadLine();
                            Console.WriteLine(clientAccount.SecurityQuestionTwo);
                            answerTwo = Console.ReadLine();
                        }

                        if(connectionHandler.workgroupChannel.Credentials.UserName.UserName != null) //Reopen channel for new credentials
                        {
                            if(connectionHandler.CloseWorkgroupChannel()) 
                            {
                                connectionHandler.OpenWorkgroupChannel();
                                connectionHandler.workgroupChannel.Credentials.UserName.UserName = clientAccount.Username;
                                connectionHandler.workgroupChannel.Credentials.UserName.Password = clientAccount.Password;
                                workgroupProxy = connectionHandler.workgroupChannel.CreateChannel();
                            }
                        }
                        else
                        {
                            connectionHandler.workgroupChannel.Credentials.UserName.UserName = clientAccount.Username;
                            connectionHandler.workgroupChannel.Credentials.UserName.Password = clientAccount.Password;
                            workgroupProxy = connectionHandler.workgroupChannel.CreateChannel();
                        }

                        try
                        {
                            loginInformation = workgroupProxy.Login(clientAccount.Username, clientAccount.Password, answerOne, answerTwo);
                        }
                        /*catch(ArgumentException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        catch(FaultException fe)
                        {
                            Console.WriteLine(fe.Message, fe.Reason);
                        }*/
                        catch(Exception e)
                        {
                            if(e is FaultException)
                            {
                                Console.WriteLine(e.Message);
                            }
                            if(e is ArgumentException)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        if(loginInformation.Authenticated)
                        {
                            clientAccount.Authenticated = true;
                            clientAccount.ClientGroup = loginInformation.UserGroup;
                        }
                        else
                        {
                            if(loginInformation.SecurityQuestionOne != null)
                            {
                                clientAccount.RequireSafeLogin = true;
                                clientAccount.SecurityQuestionOne = loginInformation.SecurityQuestionOne;
                                clientAccount.SecurityQuestionTwo = loginInformation.SecurityQuestionTwo;
                            }
                            
                            Console.WriteLine("Failed to log in");
                        }

                        break;
                    }
                case 2:
                    {
                        User newUserAccount = InputUserInfo();
                        try
                        {
                            operationSuccess = registrationProxy.Register(newUserAccount);
                            if (operationSuccess)
                            {
                                Console.WriteLine("Successfully registered a new account!");
                            }
                        }
                        catch (FaultException fe)
                        {
                            Console.WriteLine(fe.Message);
                            Console.WriteLine(fe.InnerException);
                        }
                        break;
                    }
                case 0:
                    break;
            }
            return choice;
        }

        public void WorkerWindow(ClientAccount ca, IWorkgroupService workgroupProxy, ConnectionHandler connectionHandler)
        {

            User u = workgroupProxy.FindUser(ca.Username);
            int choice = 0;
            bool ok = false;
            do
            {
                Console.WriteLine("**********************************************");
                Console.WriteLine("Hello " + u.Username);
                Console.WriteLine("1. Request to get in the team");
                Console.WriteLine("2. Request for vacation");
                Console.WriteLine("3. Change password");
                Console.WriteLine("4. Logout");

                if (ca.ClientGroup == "Boss")
                    Console.WriteLine("5. See list of requests");

                do
                {
                    Console.Write("Choice: ");

                    if (!Int32.TryParse(Console.ReadLine(), out choice))
                    {
                        Console.WriteLine("Choice is a number!");
                    }
                    ok = true;
                } while (ok != true);

                switch (choice)
                {
                    case 1:
                        {
                            try
                            {
                                workgroupProxy.Ask(u.Username);
                                Console.WriteLine("Your request for joining group is sent");
                            }
                            catch (FaultException fe)
                            {
                                Console.WriteLine(fe.Message, fe.Reason);
                            }
                            break;
                        }
                    case 2:
                        {
                            DateTime timeNow = DateTime.Now;
                            int result = 0;
                            int result2 = 0;
                            string start, end;
                            do
                            {
                                Console.WriteLine("Starting date[mm/dd/yyyy]: ");
                                start = Console.ReadLine();
                                try
                                {
                                    DateTime dt = Convert.ToDateTime(start);
                                    result = DateTime.Compare(Convert.ToDateTime(start), timeNow);       //ako je start pre trenutka onda je result = -1
                                }
                                catch
                                {
                                    Console.WriteLine("Not a good date format.");
                                }

                            } while (result <= 0);


                            do
                            {
                                Console.WriteLine("Ending date[mm/dd/yyyy]: ");
                                end = Console.ReadLine();
                                try
                                {
                                    DateTime dt = Convert.ToDateTime(end);
                                    result = DateTime.Compare(Convert.ToDateTime(end), timeNow);
                                    result2 = DateTime.Compare(Convert.ToDateTime(end), Convert.ToDateTime(start));
                                }
                                catch
                                {
                                    Console.WriteLine("Not a good date format.");
                                }


                            } while (result <= 0 || result2 <= 0);

                            try
                            {
                                workgroupProxy.RequestVacation(u.Username, start, end);
                            }
                            catch (FaultException fe)
                            {
                                Console.WriteLine(fe.Reason);
                            }


                            break;
                        }
                    
                    case 3:
                        {
                            bool correctpass = false;
                            do
                            {
                                Console.WriteLine("Input old password");
                                string oldpass = Console.ReadLine();
                                if (u.Password != oldpass)
                                {
                                    Console.WriteLine("You entered wrong password.");
                                }
                                Console.WriteLine("Input new password");
                                string newpass = Console.ReadLine();
                                try
                                {
                                    correctpass = workgroupProxy.ChangePassword(u.Username, oldpass, newpass);
                                }
                                catch (FaultException fe)
                                {
                                    Console.WriteLine(fe.Reason);
                                }
                            } while (correctpass == false);
                            break;
                        }
                    case 4:
                        {
                            bool loggedout = false;
                            loggedout = workgroupProxy.Logout(u.Username);
                            Environment.Exit(0);
                            break;
                        }
                    case 5:
                        {
                            List<Request> list = workgroupProxy.AllRequests(u.Username);
                            int no = list.Count;

                            if (no != 0)
                            {

                                Console.WriteLine("**********REQUESTS********");
                                foreach (Request r in list)
                                {
                                    Console.WriteLine("User that asked for vacation: " + r.UserId);
                                    Console.WriteLine("Id: " + r.Id);
                                    Console.WriteLine("Starting date: " + r.StartDate);
                                    Console.WriteLine("Ending date: " + r.EndDate);
                                }
                                Console.WriteLine("**********************");
                                Console.WriteLine("1. Approve request");
                                Console.WriteLine("2. Deny request");
                                Console.WriteLine("Choice: ");
                                int choice2 = Int32.Parse(Console.ReadLine());
                                if (choice2 == 1)
                                {
                                    Console.WriteLine("Input ID of request you want to approve");
                                    int id;

                                    if (!Int32.TryParse(Console.ReadLine(), out id))
                                    {
                                        Console.WriteLine("ID is number!");
                                        break;
                                    }

                                    try
                                    {
                                        workgroupProxy.ApproveRequest(id, ca.Username);
                                    }
                                    catch (FaultException fe)
                                    {
                                        Console.WriteLine(fe.Reason);
                                    }
                                }
                                else if (choice2 == 2)
                                {
                                    Console.WriteLine("Input ID of request you want to deny");
                                    int id;

                                    if (!Int32.TryParse(Console.ReadLine(), out id))
                                    {
                                        Console.WriteLine("ID is number!");
                                        break;
                                    }

                                    try
                                    {
                                        workgroupProxy.DenyRequest(id, ca.Username);
                                    }
                                    catch (FaultException fe)
                                    {
                                        Console.WriteLine(fe.Reason);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("There are no requests for vacation.");
                            }

                            break;
                        }
                }

            } while (choice != 4);
        }

        public void AdminWindow(ClientAccount ca, IWorkgroupService workgroupProxy)
        {
            User u = workgroupProxy.FindUser(ca.Username);
            Console.WriteLine("***********************");
            Console.WriteLine("Hello admin");

            int choice = 0;
            bool ok = false;
            do
            {
                Console.WriteLine("1. See requests for groups");
                Console.WriteLine("2. See requests for vacation");
                Console.WriteLine("3. Name boss");
                Console.WriteLine("4. Change password");
                Console.WriteLine("5. Logout");

                do
                {
                    Console.Write("Choice: ");

                    if (!Int32.TryParse(Console.ReadLine(), out choice))
                    {
                        Console.WriteLine("Choice is a number!");
                    }
                    ok = true;
                } while (ok != true);

                switch (choice)
                {
                    case 1:
                        {
                            List<User> list = workgroupProxy.UsersForJoin();
                            int i = 0;
                            
                            int choice2;

                            int no = list.Count;
                            if (no != 0)
                            {
                                Console.WriteLine("Put users in group:");
                                foreach (User user in list)
                                {
                                    i++;
                                    Console.WriteLine("{0}. {1}", i, user.Username);
                                }

                                do
                                {
                                    bool correct = false;
                                    string username;
                                    int teamId;

                                    do
                                    {
                                        Console.WriteLine("Enter username of user you want to put in group:");
                                        username = Console.ReadLine();

                                        foreach (User user in list)
                                        {
                                            if (user.Username == username)
                                            {
                                                correct = true;
                                            }
                                        }

                                        if (correct == false)
                                        {
                                            Console.WriteLine("You put wrong username!");
                                        }
                                    } while (correct != true);

                                    do
                                    {
                                        Console.WriteLine("In which team (1, 2, 3, 4, 5...) you want to put him?");

                                        if (Int32.TryParse(Console.ReadLine(), out teamId))
                                        {
                                            correct = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Name of a team is number!");
                                            correct = false;
                                        }

                                    } while(correct != true);
                                    
                                    try
                                    {
                                        workgroupProxy.AddToTeam(username, teamId);
                                    }
                                    catch (FaultException fe)
                                    {
                                        Console.WriteLine(fe.Reason);
                                    }
                                    Console.WriteLine("For exit, press 0");
                                    choice2 = Int32.Parse(Console.ReadLine());
                                } while (choice2 != 0);
                            }
                            else
                            {
                                Console.WriteLine("There are no requests for joining to group.");
                                break;
                            }
                            break;
                        }
                    case 2:
                        {
                            List<Request> list = workgroupProxy.AllRequests(u.Username);
                            int no = list.Count;

                            if (no != 0)
                            {
                                Console.WriteLine("**********REQUESTS********");
                                foreach (Request r in list)
                                {
                                    Console.WriteLine("User that asked for vacation: " + r.UserId);
                                    Console.WriteLine("Id: " + r.Id);
                                    Console.WriteLine("Starting date: " + r.StartDate);
                                    Console.WriteLine("Ending date: " + r.EndDate);
                                }
                                Console.WriteLine("**************************************");
                                Console.WriteLine("1. Approve request");
                                Console.WriteLine("2. Deny request");
                                Console.WriteLine("Choice: ");
                                int choice2 = Int32.Parse(Console.ReadLine());

                                if (choice2 == 1)
                                {
                                    Console.WriteLine("Input ID of request you want to approve");
                                    int id;

                                    if (!Int32.TryParse(Console.ReadLine(), out id))
                                    {
                                        Console.WriteLine("ID is number!");
                                        break;
                                    }

                                    try
                                    {
                                        workgroupProxy.ApproveRequest(id, ca.Username);
                                    }
                                    catch (FaultException fe)
                                    {
                                        Console.WriteLine(fe.Reason);
                                    }
                                }
                                else if (choice2 == 2)
                                {
                                    Console.WriteLine("Input ID of request you want to deny");
                                    int id;

                                    if (!Int32.TryParse(Console.ReadLine(), out id))
                                    {
                                        Console.WriteLine("ID is number!");
                                        break;
                                    }

                                    try
                                    {
                                        workgroupProxy.ApproveRequest(id, ca.Username);
                                    }
                                    catch (FaultException fe)
                                    {
                                        Console.WriteLine(fe.Reason);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("There are no requests for vacation.");
                            }
                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("List of workers");
                            List<User> list = workgroupProxy.AllUsers();
                            foreach(User user in list)
                            {
                                Console.WriteLine("username: {0} team: {1}", user.Username, user.Team);
                            }
                            
                            string boss;
                            int team;
                            bool correct = false;

                            do
                            {
                                Console.WriteLine("Who do you want to name as boss?");
                                boss = Console.ReadLine();

                                foreach (User user in list)
                                {
                                    if (user.Username == boss)
                                    {
                                        correct = true;
                                    }
                                }

                                if (correct == false)
                                {
                                    Console.WriteLine("You put wrong username!");
                                }
                            } while (correct != true);


                            do
                            {
                                Console.WriteLine("He/She will be boss of which team?");

                                if (Int32.TryParse(Console.ReadLine(), out team))
                                {
                                    correct = true;
                                }
                                else
                                {
                                    Console.WriteLine("Name of a team is number!");
                                    correct = false;
                                }

                            } while (correct != true);

                            try
                            {
                                workgroupProxy.NameBoss(boss, team);
                            }
                            catch(FaultException fe)
                            {
                                Console.WriteLine(fe.Reason);
                            }
                         break;
                        }

                    case 4:
                        {
                            bool correctpass = false;
                            do
                            {
                                Console.WriteLine("Input old password");
                                string oldpass = Console.ReadLine();
                                if (u.Password != oldpass)
                                {
                                    Console.WriteLine("You entered wrong password.");
                                }
                                
                                Console.WriteLine("Input new password");
                                string newpass = Console.ReadLine();
                                try
                                {
                                    correctpass = workgroupProxy.ChangePassword(u.Username, oldpass, newpass);
                                }
                                catch (FaultException fe)
                                {
                                    Console.WriteLine(fe.Reason);
                                }
                            } while (correctpass == false);
                            break;
                        }
                    case 5:
                        {
                            bool loggedout = false;
                            loggedout = workgroupProxy.Logout(u.Username);
                            Environment.Exit(0);
                            break;
                        }
                }
            } while (choice != 5);

        }

    }
}
