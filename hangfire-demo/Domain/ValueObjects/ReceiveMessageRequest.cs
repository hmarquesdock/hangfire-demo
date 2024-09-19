namespace hangfire_demo.Domain.ValueObjects
{
    public class ReceiveMessageRequest
    {
        public string QueueUrl { get; set; }
        public int MaxNumberOfMessages { get; set; }
        public int WaitTimeSeconds { get; set; }
    }
}
