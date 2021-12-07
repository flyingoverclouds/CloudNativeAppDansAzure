using CnAppForAzureDev.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Managers
{
    public class IndustryManager
    {
        
        private Industry _currentIndustry = Industry.Groceries;
        public enum Industry
        {
            Groceries,
            Banking,
            Wellness,
            None
        }
        private IConfiguration _configuration;
        public IndustryManager(IConfiguration configuration)
        {            
            _configuration = configuration;
        }
        public Industry SetIndustry(HttpRequest request)
        {

            Industry industry = Industry.Groceries;
            _currentIndustry = industry;
            return industry;
        }
        public IIndustry GetIndustry()
        {
            return new IndustryGroceries();
        }
    }
    public interface IIndustry
    {
        string Title { get; }
        string IndividualCustomerAccountType { get; }

        string LandingPageMainText { get; }
        string LandingPageSubText { get; }
        string LandingPageTextStyling { get; }
        string LandingPageProductDisplayTitle { get; }

        string BackgroundImage { get; }

        string LoginPageIndividualCustomerImage { get; }
        string LoginPageIndividualCustomerTitle { get; }
        string LoginPageIndividualCustomerSubText { get; }
        string LoginPageIndividualCustomerAccountName { get; }

        string LoginPageBusinessCustomerImage { get; }
        string LoginPageBusinessCustomerTitle { get; }
        string LoginPageBusinessCustomerSubText { get; }
        string LoginPageBusinessCustomerAccountName { get; }

        string LoginPagePartnerImage { get; }
        string LoginPagePartnerTitle { get; }
        string LoginPagePartnerSubText { get; }
        string LoginPagePartnerAccountName { get; }

        string DefaultCustomerAuthBackground { get; }

        string CatalogItemsCategory1 { get; }
        string CatalogItemsCategory2 { get; }
        string CatalogItemsCategory3 { get; }

        string CatalogAddToCart { get; }

        string TrolleyCartEmpty { get; }
        string TrolleyProductHeading { get; }
        string TrolleyContinueShopping { get; }
        string TrolleyCompletePurchase { get; }

        string CheckoutMessage { get; }

        string CatalogHeader { get; }
        string PantryHeader { get; }
        string CartHeader { get; }
        string CartHeaderLogo { get; }
        string CartAlertIndicator { get; }
        bool ItemMultiPurchasable { get; }            
    }
   
    public class IndustryGroceries : IIndustry
    {
        
        public IndustryGroceries()
        {
        
        }
        
        #region STRINGS
        
        public string Title => "Woodgrove Groceries";
        
        public string LoginPageIndividualCustomerTitle => "Individual customers";

        public string LandingPageMainText => "Save Time";
        public string LandingPageSubText => "Let us do the grocery shopping for you";
        public string LandingPageProductDisplayTitle => "SPECIALS";
        public string CatalogHeader => "Catalog";
        public string CheckoutMessage => "Thank you for shopping at Woodgrove Groceries. Your order is being shipped.";
        public string LoginPageIndividualCustomerSubText =>
           "Order your fresh groceries with WoodGrove Groceries and our friendly drivers will deliver your grocery shopping to your home door.";


        public string LandingPageTextStyling => "color: white;";        
        //TODO: replace with dynamic URL
        public string BackgroundImage => "../images/groceries/background.jpg";
        

        public string LoginPageIndividualCustomerImage => "../images/groceries/sign-in-b2c-personal.jpg";
        

       

        public string LoginPageIndividualCustomerAccountName => "personal account";

        public string LoginPageBusinessCustomerImage => "../images/groceries/sign-in-b2c-work.jpg";
        public string LoginPageBusinessCustomerTitle => "Business customers";

        public string LoginPageBusinessCustomerSubText =>
            "Order your fresh groceries with WoodGrove Groceries and our friendly drivers will deliver your grocery shopping to your office door.";

        public string LoginPageBusinessCustomerAccountName => "work account";

        public string LoginPagePartnerImage => "../images/groceries/sign-in-partner.jpg";
        public string LoginPagePartnerTitle => "Partners";

        public string LoginPagePartnerSubText =>
            "Manage your local produce in our inventory so we can deliver your fresh groceries to our customers.";

        public string LoginPagePartnerAccountName => "supplier account";

        public string DefaultCustomerAuthBackground => "images/groceries/background.jpg";

        public string CatalogItemsCategory1 => "Bakery";
        public string CatalogItemsCategory2 => "Dairy & Eggs";
        public string CatalogItemsCategory3 => "Fruits & Vegetables";

        public string TrolleyCartEmpty => "Your cart is empty";
        public string TrolleyProductHeading => "Product";
        public string TrolleyContinueShopping => "Continue shopping";
        public string TrolleyCompletePurchase => "Complete Purchase";

        

        public string CatalogAddToCart => "Add To Cart";

        
        public string CartHeader => "Cart";
        public string PantryHeader => "Pantry";
        public string CartHeaderLogo => "fa-shopping-cart";
        public string CartAlertIndicator => "images/alerticon.png";

        public bool ItemMultiPurchasable => true;

        public string IndividualCustomerAccountType => "Individual Customer";
        #endregion
      
    }
}
