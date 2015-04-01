# FirstDataPaymentGateway-CSharp.Net
A C#  REST based implementation for the FirstData Global Gateway Web Service. Please read the disclaimer section and LICENSE.



This code was taken from the original source https://support.payeezy.com/hc/en-us/articles/204699865-C-REST-Sample-Code-For-Use-with-v12-v13

Example Usage:
```c#
    var paymentService = new PaymentService("1.50", "0515", "Card holder name", 
                          "4111111111111111", "23".ToString()); //customer Id for Internal purpose to track which customer made the transaction...
    var transactionResult = await paymenetService.PostAsync();
    if (transactionResult == null) return;
    if (transactionResult.TransactionApproved && !transactionResult.TransactionError 
            && transactionResult.CustomerRef == customerId.ToString())
    {
        //transaction success....
    }
    else
    {
        //transaction failed
        var message = transactionResult.Message;
    }
```
Configuration Settings:

Following values needs to be changed in the web.config file.
```xml
    <add key="GATEWAY_ID" value="Exact ID" />
    <add key="GATEWAY_PWD" value="Password" />
    <add key="TRANS_TYPE" value="00" />
    <add key="HMAC_KEY" value="HMAC Key" />
    <add key="KEY_ID" value="Key Id" />
    <add key="GATEWAY_URL" value="https://api.demo.globalgatewaye4.firstdata.com" />
```

Transaction Summary file(transaction_summary.xml) from payment gateway is included in the App_Data folder.

Also, refer : https://support.payeezy.com/hc/en-us/articles/203731109-First-Data-Global-Gateway-e4-Web-Service-API-Reference-Guide

As per First Data knowledge base Note: "Code samples are provided "as is" and are not designed to be used in production."




#Disclaimer of Warranty
Disclaimer of Warranty. Unless required by applicable law or
agreed to in writing, Licensor provides the Work (and each
Contributor provides its Contributions) on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied, including, without limitation, any warranties or conditions
of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
PARTICULAR PURPOSE. You are solely responsible for determining the
appropriateness of using or redistributing the Work and assume any
risks associated with Your exercise of permissions under this License.
