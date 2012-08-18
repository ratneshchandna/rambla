using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using otr_project.Models;

namespace otr_project.Utils
{
    /// <summary>
    /// Summary description for NVPAPICaller
    /// </summary>
    public class NVPAPICaller
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(NVPAPICaller));

        MarketPlaceEntities market = new MarketPlaceEntities();

        string returnURL = "";
        string cancelURL = "";

        string PayPalToken = "";
        string PayerID = "";

        private string pendpointurl = "https://api-3t.paypal.com/nvp";
        private const string CVV2 = "CVV2";

        //Flag that determines the PayPal environment (live or sandbox)
        private const bool bSandbox = true;

        private const string SIGNATURE = "SIGNATURE";
        private const string PWD = "PWD";
        private const string ACCT = "ACCT";
        private const string PAYMENT = "L_PAYMENTREQUEST_0_";

        //Replace <API_USERNAME> with your API Username
        //Replace <API_PASSWORD> with your API Password
        //Replace <API_SIGNATURE> with your Signature

        // Subu's API credentials (Sandbox)
        public string APIUsername = "seller_1316286746_biz_api1.gmail.com";
        private string APIPassword = "1316286791";
        private string APISignature = "AFWhlTuogu.Dp3IoiJdWzRmrFDQiA9OnAyXrOU7bmrY8cfcKhLIIk7ff";
        
        /* // Non Sandbox (Live) Admin API credentials
        public string APIUsername = "admin_api1.rambla.ca";
        private string APIPassword = "7QB7Q2ZU4N5SKQAB";
        private string APISignature = "AFcWxV21C7fd0v3bYYYRCpSSRl31A2U9P67j5sjmGOP3FQiYLbF9NuuZ";

        /* // Rattu's API credentials
        public string APIUsername = "me_1328686156_biz_api1.ascensioncap.ca";
        private string APIPassword = "1328686179";
        private string APISignature = "A9fPlbDWfaCuadSUUFJydlp7J.c6A6d18Jj78z9e.JMsJzeClL5efcdk"; */

        private string Subject = "";
        private string BNCode = "PP-ECWizard";

        //HttpWebRequest Timeout specified in milliseconds 
        private const int Timeout = 50000;
        private static readonly string[] SECURED_NVPS = new string[] { ACCT, CVV2, SIGNATURE, PWD };

        public NVPAPICaller(string token, string PayerID, string total)
        {
            this.PayPalToken = token;
            this.PayerID = PayerID;
        }
        
        public NVPAPICaller(string complete, string cancel)
        {
            this.returnURL = complete;
            this.cancelURL = cancel;
        }

        /// <summary>
        /// Sets the API Credentials
        /// </summary>
        /// <param name="Userid"></param>
        /// <param name="Pwd"></param>
        /// <param name="Signature"></param>
        /// <returns></returns>
        public void SetCredentials(string Userid, string Pwd, string Signature)
        {
            APIUsername = Userid;
            APIPassword = Pwd;
            APISignature = Signature;
        }


        public NVPCodec GetNVPFromOrder(Order order)
        {
            NVPCodec encoder = new NVPCodec();
            int count = 0;
            var orderDetails = market.OrderDetails.Where(o => o.OrderId == order.OrderId).ToList();
            
            encoder["METHOD"] = "SetExpressCheckout";
            encoder["RETURNURL"] = returnURL;
            encoder["CANCELURL"] = cancelURL;
            encoder["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale";
            encoder["PAYMENTREQUEST_0_CURRENCYCODE"] = "CAD";
            encoder["NOSHIPPING"] = "1";
            //encoder["AMT"] = order.Total.ToString();
            encoder["PAYMENTREQUEST_0_ITEMAMT"] = order.Total.ToString();
            encoder["PAYMENTREQUEST_0_AMT"] = order.Total.ToString();

            foreach (OrderDetailModel o in orderDetails)
            {
                encoder[PAYMENT + "NAME" + count] = o.Item.Name;
                encoder[PAYMENT + "DESC" + count] = "Test Description";
                /*
                encoder[PAYMENT + "DESC" + count] = "Pickup:" + o.PickupDate.ToLongDateString() + 
                    " Return:" + o.DrofoffDate.ToLongDateString();
                 */
                encoder[PAYMENT + "QTY" + count] = o.NumberOfDays.ToString();
                encoder[PAYMENT + "AMT" + count] = o.UnitPrice.ToString();
                count++;
                if (o.SecurityDeposit > 0)
                {
                    encoder[PAYMENT + "NAME" + count] = "Security Deposit - " + o.Item.Name;
                    encoder[PAYMENT + "DESC" + count] = "Security Deposit";
                    /*
                    encoder[PAYMENT + "DESC" + count] = "Pickup:" + o.PickupDate.ToLongDateString() + 
                        " Return:" + o.DrofoffDate.ToLongDateString();
                     */
                    encoder[PAYMENT + "QTY" + count] = "1";
                    encoder[PAYMENT + "AMT" + count] = o.SecurityDeposit.ToString();
                    count++;
                }
            }
            
            
            return encoder;
        }
        /// <summary>
        /// ShortcutExpressCheckout: The method that calls SetExpressCheckout API
        /// </summary>
        /// <param name="amt"></param>
        /// <param ref name="token"></param>
        /// <param ref name="retMsg"></param>
        /// <returns></returns>
        public bool ShortcutExpressCheckout(string amt, NVPCodec encoder, ref string token, ref string retMsg)
        {
            string host = "www.paypal.com";
            if (bSandbox)
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
                host = "www.sandbox.paypal.com";
            }

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                token = decoder["TOKEN"];

                string ECURL = "https://" + host + "/cgi-bin/webscr?cmd=_express-checkout" + "&useraction=commit" + "&token=" + token;

                retMsg = ECURL;
                return true;
            }
            else
            {
                retMsg = "?" + 
                    "ErrorCode=" + ErrorCode.PAYPAL_ERROR + "&" + //decoder["L_ERRORCODE0"] 
                    "Description=" + decoder["L_LONGMESSAGE0"];
                
                return false;
            }
        }

        /// <summary>
        /// MarkExpressCheckout: The method that calls SetExpressCheckout API, invoked from the 
        /// Billing Page EC placement
        /// </summary>
        /// <param name="amt"></param>
        /// <param ref name="token"></param>
        /// <param ref name="retMsg"></param>
        /// <returns></returns>
        public bool MarkExpressCheckout(string amt,
                            string shipToName, string shipToStreet, string shipToStreet2,
                            string shipToCity, string shipToState, string shipToZip,
                            string shipToCountryCode, ref string token, ref string retMsg)
        {
            string host = "www.paypal.com";
            if (bSandbox)
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
                host = "www.sandbox.paypal.com";
            }

            string returnURL = "http://test";
            string cancelURL = "http://test";

            NVPCodec encoder = new NVPCodec();
            encoder["METHOD"] = "SetExpressCheckout";
            encoder["RETURNURL"] = returnURL;
            encoder["CANCELURL"] = cancelURL;
            encoder["AMT"] = amt;
            encoder["PAYMENTACTION"] = "Sale";
            encoder["CURRENCYCODE"] = "CAD";

            //Optional Shipping Address entered on the merchant site
            encoder["SHIPTONAME"] = shipToName;
            encoder["SHIPTOSTREET"] = shipToStreet;
            encoder["SHIPTOSTREET2"] = shipToStreet2;
            encoder["SHIPTOCITY"] = shipToCity;
            encoder["SHIPTOSTATE"] = shipToState;
            encoder["SHIPTOZIP"] = shipToZip;
            encoder["SHIPTOCOUNTRYCODE"] = shipToCountryCode;


            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                token = decoder["TOKEN"];

                string ECURL = "https://" + host + "/cgi-bin/webscr?cmd=_express-checkout" + "&token=" + token;

                retMsg = ECURL;
                return true;
            }
            else
            {
                retMsg = "?" + 
                    "ErrorCode=" + ErrorCode.PAYPAL_ERROR + "&" + //decoder["L_ERRORCODE0"] 
                    "Description=" + decoder["L_LONGMESSAGE0"];

                return false;
            }
        }



        /// <summary>
        /// GetShippingDetails: The method that calls SetExpressCheckout API, invoked from the 
        /// Billing Page EC placement
        /// </summary>
        /// <param name="token"></param>
        /// <param ref name="retMsg"></param>
        /// <returns></returns>
        public bool GetShippingDetails(string token, ref string PayerId, ref string ShippingAddress, ref string retMsg)
        {

            if (bSandbox)
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
            }

            NVPCodec encoder = new NVPCodec();
            encoder["METHOD"] = "GetExpressCheckoutDetails";
            encoder["TOKEN"] = token;

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                ShippingAddress = "<table><tr>";
                ShippingAddress += "<td> First Name </td><td>" + decoder["FIRSTNAME"] + "</td></tr>";
                ShippingAddress += "<td> Last Name </td><td>" + decoder["LASTNAME"] + "</td></tr>";
                ShippingAddress += "<td colspan='2'> Shipping Address</td></tr>";
                ShippingAddress += "<td> Name </td><td>" + decoder["SHIPTONAME"] + "</td></tr>";
                ShippingAddress += "<td> Street1 </td><td>" + decoder["SHIPTOSTREET"] + "</td></tr>";
                ShippingAddress += "<td> Street2 </td><td>" + decoder["SHIPTOSTREET2"] + "</td></tr>";
                ShippingAddress += "<td> City </td><td>" + decoder["SHIPTOCITY"] + "</td></tr>";
                ShippingAddress += "<td> State </td><td>" + decoder["SHIPTOSTATE"] + "</td></tr>";
                ShippingAddress += "<td> Zip </td><td>" + decoder["SHIPTOZIP"] + "</td>";
                ShippingAddress += "</tr>";

                return true;
            }
            else
            {
                retMsg = "?" + 
                    "ErrorCode=" + ErrorCode.PAYPAL_ERROR + "&" + //decoder["L_ERRORCODE0"]
                    "Description=" + decoder["L_LONGMESSAGE0"];

                return false;
            }
        }


        /// <summary>
        /// ConfirmPayment: The method that calls SetExpressCheckout API, invoked from the 
        /// Billing Page EC placement
        /// </summary>
        /// <param name="token"></param>
        /// <param ref name="retMsg"></param>
        /// <returns></returns>
        public bool ConfirmPayment(string finalPaymentAmount, ref NVPCodec decoder, ref string retMsg)
        {
            if (bSandbox)
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
            }

            NVPCodec encoder = new NVPCodec();
            encoder["METHOD"] = "DoExpressCheckoutPayment";
            encoder["TOKEN"] = this.PayPalToken;
            encoder["PAYMENTACTION"] = "Sale";
            encoder["PAYERID"] = this.PayerID;
            encoder["AMT"] = finalPaymentAmount;
            encoder["CURRENCYCODE"] = "CAD";

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                return true;
            }
            else
            {
                retMsg = "?" + 
                    "ErrorCode=" + ErrorCode.PAYPAL_ERROR + "&" + //decoder["L_ERRORCODE0"]
                    "Description=" + decoder["L_LONGMESSAGE0"];

                return false;
            }
        }


        /// <summary>
        /// HttpCall: The main method that is used for all API calls
        /// </summary>
        /// <param name="NvpRequest"></param>
        /// <returns></returns>
        public string HttpCall(string NvpRequest) //CallNvpServer
        {
            string url = pendpointurl;

            //To Add the credentials from the profile
            string strPost = NvpRequest + "&" + buildCredentialsNVPString();
            strPost = strPost + "&BUTTONSOURCE=" + HttpUtility.UrlEncode(BNCode);

            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Timeout = Timeout;
            objRequest.Method = "POST";
            objRequest.ContentLength = strPost.Length;

            try
            {
                using (StreamWriter myWriter = new StreamWriter(objRequest.GetRequestStream()))
                {
                    myWriter.Write(strPost);
                }
            }
            catch (Exception ex)
            {
                /*
                if (log.IsFatalEnabled)
                {
                    log.Fatal(e.Message, this);
                }*/
            }

            //Retrieve the Response returned from the NVP API call to PayPal
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            string result;
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
            }

            //Logging the response of the transaction
            /* if (log.IsInfoEnabled)
             {
                 log.Info("Result :" +
                           " Elapsed Time : " + (DateTime.Now - startDate).Milliseconds + " ms" +
                          result);
             }
             */
            return result;
        }

        /// <summary>
        /// Credentials added to the NVP string
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private string buildCredentialsNVPString()
        {
            NVPCodec codec = new NVPCodec();

            if (!IsEmpty(APIUsername))
                codec["USER"] = APIUsername;

            if (!IsEmpty(APIPassword))
                codec[PWD] = APIPassword;

            if (!IsEmpty(APISignature))
                codec[SIGNATURE] = APISignature;

            if (!IsEmpty(Subject))
                codec["SUBJECT"] = Subject;

            codec["VERSION"] = "63";

            return codec.Encode();
        }

        /// <summary>
        /// Returns if a string is empty or null
        /// </summary>
        /// <param name="s">the string</param>
        /// <returns>true if the string is not null and is not empty or just whitespace</returns>
        public static bool IsEmpty(string s)
        {
            return s == null || s.Trim() == string.Empty;
        }
    }


    public sealed class NVPCodec : NameValueCollection
    {
        private const string AMPERSAND = "&";
        private const string EQUALS = "=";
        private static readonly char[] AMPERSAND_CHAR_ARRAY = AMPERSAND.ToCharArray();
        private static readonly char[] EQUALS_CHAR_ARRAY = EQUALS.ToCharArray();

        /// <summary>
        /// Returns the built NVP string of all name/value pairs in the Hashtable
        /// </summary>
        /// <returns></returns>
        public string Encode()
        {
            StringBuilder sb = new StringBuilder();
            bool firstPair = true;
            foreach (string kv in AllKeys)
            {
                string name = HttpUtility.UrlEncode(kv);
                string value = HttpUtility.UrlEncode(this[kv]);
                if (!firstPair)
                {
                    sb.Append(AMPERSAND);
                }
                sb.Append(name).Append(EQUALS).Append(value);
                firstPair = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decoding the string
        /// </summary>
        /// <param name="nvpstring"></param>
        public void Decode(string nvpstring)
        {
            Clear();
            foreach (string nvp in nvpstring.Split(AMPERSAND_CHAR_ARRAY))
            {
                string[] tokens = nvp.Split(EQUALS_CHAR_ARRAY);
                if (tokens.Length >= 2)
                {
                    string name = HttpUtility.UrlDecode(tokens[0]);
                    string value = HttpUtility.UrlDecode(tokens[1]);
                    Add(name, value);
                }
            }
        }


        #region Array methods
        public void Add(string name, string value, int index)
        {
            this.Add(GetArrayName(index, name), value);
        }

        public void Remove(string arrayName, int index)
        {
            this.Remove(GetArrayName(index, arrayName));
        }

        /// <summary>
        /// 
        /// </summary>
        public string this[string name, int index]
        {
            get
            {
                return this[GetArrayName(index, name)];
            }
            set
            {
                this[GetArrayName(index, name)] = value;
            }
        }

        private static string GetArrayName(int index, string name)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "index can not be negative : " + index);
            }
            return name + index;
        }
        #endregion
    }
}