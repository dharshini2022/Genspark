using BankingAPI.Interfaces;
using BankingAPI.Models;
using BankingAPI.Models.DTOs;
using BankingAPI.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BankingAPI.Services
{
    public class TransactionService : ITransaction
    {
        private readonly BankingContext _dbContext;

        public TransactionService(BankingContext dbContext)
        {
            _dbContext = dbContext;
        }

        public TransactionResponse Deposit(DepositRequest request)
        {
            var account = _dbContext.Accounts.SingleOrDefault(a => a.AccountNumber == request.ToAccountNumber);
            if (account == null)
                throw new ArgumentException("Destination account not found: " + request.ToAccountNumber);

            using var dbTxn = _dbContext.Database.BeginTransaction();
            try
            {
                // update balance
                account.Balance += request.Amount;
                _dbContext.Accounts.Update(account);
                _dbContext.SaveChanges();

                // create transaction record
                var tx = new Transaction
                {
                    TransactionDate = DateTime.Now,
                    FromAccountNumber = null,
                    ToAccountNumber = account.AccountNumber,
                    Amount = request.Amount,
                    Reason = "Deposit",
                    Status = TransactionStatus.Success
                };

                var created = _dbContext.Transactions.Add(tx);
                _dbContext.SaveChanges();

                dbTxn.Commit();

                return Map(created.Entity);
            }
            catch
            {
                dbTxn.Rollback();
                throw;
            }
        }

        public TransactionResponse Withdraw(WithdrawRequest request)
        {
            var account = _dbContext.Accounts.SingleOrDefault(a => a.AccountNumber == request.FromAccountNumber);
            if (account == null)
                throw new ArgumentException("Source account not found: " + request.FromAccountNumber);

            var amt = (float)request.Amount;
            if (account.Balance < amt)
                throw new InvalidOperationException("Insufficient funds");

            using var dbTxn = _dbContext.Database.BeginTransaction();
            try
            {
                account.Balance -= amt;
                _dbContext.Accounts.Update(account);
                _dbContext.SaveChanges();

                var tx = new Transaction
                {
                    TransactionDate = DateTime.Now,
                    FromAccountNumber = account.AccountNumber,
                    ToAccountNumber = null,
                    Amount = request.Amount,
                    Reason = "Withdraw",
                    Status = TransactionStatus.Success
                };

                var created = _dbContext.Transactions.Add(tx);
                _dbContext.SaveChanges();

                dbTxn.Commit();

                return Map(created.Entity);
            }
            catch
            {
                dbTxn.Rollback();
                throw;
            }
        }

        public TransactionResponse Transfer(TransferRequest request)
        {
            if (request.FromAccountNumber == request.ToAccountNumber)
                throw new ArgumentException("From and To account numbers must differ.");

            var from = _dbContext.Accounts.SingleOrDefault(a => a.AccountNumber == request.FromAccountNumber);
            var to = _dbContext.Accounts.SingleOrDefault(a => a.AccountNumber == request.ToAccountNumber);

            if (from == null)
                throw new ArgumentException("Source account not found: " + request.FromAccountNumber);
            if (to == null)
                throw new ArgumentException("Destination account not found: " + request.ToAccountNumber);

            var amt = (float)request.Amount;
            if (from.Balance < amt)
                throw new InvalidOperationException("Insufficient funds in source account");

            using var dbTxn = _dbContext.Database.BeginTransaction();
            try
            {
                from.Balance -= amt;
                to.Balance += amt;

                _dbContext.Accounts.Update(from);
                _dbContext.Accounts.Update(to);
                _dbContext.SaveChanges();

                // If after transfer source balance is below 1000, rollback entire transaction
                if (from.Balance < 1000f)
                {
                    // create a failed transaction record before rollback (optional) or simply rollback
                    var failedTx = new Transaction
                    {
                        TransactionDate = DateTime.Now,
                        FromAccountNumber = from.AccountNumber,
                        ToAccountNumber = to.AccountNumber,
                        Amount = request.Amount,
                        Reason = "Insufficient Balance",
                        Status = TransactionStatus.Failed
                    };

                    _dbContext.Transactions.Add(failedTx);
                    _dbContext.SaveChanges();

                    dbTxn.Rollback();
                    throw new InvalidOperationException("Transfer would reduce source balance below required minimum of 1000. Transaction rolled back.");
                }

                var tx = new Transaction
                {
                    TransactionDate = DateTime.Now,
                    FromAccountNumber = from.AccountNumber,
                    ToAccountNumber = to.AccountNumber,
                    Amount = request.Amount,
                    Status = TransactionStatus.Success
                };

                var created = _dbContext.Transactions.Add(tx);
                _dbContext.SaveChanges();

                dbTxn.Commit();

                return Map(created.Entity);
            }
            catch
            {
               dbTxn.Rollback();
                throw;
            }
        }

        public IEnumerable<TransactionResponse> GetTransactionsForAccount(FilterRequest filterRequest)
        {
            var transactions = _dbContext.Transactions.Where(t => t.FromAccountNumber == filterRequest.AccountNumber || t.ToAccountNumber == filterRequest.AccountNumber);
            if (filterRequest.FromDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate >= filterRequest.FromDate);
            }
            if (filterRequest.ToDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate <= filterRequest.ToDate);
            }

            if (filterRequest.MinAmount.HasValue)
            {
                transactions = transactions.Where(t => t.Amount >= filterRequest.MinAmount);
            }
            if (filterRequest.MaxAmount.HasValue)
            {
                transactions = transactions.Where(t => t.Amount <= filterRequest.MaxAmount);
            }

            var pagedResult = transactions.Skip((filterRequest.PageNumber - 1 ) * filterRequest.PageSize).Take(filterRequest.PageSize).ToList();

            var orderedResult = pagedResult.OrderByDescending(r => r.TransactionDate);
            return orderedResult.Select(Map).ToList();
        }

        public TransactionResponse? GetTransactionByReference(int referenceNumber)
        {
            var tx = _dbContext.Transactions.Find(referenceNumber);
            if (tx == null) return null;
            return Map(tx);
        }

        private TransactionResponse Map(Transaction t)
        {
            return new TransactionResponse
            {
                TransactionReferenceNumber = t.TransactionReferenceNumber,
                TransactionDate = t.TransactionDate,
                FromAccountNumber = t.FromAccountNumber,
                ToAccountNumber = t.ToAccountNumber,
                Amount = t.Amount,
                Reason = t.Reason,
                Status = t.Status.ToString()
            };
        }
    }
}