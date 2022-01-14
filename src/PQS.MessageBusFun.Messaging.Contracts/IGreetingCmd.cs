namespace PQS.MessageBusFun.Messaging.Contracts
{
    public interface IGreetingCmd
    {
        string Name { get; set; }
    }

    public class GreetingCmdResponse
    {
        public string Saludo { get; set; }
    }
}
