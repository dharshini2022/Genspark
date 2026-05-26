using BankingAPI.Models.DTOs;
using System.Collections.Generic;

namespace BankingAPI.Interfaces
{
    public interface ITransaction
    {
        public Task<TransactionResponse> Deposit(DepositRequest request);
        public Task<TransactionResponse> Withdraw(WithdrawRequest request);
        public Task<TransactionResponse> Transfer(TransferRequest request);

        public Task<IEnumerable<TransactionResponse>> GetTransactionsForAccount(FilterRequest filterRequest);
        public Task<TransactionResponse?> GetTransactionByReference(int referenceNumber);
    }
}