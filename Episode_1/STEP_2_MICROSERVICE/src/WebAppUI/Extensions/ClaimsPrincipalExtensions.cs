using CnAppForAzureDev.Managers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        //TODO : Mock        
        public static bool MockIsInBusinessCustomerManagerRole { get; set; }
        public static bool MockIsInIndividualCustomerRole;
        public static bool MockIsInEmployeeRole;
        public static bool MockIsInPartnerRole;
        public static bool MockIsOther;

        public static bool IsAllergicTo(this ClaimsPrincipal principal, string productAllergens)
        {
            if (!principal.Claims.Any(x => x.Type == "Allergens")
                || string.IsNullOrEmpty(productAllergens)
                || string.IsNullOrEmpty(principal.Claims.First(x => x.Type == "Allergens").Value)
            )
            {
                return false;
            }

            var productAllergenList = productAllergens.Split(",").ToList();

            var allergenClaim = principal.Claims.First(x => x.Type == "Allergens").Value.Split(",").ToList();

            return allergenClaim.Any(x => productAllergenList.Contains(x));
        }
        public static string GetRoleForDisplay(this ClaimsPrincipal principal, IIndustry industry)
        {
            if (principal.IsInPartnerRole())
            {
                return "Partner";
            }

            if (principal.IsInBusinessCustomerManagerRole())
            {
                //TODO : uncomment when authentication is enabled
                var organizationName = principal.FindFirstValue(Constants.ClaimTypes.OrganizationName);
                organizationName = "CNA";
                return $"{organizationName} Manager";
            }

            if (principal.IsInBusinessCustomerStockerRole())
            {
                var organizationName = principal.FindFirstValue(Constants.ClaimTypes.OrganizationName);
                return $"{organizationName} Stocker";
            }

            if (principal.IsInEmployeeRole())
            {
                return "Employee";
            }

            return industry?.IndividualCustomerAccountType;
        }
        public static bool IsSocialAccountLinked(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.LinkedSocialAccount))
            {
                return Convert.ToBoolean(principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.LinkedSocialAccount).Value);
            }

            return false;
        }
        public static string GetIdentityProvider(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.IdentityProvider))
            {
                return principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.IdentityProvider).Value;
            }

            return string.Empty;
        }
        public static string GetGroup(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.Group))
            {
                return principal.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Group).Value;
            }

            return string.Empty;
        }
        public static string GetPictureUrl(this ClaimsPrincipal principal)
        {
            if (!HasIdentityProvider(principal))
            {
                return string.Empty;
            }

            if (!HasIdpAccessToken(principal))
            {
                return string.Empty;
            }

            switch (GetIdentityProvider(principal))
            {
                case Constants.IdentityProvider.Facebook:
                    //TODO :uncomment if you implement social authentication
                    //try
                    //{
                    //    var client = new HttpClient();
                    //    var response = client.GetAsync("https://graph.facebook.com/me?fields=picture&access_token=" + GetAccessToken(principal))
                    //        .Result;
                    //    response.EnsureSuccessStatusCode();
                    //    var responseBody = response.Content.ReadAsStringAsync().Result;

                    //    var faceBookClaimResponse = JsonConvert.DeserializeObject<FaceBookClaimResponse>(responseBody);

                    //    if (faceBookClaimResponse == null || faceBookClaimResponse.picture == null || faceBookClaimResponse.picture.data == null ||
                    //        string.IsNullOrEmpty(faceBookClaimResponse.picture.data.url))
                    //    {
                    //        return string.Empty;
                    //    }

                    //    var imageByteArray = client.GetByteArrayAsync(faceBookClaimResponse.picture.data.url).Result;

                    //    if (imageByteArray == null)
                    //    {
                    //        return string.Empty;
                    //    }

                    //    return Convert.ToBase64String(imageByteArray);
                    //}
                    //catch
                    //{
                    //    return string.Empty;
                    //}
                default:
                    return string.Empty;
            }
        }
        public static bool HasIdpAccessToken(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == "idp_access_token"))
            {
                return true;
            }

            return false;
        }
        public static bool HasGroup(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.Group))
            {
                return true;
            }

            return false;
        }

        public static int GetUserProfilePercentage(this ClaimsPrincipal principal)
        {
            //TODO : Mock here
            int completion = 75;
            if (principal.Claims.Any(x => x.Type == "ProfileCompletion")
                && !string.IsNullOrEmpty(principal.Claims.First(x => x.Type == "ProfileCompletion").Value))
            {
                
                return Convert.ToInt32(principal.Claims.First(x => x.Type == "ProfileCompletion").Value);
            }

            return completion;
        }
        public static bool HasIdentityProvider(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Any(x => x.Type == Constants.ClaimTypes.IdentityProvider))
            {
                return true;
            }

            return false;
        }
        public static string GetRoleForDisplay(this ClaimsPrincipal principal)
        {
            if (principal.IsInPartnerRole())
            {
                return "Partner";
            }

            if (principal.IsInBusinessCustomerManagerRole())
            {
                var organizationName = principal.FindFirstValue(Constants.ClaimTypes.OrganizationName);
                return $"{organizationName} Manager";
            }

            if (principal.IsInBusinessCustomerStockerRole())
            {
                var organizationName = principal.FindFirstValue(Constants.ClaimTypes.OrganizationName);
                return $"{organizationName} Stocker";
            }

            if (principal.IsInEmployeeRole())
            {
                return "Employee";
            }

            return "Individual Customer";
        }

      
        public static bool IsInBusinessCustomerManagerRole(this ClaimsPrincipal principal)
        {
            //TODO: comment when authentication is enabled
            return MockIsInBusinessCustomerManagerRole;
            //return principal.IsInRole(Constants.Roles.BusinessCustomerManager);
        }

        public static bool IsInBusinessCustomerStockerRole(this ClaimsPrincipal principal)
        {
            //TODO: comment when authentication is enabled
            return MockIsOther;
            //return principal.IsInRole(Constants.Roles.BusinessCustomerStocker);
        }

        public static bool IsInEmployeeRole(this ClaimsPrincipal principal)
        {
            //TODO: comment when authentication is enabled
            return MockIsInEmployeeRole;
            //return principal.IsInRole(Constants.Roles.Employee);
        }

        public static bool IsInIndividualCustomerRole(this ClaimsPrincipal principal)
        {
            //TODO: comment when authentication is enabled
            return MockIsInIndividualCustomerRole;
            //return principal.IsInRole(Constants.Roles.IndividualCustomer);
        }

        public static bool IsInPartnerRole(this ClaimsPrincipal principal)
        {
            //TODO: comment when authentication is enabled
            return MockIsInPartnerRole;
            //return principal.IsInRole(Constants.Roles.Partner);
        }
    }
    public class IdentityService
    {
        private readonly IConfiguration _configuration;

        public IdentityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        public static bool MokeUserLoggedIn { get; set; }
        public bool IsUserLoggedIn(ClaimsPrincipal principal)
        {
            
            return MokeUserLoggedIn;
            //TODO : uncomment when authentication is enabled
            //var config = AuthenticationBetaAppAccessOptions.Construct(_configuration);

            //if (!config.RequireFullAppAuth)
            //{
            //    return principal.Identity.IsAuthenticated;
            //}

            //if (!principal.Identity.IsAuthenticated)
            //{
            //    return false;
            //}

            //var tenantId = principal.FindFirstValue(Constants.ClaimTypes.TenantIdentifier);

            //return tenantId != config.TenantId;
        }
    }
}
