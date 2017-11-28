using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    public class DatabaseManager
    {
        //On server's request database should return user if it exists...
        public User FindUser(string username)
        {
            using (var access = new AccessDB())
            {

                    User user = access.ListOfUsers.FirstOrDefault(x => x.Username == username);
                    return user;
            }
        }

        public Team FindTeam(int teamId)
        {
            using (var access = new AccessDB())
            {
                Team team = access.ListOfTeams.FirstOrDefault(x => x.TeamName == teamId);
                return team;
            }
        }

        public bool UpdateUser(User user)
        {
            using (var access = new AccessDB())
            {
                try
                {
                    access.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    access.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        public bool AddUser(User user)
        {
            if (AlreadyExists(user.Username) == false)
            {
                using (var access = new AccessDB())
                {
                    access.ListOfUsers.Add(user);
                    int i = access.SaveChanges();
                    if (i > 0)
                    {
                        return true;
                    }
                }
        }
            else
            {
                throw new Exception("User already exists with that name!");
    }

            return false;
        }

        //public bool ChangePassword(User user, string newPassword)
        //{
        //    using (var access = new AccessDB())
        //    {
        //        foreach(User u in access.ListOfUsers)
        //        {
        //            if(u.Username == user.Username)
        //            {
        //                u.Password = newPassword;
        //                break;
        //            }
                        
        //        }
        //        int i = access.SaveChanges();
        //        if (i > 0)
        //         {
        //                return true;
        //         }
        //    }
        //    return false;
        //}

        public bool AlreadyExists(string username)
        {
            using (var access = new AccessDB()) {
                var q = from b in access.ListOfUsers
                        where b.Username == username
                        select b;

                if (q.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public bool AddToTeam(string username, int teamId) //PR
        {
            using (var access = new AccessDB())
            {
                User user = FindUser(username);
                user.Team = teamId;
                user.AskedToJoin = false;
               
                Team t = FindTeam(teamId);               

                if (t == null)
                {
                    t = new Team() { TeamName = teamId, BossId = -1 };
                    access.ListOfTeams.Add(t);
                }

                if(t.BossId!= -1)
                {
                    user.BossId = t.BossId;
                }

                try
                    {
                        access.Entry(user).State = System.Data.Entity.EntityState.Modified;                        
                        access.SaveChanges();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
            } 
              
        }

        public bool NameBoss(string username, int teamId)   //PR
        {
            User user = FindUser(username);
            Team team = FindTeam(teamId);

            using (var access = new AccessDB())
            {
                if (team.BossId == -1 && user.Team!=team.TeamName)
                {   
                    user.Group = "Boss";

                    access.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    access.SaveChanges();


                    team.BossId = user.Id;
                    access.Entry(team).State = System.Data.Entity.EntityState.Modified;

                    access.SaveChanges();

                    foreach (User u in access.ListOfUsers)                                    
                    {
                        if (u.Team == teamId)
                        {
                            u.BossId = team.BossId;

                        }
                    }               
                    access.SaveChanges();
                    return true;        
                }
            }
            return false;
        }

        public bool AddRequest(string username, string start, string end)
        {
            User user = FindUser(username);
            Request request = new Request(start, end, user.Id);



            using (var access = new AccessDB())
            {
                do
                {
                    user = access.ListOfUsers.FirstOrDefault(x => x.Id == user.BossId);
                    request.RequestManagers += user.Id + " ";
              
                } while (user.BossId != -1);

                access.ListOfRequests.Add(request);
                int i = access.SaveChanges();
                
                if(i > 0 )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ApproveRequest(int id, string username)
        {
            using (var access = new AccessDB())
            {
                User user = access.ListOfUsers.FirstOrDefault(x => x.Username == username);
                
                Request request = access.ListOfRequests.FirstOrDefault(x => x.Id == id);

                char c = request.RequestManagers[0]; //Prvi boss
                int bossID = Int32.Parse(c.ToString());
                if(user.Id == bossID)
                {
                    request.Approved = true;
                    WriteToXmlApproved(request, DateTime.Now);
                    access.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    access.SaveChanges();                    
                    return true;
                }

            }
            return false;
        }

        public bool DenyRequest(int id, string username)
        {
            User user = FindUser(username);
            Request req = new Request();
            using (var access = new AccessDB())
            {
                foreach (Request r in access.ListOfRequests)
                {
                    if (r.Id == id)
                    {
                        
                        DateTime dateOfRequest = r.TimeOfRequest;
                        DateTime now = DateTime.Now;
                        TimeSpan interval = now - dateOfRequest;
                        if (interval.Minutes > 100)        //.Days>7
                        {                          
                           return false;
                        }
                        else
                        {
                            string[] bossString = r.RequestManagers.Split(' ');
                            foreach (string s in bossString)
                            {
                                if (Int32.Parse(s) == user.Id)
                                {
                                    r.Denied = true;
                                    r.Approved = false;
                                    req = r;
                                    WriteToXmlDeny(r, DateTime.Now);
                                    DeleteFromXml(r);
                                    break;                                
                                }
                                else
                                {
                                    return false;
                                }
                            }                        
                        }                  
                    }
                }
                access.Entry(req).State = System.Data.Entity.EntityState.Modified;
                access.SaveChanges();
                return true;
            }
            return false;
        }

        public void WriteToXmlApproved(Request r, DateTime time)
        {
            if (!File.Exists("approvedVacations.xml"))
            {
                XmlWriterSettings xmlset = new XmlWriterSettings();
                xmlset.Indent = true;
                xmlset.NewLineOnAttributes = true;
                using (XmlWriter writer = XmlWriter.Create("approvedVacations.xml"))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Requests");
                    writer.WriteStartElement("Request");

                    writer.WriteElementString("StartDate", r.StartDate.ToString());
                    writer.WriteElementString("EndDate", r.EndDate.ToString());
                    writer.WriteElementString("TimeOfRequest", r.TimeOfRequest.ToString());
                    writer.WriteElementString("ApprovedTime", time.ToString());

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                XDocument xdoc = XDocument.Load("approvedVacations.xml");
                XElement root = xdoc.Element("Requests");
                IEnumerable<XElement> rows = root.Descendants("Request");
                XElement firstRow = rows.First();
                firstRow.AddBeforeSelf(new XElement("Request",
                                       new XElement("StartDate", r.StartDate.ToString()),
                                       new XElement("EndDate", r.EndDate.ToString()),
                                       new XElement("TimeOfRequest", r.TimeOfRequest.ToString()),
                                       new XElement("ApprovedTime", time.ToString())
                                       ));
                xdoc.Save("approvedVacations.xml");
            }
        }

        

        public void WriteToXmlDeny(Request r, DateTime time)
        {
            if (!File.Exists("deniedVacations.xml"))
            {
                XmlWriterSettings xmlset = new XmlWriterSettings();
                xmlset.Indent = true;
                xmlset.NewLineOnAttributes = true;
                using (XmlWriter writer = XmlWriter.Create("deniedVacations.xml"))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Requests");
                    writer.WriteStartElement("Request");

                    writer.WriteElementString("StartDate", r.StartDate.ToString());
                    writer.WriteElementString("EndDate", r.EndDate.ToString());
                    writer.WriteElementString("TimeOfRequest", r.TimeOfRequest.ToString());
                    writer.WriteElementString("DeniedTime", time.ToString());

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                XDocument xdoc = XDocument.Load("deniedVacations.xml");
                XElement root = xdoc.Element("Requests");
                IEnumerable<XElement> rows = root.Descendants("Request");
                XElement firstRow = rows.First();
                firstRow.AddBeforeSelf(new XElement("Request",
                                       new XElement("StartDate", r.StartDate.ToString()),
                                       new XElement("EndDate", r.EndDate.ToString()),
                                       new XElement("TimeOfRequest", r.TimeOfRequest.ToString()),
                                       new XElement("DeniedTime", time.ToString())
                                       ));
                xdoc.Save("deniedVacations.xml");
            }
        }

        public void DeleteFromXml(Request r)
        {


        }

        public void Ask(User user)
        {
            using(var access = new AccessDB())
            {
                user.AskedToJoin = true;
                access.Entry(user).State = System.Data.Entity.EntityState.Modified;
                access.SaveChanges();
            }
        }

        public List<User> UsersForJoin()
        {
            List<User> list = new List<User>();
            using(var access = new AccessDB())
            {
                foreach(User u in access.ListOfUsers)
                {
                    if (u.AskedToJoin == true)
                    {
                        list.Add(u);
                    }
                }
            }
            return list;
        }



    }
}
