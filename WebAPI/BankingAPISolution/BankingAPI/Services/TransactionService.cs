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

        public async Task<TransactionResponse> Deposit(DepositRequest request)
        {
            var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == request.ToAccountNumber);
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
                await _dbContext.SaveChangesAsync();

                await dbTxn.CommitAsync();

                return Map(created.Entity);
            }
            catch
            {
                await dbTxn.RollbackAsync();
                throw;
            }
        }

        public async Task<TransactionResponse> Withdraw(WithdrawRequest request)
        {
            var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == request.FromAccountNumber);
            if (account == null)
                throw new ArgumentException("Source account not found: " + request.FromAccountNumber);

            var amt = (float)request.Amount;
            if (account.Balance < amt)
                throw new InvalidOperationException("Insufficient funds");

            using var dbTxn = await _dbContext.Database.BeginTransactionAsync();
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
                await _dbContext.SaveChangesAsync();

                await dbTxn.CommitAsync();

                return Map(created.Entity);
            }
            catch
            {
                await dbTxn.RollbackAsync();
                throw;
            }
        }

        public async Task<TransactionResponse> Transfer(TransferRequest request)
        {
            if (request.FromAccountNumber == request.ToAccountNumber)
                throw new ArgumentException("From and To account numbers must differ.");

            var from = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == request.FromAccountNumber);
            var to = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == request.ToAccountNumber);

            if (from == null)
                throw new ArgumentException("Source account not found: " + request.FromAccountNumber);
            if (to == null)
                throw new ArgumentException("Destination account not found: " + request.ToAccountNumber);

            var amt = (float)request.Amount;
            if (from.Balance < amt)
                throw new InvalidOperationException("Insufficient funds in source account");

            using var dbTxn =await _dbContext.Database.BeginTransactionAsync();
            try
            {
                from.Balance -= amt;
                to.Balance += amt;

                _dbContext.Accounts.Update(from);
                _dbContext.Accounts.Update(to);
                await _dbContext.SaveChangesAsync();

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
                    await _dbContext.SaveChangesAsync();

                    await dbTxn.RollbackAsync();
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
                await _dbContext.SaveChangesAsync();

                await dbTxn.CommitAsync();

                return Map(created.Entity);
            }
            catch
            {
               await dbTxn.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TransactionResponse>> GetTransactionsForAccount(FilterRequest filterRequest)
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

            var pagedResult = await transactions.Skip((filterRequest.PageNumber - 1 ) * filterRequest.PageSize).Take(filterRequest.PageSize).ToListAsync();

            var orderedResult = pagedResult.OrderByDescending(r => r.TransactionDate);
            return orderedResult.Select(Map).ToList();
        }

        public async Task<TransactionResponse?> GetTransactionByReference(int referenceNumber)
        {
            var tx = await _dbContext.Transactions.FindAsync(referenceNumber);
            if (tx == null) return null;
            return Map(tx);
        }

        private TransactionResponse Map(Transaction t)
        {
            return new TransactionResponse
            {
                TransactionReferenceNumber = t.TransactionReferenceNumber,
                TransactionDate = t.TransactionDate,
                FromAccountNumber = t.FromAccountNumber!,
                ToAccountNumber = t.ToAccountNumber!,
                Amount = t.Amount,
                Reason = t.Reason!,
                Status = t.Status.ToString()
            };
        }
    }
}