using FirstDataPaymentSample.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Xml;

namespace FirstDataPaymentSample.Api
{
    public sealed class PaymentService
    {
        private const string HttpMethod = "POST";
        private const string MessageType = "application/xml";
        private const string TransactionVer = "/transaction/v13";

        private readonly string GatewayId = string.Empty;
        private readonly string GatewayPwd = string.Empty;
        private readonly string TransactionType = string.Empty;
        private readonly string Amount = string.Empty;
        private readonly string CardExpiryDate = string.Empty;
        private readonly string CardHolderName = string.Empty;
        private readonly string CardNumber = string.Empty;
        private readonly string HmacKey = string.Empty;
        private readonly string KeyId = string.Empty;
        private readonly string CustomerId = string.Empty;

        private string ApiUri
        {
            get
            {
                return ConfigurationManager.AppSettings["GATEWAY_URL"] + TransactionVer;
            }
        }

        private string RequestXml
        {
            get
            {
                var builder = new StringBuilder();
                var writer = new StringWriter(builder);
                using (var xml = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = true }))
                {
                    xml.WriteStartElement("Transaction");
                    xml.WriteElementString("ExactID", GatewayId);//Gateway ID
                    xml.WriteElementString("Password", GatewayPwd);//Password
                    xml.WriteElementString("Transaction_Type", TransactionType);
                    xml.WriteElementString("DollarAmount", Amount);
                    xml.WriteElementString("Expiry_Date", CardExpiryDate);
                    xml.WriteElementString("CardHoldersName", CardHolderName);
                    xml.WriteElementString("Card_Number", CardNumber);
                    xml.WriteElementString("Customer_Ref", CustomerId);
                    xml.WriteEndElement();
                    writer.Flush();
                }
                writer = null;
                return builder.ToString();
            }
        }

        internal PaymentService(string amount, string cardExpiryDate, string cardHolderName, string cardNumber, string customerId)
        {
            GatewayId = ConfigurationManager.AppSettings["GATEWAY_ID"];
            GatewayPwd = ConfigurationManager.AppSettings["GATEWAY_PWD"];
            TransactionType = ConfigurationManager.AppSettings["TRANS_TYPE"];
            HmacKey = ConfigurationManager.AppSettings["HMAC_KEY"];
            KeyId = ConfigurationManager.AppSettings["KEY_ID"];
            Amount = amount;
            CardExpiryDate = cardExpiryDate;
            CardHolderName = cardHolderName;
            CardNumber = cardNumber;
            CustomerId = customerId;
        }

        internal async Task<TransactionResult> PostAsync()
        {
            var xmlBytes = new ASCIIEncoding().GetBytes(RequestXml);
            var sha1Crypto = new SHA1CryptoServiceProvider();
            var hashContent = BitConverter.ToString(sha1Crypto.ComputeHash(xmlBytes)).Replace("-", "").ToLower();
            var time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var hashData = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", HttpMethod, MessageType, hashContent, time, TransactionVer);
            var hmacSha = new HMACSHA1(Encoding.UTF8.GetBytes(HmacKey));
            var hmacData = hmacSha.ComputeHash(Encoding.UTF8.GetBytes(hashData));
            var base64Hash = Convert.ToBase64String(hmacData);
            var responseXml = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, ApiUri))
                    {
                        request.Headers.Add("x-gge4-date", time);
                        request.Headers.Add("x-gge4-content-sha1", hashContent);
                        request.Headers.Add("Authorization", "GGE4_API " + KeyId + ":" + base64Hash);
                        request.Content = new StringContent(RequestXml);
                        request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(MessageType);

                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                                responseXml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            else
                                LogStatusCode(response.StatusCode.ToString());
                        }
                        request.Content.Dispose();
                    }
                }
                return await ParseXml(responseXml);
            }
            catch (Exception)
            { 
                /*log exception here*/ 
            }
            return await Task.FromResult<TransactionResult>(null);
        }

        private void LogStatusCode(string httpStatusCode)
        {
             //log your status code
        }

        /// <summary>
        /// convert string xml to object.
        /// </summary>
        /// <param name="responseXml"></param>
        /// <returns></returns>
        private async Task<TransactionResult> ParseXml(string responseXml)
        {
            TransactionResult model = null;
            try
            {
                if (string.IsNullOrWhiteSpace(responseXml)) return await Task.FromResult<TransactionResult>(null);
                 
                var xml = XElement.Parse(responseXml);
                model = (from result in xml.DescendantsAndSelf("TransactionResult")
                        select new TransactionResult
                        {
                            TransactionTag = result.Element("Transaction_Tag").Value,
                            AuthorizationNum = result.Element("Authorization_Num").Value,
                            CustomerRef = result.Element("Customer_Ref").Value,
                            ClientIP = result.Element("Client_IP").Value,
                            TransactionError = Boolean.Parse(result.Element("Transaction_Error").Value),
                            TransactionApproved =  Boolean.Parse(result.Element("Transaction_Approved").Value),
                            BankMessage = result.Element("Bank_Message").Value,
                            CardType = result.Element("CardType").Value,

                        }).FirstOrDefault();
                     
            }
            catch (Exception)
            {
                if (model == null)
                    model = new TransactionResult { CustomerRef = CustomerId, Message = responseXml };
            }
            return await Task.FromResult<TransactionResult>(model);
        }
    }
}