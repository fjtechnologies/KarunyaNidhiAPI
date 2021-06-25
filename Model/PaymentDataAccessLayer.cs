using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KarunyaAPI.Model
{
    public class PaymentDataAccessLayer
    {
        private KarunyaNidhiContext Context { get; }
        public PaymentDataAccessLayer(KarunyaNidhiContext ctx)
        {
            Context = ctx;
        }
        public int CreatePaymentTransaction(TransactionModel payTransaction)
        {
            try
            {
                Context.Transactions.Add(payTransaction);
                Context.SaveChanges();
                return 1;
            }
            catch
            {
                throw;
            }
        }
        public IEnumerable<TransactionModel> GetAllPaymentTransaction()
        {
            try
            {
                return Context.Transactions.ToList();
            }
            catch
            {
                throw;
            }
        }

        public TransactionModel GetPaymentTransactionByOrderId(string orderId)
        {
            try
            {
                return Context.Transactions.ToList().Where(x=>x.OrderId==orderId).Select(y=>y).FirstOrDefault();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task UpdatePaymentTransaction(TransactionModel payTransaction)
        {
            if (payTransaction != null)
            {
                Context.Transactions.Update(payTransaction);
                await Context.SaveChangesAsync();
            }
        }
    }
}
