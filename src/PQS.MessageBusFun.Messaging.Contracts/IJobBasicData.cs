namespace PQS.MessageBusFun.Messaging.Contracts
{
    public interface IJobBasicData
    {
        string GroupId { get; }
        int Index { get; }
        int Count { get; }
        string Path { get; }
    }
}
