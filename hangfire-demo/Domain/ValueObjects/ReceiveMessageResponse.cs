namespace hangfire_demo.Domain.ValueObjects
{
    public class ReceiveMessageResponse
    {
        public int Id { get; set; }
        public Message Messages { get; set; }
    }

    public class Message
    {
        public string Body { get; set; }

        public int RetryCount { get; set; }
    }
}
