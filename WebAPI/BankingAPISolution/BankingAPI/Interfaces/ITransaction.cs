using BankingAPI.Models.DTOs;
using System.Collections.Generic;

namespace BankingAPI.Interfaces
{
    public interface ITransaction
    {
        TransactionResponse Deposit(DepositRequest request);
        TransactionResponse Withdraw(WithdrawRequest request);
        TransactionResponse Transfer(TransferRequest request);

        IEnumerable<TransactionResponse> GetTransactionsForAccount(FilterRequest filterRequest);
        TransactionResponse? GetTransactionByReference(int referenceNumber);
    }
}