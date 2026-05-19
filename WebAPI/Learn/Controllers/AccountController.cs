using Learn.Models;
using Microsoft.AspNetCore.Mvc;
namespace Learn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        static List<Account> accounts = new List<Account>
        {
            new Account{AccountNumber="0009998787",Balance= 100000,OpeninigDate=new DateTime(2026,1,14),Status="Active"},
            new Account{AccountNumber="0009998789",Balance= 100030,OpeninigDate=new DateTime(2026,2,14),Status = "Active"}
        };
        [HttpGet]
        public ActionResult<IEnumerable<Account>> Get()
        {
            if(accounts.Count == 0)
                return NotFound("No Accounts in the bank yet");
            return Ok(accounts);
        }

        [HttpGet("GetAccountByNumebr")]
        public ActionResult<Account> Get(string accountNumber)
        {
            if (accounts.Count == 0)
                return NotFound("No Accounts in the bank yet");
            var account = accounts.SingleOrDefault(a=>a.AccountNumber == accountNumber);
            if (account == null)
                return NotFound("No accont with the given account number");
            return Ok(account);
        }
        [HttpPost]
        public ActionResult<Account> Post([FromBody] Account account)
        {
            accounts.Add(account);
            return Created("https://localhost:5248/api/Account/GetAccountByNumebr?accountNumber="+account.AccountNumber, account);
        }

        [HttpPut]
        public ActionResult<Account> Put(string accountNumber, [FromBody]Account account)
        {
            var exisitingAccount = accounts.SingleOrDefault(a => a.AccountNumber == accountNumber);
            if(exisitingAccount == null)
            {
                return NotFound($"Account not Found with Account Number: {account.AccountNumber}");
            }
            exisitingAccount.Balance = account.Balance;
            exisitingAccount.Status = account.Status;
            //Already the updated obj is with the user so no need to get agian with Ok() as it causes Resource OVerhead.
            return NoContent();
        }

        [HttpDelete]
        public ActionResult<Account> Delete(string accountNumber)
        {
            var exisitingAccount = accounts.SingleOrDefault(a => a.AccountNumber == accountNumber);
            if(exisitingAccount == null)    return NotFound($"Account Not Found with {accountNumber}");
            accounts.Remove(exisitingAccount);
            return NoContent();
        }

        [HttpPatch("UpdateAccountBalance")]
        public ActionResult<Account> Patch(string accountNumber, decimal balance)
        {
            var exisitingAccount = accounts.SingleOrDefault(a => a.AccountNumber == accountNumber);
            if(exisitingAccount == null)    return NotFound($"Account Not Found with {accountNumber}");
            exisitingAccount.Balance = balance;
            return NoContent();
        }
    }
    
}