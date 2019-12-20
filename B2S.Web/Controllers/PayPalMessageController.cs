using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Common;
using B2S.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPal;
using PayPal.Api;

namespace B2S.Web.Controllers
{
    public class PayPalMessageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PayPalOptions _payPalOptions;
        private Payment payment;

        public PayPalMessageController(AppDbContext context ,IOptions<PayPalOptions> payPalOptions)
        {
            _payPalOptions = payPalOptions.Value;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PaymentWithPaypal(string localPaymentId, string Cancel = null)
        {
            if (Cancel == "true")
            {
                return View("Cancel");
            }
                
            //getting the apiContext 
            APIContext apiContext = new APIContext(new OAuthTokenCredential(_payPalOptions.ClientId, _payPalOptions.ClientSecret, GetConfig()).GetAccessToken());
            apiContext.Config = GetConfig();

            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Query["PayerID"].ToString();
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Scheme + "://" + Request.Host + "/PayPalMessageController/PaymentWithPayPal";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = await CreatePayment(localPaymentId, apiContext, baseURI + "?guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    //Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Query["guid"].ToString();
                    var executedPayment = ExecutePayment(apiContext, payerId, guid);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PayPal.ConfigException)
                {
                    string errMsg = (((PayPal.ConfigException)ex.InnerException).InnerException?.Message);
                }
                else if (ex.InnerException is PayPal.ConnectionException)
                {
                    string errMsg = (((PayPal.ConnectionException)ex.InnerException).Response);
                }
                else if (ex.InnerException is PayPal.HttpException)
                {
                    string errMsg = (((PayPal.HttpException)ex.InnerException).Response);
                }
                else if (ex.InnerException is PayPal.IdentityException)
                {
                    string errMsg = (((PayPal.IdentityException)ex.InnerException).Response);
                }
                else if (ex.InnerException is PayPal.InvalidCredentialException)
                {
                    string errMsg = (((PayPal.InvalidCredentialException)ex.InnerException).InnerException?.Message);
                }
                else if (ex.InnerException is PayPal.MissingCredentialException)
                {
                    string errMsg = (((PayPal.MissingCredentialException)ex.InnerException).InnerException?.Message);
                }
                else if (ex.InnerException is PayPal.OAuthException)
                {
                    string errMsg = (((PayPal.OAuthException)ex.InnerException).InnerException?.Message);
                }
                else if (ex.InnerException is PayPal.PaymentsException)
                {
                    string errMsg = (((PayPal.PaymentsException)ex.InnerException).Response);
                }
                else if (ex.InnerException is PayPal.PayPalException)
                {
                    string errMsg = (((PayPal.PayPalException)ex.InnerException).InnerException?.Message);
                }
               
                else
                {
                    string errMsg = (ex.Message);
                }
            }

            //on successful payment, show success page to user.  
            var localPayment = await _context.Payment.FindAsync(localPaymentId);
            if (localPayment != null)
            {
                localPayment.PaymentStatus = Core.Enums.PaymentStatus.Paid;
                _context.Update(localPayment);
                await _context.SaveChangesAsync();
            }      
            return View("Success");
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private async Task<Payment> CreatePayment(string localPaymentId, APIContext apiContext, string redirectUrl)
        {
            var localPayment = await _context.Payment.FindAsync(localPaymentId);
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = "Booking",
                currency = "USD",
                price = localPayment.Price.ToString(),
                quantity = ((int)localPayment.Quantity).ToString(),
                sku = "sku"
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl + "&localPaymentId=" + localPaymentId
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = localPayment.Amount.ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = localPayment.Amount.ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "Transaction for booking",
                invoice_number = GetRandomInvoiceNumber(99999), //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "Order",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

        // getting properties from the paypal.config  
        private Dictionary<string, string> GetConfig()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                ["mode"] = _payPalOptions.Mode,
                ["clientId"] = _payPalOptions.ClientId,
                ["clientSecret"] = _payPalOptions.ClientSecret
            };
            return config;
        }

        private string GetRandomInvoiceNumber(int numb)
        {
            return new Random().Next(numb).ToString();
        }
    }
}