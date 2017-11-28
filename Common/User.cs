using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class User
    {
        public int Id { get; set; }
        [DataMember]
        [Required]
        public string Username { get; set; }

        [DataMember]
        [Required]
        public string FirstName { get; set; }

        [DataMember]
        public int RequireSafeLogin { get; set; }

        [DataMember]
        [Required]
        public string LastName { get; set; }

        [DataMember]
        [Required]
        public string Password { get; set; }

        [DataMember]
        [Required]
        public string QuestionOne { get; set; }

        [DataMember]
        [Required]
        public string QuestionTwo { get; set; }

        [DataMember]
        [Required]
        public string AnswerOne { get; set; }

        [DataMember]
        [Required]
        public string AnswerTwo { get; set; }

        [DataMember]
        public int BossId { get; set; }

        [DataMember]
        public int Team { get; set; }

        [DataMember]
        public string Group { get; set; }      //role

        [DataMember]
        public bool AskedToJoin { get; set; }


        public User(string username, string firstName, string lastName, string password, string questionOne, string questionTwo, string answerOne, string answerTwo)
        {
            this.Username = username;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;
            this.QuestionOne = questionOne;
            this.QuestionTwo = questionTwo;
            this.AnswerOne = answerOne;
            this.AnswerTwo = answerTwo;
            this.BossId = -1;
            this.Team = -1;
            this.RequireSafeLogin = 0;
            this.AskedToJoin = false;
        }

        public User()
        {

        }
    }
}
