using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Request
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        [Required]
        public string StartDate { get; set; }
        [DataMember]
        [Required]
        public string EndDate { get; set; }
        [DataMember]
        public string RequestManagers { get; set; }
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public bool Approved { get; set; }

        [DataMember]
        public bool Denied { get; set; }

        public DateTime TimeOfRequest { get; set; }

        public Request(string startdate, string enddate, int userid)
        {
            this.StartDate = startdate;
            this.EndDate = enddate;
            this.UserId = userid;
            this.RequestManagers = null;
            this.Approved = false;
            this.TimeOfRequest = DateTime.Now;
            this.Denied = false;
        }

        public Request()
        {

        }
    }
}