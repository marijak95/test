namespace Client
{
    public class ClientAccount
    {
        public string Username;
        public string Password;
        public string SecurityQuestionOne;
        public string SecurityQuestionTwo;

        public bool Authenticated;
        public bool RequireSafeLogin;

        public string ClientGroup;

        public ClientAccount()
        {
            Username = null;
            Password = null;
            Authenticated = false;
            SecurityQuestionOne = null;
            SecurityQuestionTwo = null;
            RequireSafeLogin = false;
            ClientGroup = null;
        }
    }
}
