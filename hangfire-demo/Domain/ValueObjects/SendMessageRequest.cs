namespace hangfire_demo.Domain.ValueObjects
{
    public class SendMessageRequest
    {
        public string QueueUrl { get; set; }
        public string MessageBody { get; set; }
    }
}
