using Telegram.Bot;

namespace teko_bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient("5446428708:AAEOFR_1BHWh0G6W8yCNzDgJkU7tSfBgS3g");

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }
    }
}