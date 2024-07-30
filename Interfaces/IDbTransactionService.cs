using Microsoft.AspNetCore.Mvc;

namespace SalesOrderApp.Interfaces
{
    public interface IDbTransactionService
    {
        Task<IActionResult> ExecuteInTransactionAsync(Func<Task<IActionResult>> action);
    }
}
