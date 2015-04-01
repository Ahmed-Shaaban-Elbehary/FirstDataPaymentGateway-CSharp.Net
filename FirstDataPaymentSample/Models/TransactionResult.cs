namespace FirstDataPaymentSample.Models
{
    public class TransactionResult
    {
        public string TransactionTag { get; set; }

        public string AuthorizationNum  { get; set; }
      
        public string CustomerRef  { get; set; }

        public string ClientIP  { get; set; }

        public bool TransactionError  { get; set; }
         
        public bool TransactionApproved  { get; set; }
        
        public string BankMessage  { get; set; }
          
        public string CardType  { get; set; }

        public string Message { get; set; }
    }
}