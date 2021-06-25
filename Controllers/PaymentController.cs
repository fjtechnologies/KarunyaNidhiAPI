using KarunyaAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KarunyaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private RazorpayClient _razorpayClient;
        private PaymentDataAccessLayer _paymentDataLayer;
        public PaymentController(KarunyaNidhiContext ctx, IOptions<RazorModel> razorSettings)
        {           
            _razorpayClient = new RazorpayClient(razorSettings.Value.Key, razorSettings.Value.Secret);
            _paymentDataLayer = new PaymentDataAccessLayer(ctx);
        }
       
        [HttpPost]
        [Route("initialize")]
        public async Task<IActionResult> InitializePayment([FromForm] TransactionModel data)
        {
            if (data != null)
            {
                if (data.Amount < 0)
                    return Ok("Donation failed-Invalid Amount");
                if (data.EmailId != null && data.FirstName != null && data.PhoneNumber != null)
                {
                    var options = new Dictionary<string, object>
                    {
                        { "amount", data.Amount*100 },
                        { "currency", "INR" },
                        { "receipt", RandomString(8) },
                        // auto capture payments rather than manual capture
                        // razor pay recommended option
                        { "payment_capture", true }
                    };

                    var order = _razorpayClient.Order.Create(options);
                    var orderId = order["id"].ToString();
                    if (orderId != null)
                    {
                        //creating Transaction Entry
                        data.OrderId = orderId;
                        data.Status = "Payment Initiated";
                        _paymentDataLayer.CreatePaymentTransaction(data);
                        var orderJson = order.Attributes.ToString();
                        return Ok(orderJson);
                    }
                    else
                    {
                        data.Status = "Payment not Initiated";
                        _paymentDataLayer.CreatePaymentTransaction(data);
                        return Ok("Donation failed-Order Id not Generated");
                    }

                }
                else
                {
                    return Ok("Donation failed-Invalid email,Phone,Firstname");
                }
            }
            else
            {
                return Ok("Donation failed");
            }
           
        }       

        [HttpPost]
        [Route("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody]ConfirmPaymentPayload confirmPayment)
        {
            var attributes = new Dictionary<string, string>
        {
            { "razorpay_payment_id", confirmPayment.razorpay_payment_id },
            { "razorpay_order_id", confirmPayment.razorpay_order_id },
            { "razorpay_signature", confirmPayment.razorpay_signature }
        };
            try
            {
                Utils.verifyPaymentSignature(attributes);
                // OR
                //var isValid = Utils.ValidatePaymentSignature(attributes);
                //if (isValid)
                //{
                var order = _razorpayClient.Order.Fetch(confirmPayment.razorpay_order_id);
                var payment = _razorpayClient.Payment.Fetch(confirmPayment.razorpay_payment_id);
                if (payment["status"] == "captured")
                {
                    var paymentDetails = _paymentDataLayer.GetPaymentTransactionByOrderId(confirmPayment.razorpay_order_id);
                    paymentDetails.PaymentId = confirmPayment.razorpay_payment_id;
                    await _paymentDataLayer.UpdatePaymentTransaction(paymentDetails);
                    return Ok("Payment Successful");
                }
                else
                {
                    return Ok("Payment Failed");
                }
                //}
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet]
        [Route("gettransaction")]
        public IEnumerable<TransactionModel> GetTransaction()
        {
            var paymentDetails = _paymentDataLayer.GetAllPaymentTransaction();
            return paymentDetails;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public class ConfirmPaymentPayload
        {
            public string razorpay_payment_id { get; set; }
            public string razorpay_order_id { get; set; }
            public string razorpay_signature { get; set; }
        }
    }
}