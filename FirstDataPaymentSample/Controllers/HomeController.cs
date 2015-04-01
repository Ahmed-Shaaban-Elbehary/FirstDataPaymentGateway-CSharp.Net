using FirstDataPaymentSample.Api;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FirstDataPaymentSample.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var amount = "1.66";
            var cardExpirtyDate = "0515";
            var cardHolderName = "Card Holder Name";
            var cardNumber = "4111111111111111";
            var customerId = 23;//Internal purpose to track which customer made the transaction....
            var paymentService = new PaymentService(amount, cardExpirtyDate, cardHolderName, cardNumber, customerId.ToString());
            var transactionResult = await paymentService.PostAsync();
            if (transactionResult == null) return View();
            if (transactionResult.TransactionApproved && !transactionResult.TransactionError 
                    && transactionResult.CustomerRef == customerId.ToString())
            {
                //transaction success....
                TempData["TRANSACTION_STATUS"] = "APPROVED";
            }
            else
            {
                var message = transactionResult.Message;
            }
            return View();
        }
    }
}