using Common;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace Server
{
    class CustomAuthorizationManager : ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            ServiceSecurityContext securityContext = operationContext.ServiceSecurityContext;
            IIdentity callingIdentity = securityContext.PrimaryIdentity;

            var identity = securityContext.PrimaryIdentity;

            var claimsIdentity = new ClaimsIdentity(identity);

            DatabaseManager dbManager = new DatabaseManager();
            User userAccount = dbManager.FindUser(claimsIdentity.Name);

            if(userAccount == null)
            {
                Thread.CurrentPrincipal = new ClaimsPrincipal();
                operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = Thread.CurrentPrincipal;
                return true;
            }

            string role = userAccount.Group;
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));

            Thread.CurrentPrincipal = new ClaimsPrincipal(claimsIdentity);

            operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = Thread.CurrentPrincipal;

            if (Thread.CurrentPrincipal != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
