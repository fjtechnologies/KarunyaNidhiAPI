using System;
using System.Collections.Generic;

#nullable disable

namespace KarunyaAPI.Model
{
    public partial class TransactionModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int Amount { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string PanNumber { get; set; }
        public string Status { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
