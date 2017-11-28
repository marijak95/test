using System.Runtime.Serialization;

namespace Common
{
    //Login data transfer object containg security questions
    [DataContract]
    public class LoginDTO
    {
        [DataMember]
        public bool Authenticated { get; set; }

        [DataMember]
        public string SecurityQuestionOne { get; set; }

        [DataMember]
        public string SecurityQuestionTwo { get; set; }
        
        [DataMember]
        public string UserGroup { get; set; }

        public LoginDTO()
        {
            SecurityQuestionOne = null;
            SecurityQuestionTwo = null;
            Authenticated = false;
            UserGroup = null;
        }
    }
}
