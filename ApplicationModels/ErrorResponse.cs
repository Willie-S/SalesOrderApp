namespace SalesOrderApp.ApplicationModels
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Details { get; set; }
        public string StackTrace { get; set; }
    }
}
