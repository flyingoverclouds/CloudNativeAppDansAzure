using CnAppForAzureDev.Extensions;
using CnAppForAzureDev.Managers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Controllers
{
    public class AccountController : BaseController
    {
        IndustryManager _industryManager;
        public AccountController(IndustryManager manager):base(manager)
        {
            _industryManager = manager;
        }
        public IActionResult EditProfile()
        {
            return View("NotImplemented");
            
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpGet]
        [HttpPost]
        public IActionResult LogInFor(string command, string uiLocale)
        {

            if (string.IsNullOrEmpty(uiLocale))
            {
                uiLocale = "en";
            }

            switch (command)
            {
                case "customerSignIn":
                default:
                    return LogInForIndividualCustomer(uiLocale);

                //case "customerSignUp":
                //    return LogInForIndividualCustomer(uiLocale, true);

                case "businessSignIn":
                case "businessSignUp":
                    return LogInForBusinessCustomer(uiLocale);

                case "partnerSignIn":
                    return LogInForPartner(uiLocale);

                //case "partnerSignUp":
                //    return LogInForPartner(uiLocale, true);
            }
        }
        
        public IActionResult MockLoggedIn(int roleType)
        {
            ClaimsPrincipalExtensions.MockIsInBusinessCustomerManagerRole =
            ClaimsPrincipalExtensions.MockIsInIndividualCustomerRole =
            ClaimsPrincipalExtensions.MockIsInEmployeeRole =
            ClaimsPrincipalExtensions.MockIsInPartnerRole =
            ClaimsPrincipalExtensions.MockIsOther = false;
            switch(roleType)
            {
                case 0:
                    ClaimsPrincipalExtensions.MockIsInIndividualCustomerRole = true;
                    Constants.ClaimTypes.MockUser = "Jean SansTerre";
                    break;
                case 1:
                    ClaimsPrincipalExtensions.MockIsInBusinessCustomerManagerRole = true;
                    Constants.ClaimTypes.MockUser = "Richard CoeurdeLyon";
                    break;
                case 2:
                    ClaimsPrincipalExtensions.MockIsInEmployeeRole = true;
                    Constants.ClaimTypes.MockUser = "Henri Plantagenêt";
                    break;
                case 3:
                    ClaimsPrincipalExtensions.MockIsInPartnerRole = true;
                    Constants.ClaimTypes.MockUser = "Alienor D'Aquitaine";
                    break;
                default:
                    ClaimsPrincipalExtensions.MockIsOther = true;
                    Constants.ClaimTypes.MockUser = "John Doe";
                    break;
            }
            IdentityService.MokeUserLoggedIn = true;
            if (User.IsInBusinessCustomerManagerRole() || User.IsInIndividualCustomerRole() || User.IsInEmployeeRole() || User.IsInPartnerRole())
            {
                return RedirectToAction("Index", "CatalogItem");
            }

            return RedirectToAction("Index", "Pantry");
        }
        public IActionResult LoggedIn()
        {
            if (User.IsInBusinessCustomerManagerRole() || User.IsInIndividualCustomerRole() || User.IsInEmployeeRole() || User.IsInPartnerRole())
            {
                return RedirectToAction("Index", "CatalogItem");
            }

            return RedirectToAction("Index", "Pantry");
        }

        public IActionResult LogInForBusinessCustomer(string uiLocale)
        {
            //TODO :mock to sign in 0 Individual, 1 Manager, 2 Employee, 3 Partner, 4 other
            return MockLoggedIn(1);
            //return LogInFor(Constants.AuthenticationSchemes.B2COpenIdConnect, Constants.Policies.SignUpOrSignInWithWorkAccount, uiLocale);
        }

        public IActionResult LogInForIndividualCustomer(string uiLocale)
        {
            //TODO :mock to sign in 0 Individual, 1 Manager, 2 Employee, 3 Partner, 4 other
            return MockLoggedIn(0);
            //return LoggedIn();
            //return LogInFor(Constants.AuthenticationSchemes.B2COpenIdConnect, Constants.Policies.SignUpOrSignInWithPersonalAccount, uiLocale);
        }

        public IActionResult LogInForPartner(string uiLocale)
        {
            //TODO :mock to sign in 0 Individual, 1 Manager, 2 Employee, 3 Partner, 4 other
            return MockLoggedIn(3);
            //return LogInFor(Constants.AuthenticationSchemes.B2BOpenIdConnect, null, uiLocale);
        }

        public async Task<IActionResult> LogOut()
        {
            IdentityService.MokeUserLoggedIn = false;
            return RedirectToHome();
        }

        public IActionResult ResetPassword(string uiLocale)
        {
            throw new NotImplementedException();
        }

        private IActionResult LogInFor(string authenticationScheme, string policy, string uiLocale)
        {


            throw new NotImplementedException();
            //return RedirectToHome();
        }

        private IActionResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
