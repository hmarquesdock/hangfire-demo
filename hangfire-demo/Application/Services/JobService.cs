using Hangfire.Console;
using Hangfire.Server;
using hangfire_demo.Domain.Interfaces.Services;
using hangfire_demo.Domain.ValueObjects;

namespace hangfire_demo.Application.Services
{
    public class JobService : IJobService
    {

        private const string QueueUrl = "SQS_QUEUE_URL";
        private const int MaxRetryCount = 3;
        public async Task ProcessJobAsync(PerformContext context)
        {
            await Task.Delay(10000);

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = QueueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 10
            };

            var messages = sqsQeue.ToList();
            var bar = context.WriteProgressBar();

            foreach (var message in messages.WithProgress(bar))
            {

                await Task.Delay(10000);

                if (message.Messages.RetryCount > MaxRetryCount)
                    sqsQeue.Remove(message);
                else
                {
                    try
                    {
                        if (message.Id == 2)
                            throw new Exception();
                        // Retry the service 
                        await RetryServicePanUpdate(message.Messages, context);
                        // If successful, delete the message from SQS
                        sqsQeue.Remove(message);
                    }
                    catch (Exception ex)
                    {
                        context.SetTextColor(ConsoleTextColor.Red);
                        context.WriteLine($"Error! {message.Messages.Body}");
                        var update = sqsQeue.FirstOrDefault(x => x.Id == message.Id);
                        update.Messages.RetryCount++;
                        context.ResetTextColor();
                        throw new Exception("Exemple of error on server!");
                    }
                }
            }

            context.WriteLine($"Job completed!");
        }

        private static async Task RetryServicePanUpdate(Message message, PerformContext context)
        {
            await Task.Delay(5000);
            context.WriteLine("Reprocessando....");
            context.WriteLine($"{message.Body}");
        }


        private readonly static List<ReceiveMessageResponse> sqsQeue =
        [
            new ReceiveMessageResponse
            {
                Id = 1,
                Messages = new Message
                {
                    Body = "Test 1",
                    RetryCount = 0
                }
            },

            new ReceiveMessageResponse
            {
                Id = 2,
                Messages = new Message
                {
                    Body = "Test 2",
                    RetryCount = 0
                }
            },

            new ReceiveMessageResponse
            {
                Id = 3,
                Messages = new Message
                {
                    Body = "Test 3",
                    RetryCount = 0
                }
            }
        ];
    }
}
