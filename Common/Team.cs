using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Team
    {
        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        public int TeamName { get; set; }
        [DataMember]
        public int BossId { get; set; }
        
    }
}
