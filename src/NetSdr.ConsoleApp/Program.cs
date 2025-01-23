using Microsoft.Extensions.Logging;
using NetSdr.Client;
using NetSdr.Client.Exceptions;

namespace NetSdr.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Налаштування логування
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole();
        });
        var logger = loggerFactory.CreateLogger<NetSdrClient>();

        // Створення клієнта
        using var client = new NetSdrClient(logger);

        try
        {
            // Підключення до пристрою
            Console.WriteLine("Connecting to device...");
            await client.ConnectAsync("localhost");
            Console.WriteLine("Connected successfully!");

            // Встановлення частоти
            Console.WriteLine("Setting frequency to 14.1 MHz...");
            await client.SetFrequencyAsync(14_100_000);

            // Запуск передачі IQ даних
            Console.WriteLine("Starting IQ transfer...");
            await client.StartIqTransferAsync();

            Console.WriteLine("Press any key to stop IQ transfer and exit...");
            Console.ReadKey(true);

            // Зупинка передачі IQ даних
            Console.WriteLine("Stopping IQ transfer...");
            await client.StopIqTransferAsync();
        }
        catch (NetSdrClientException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.ControlItem.HasValue)
            {
                Console.WriteLine($"Control Item: {ex.ControlItem}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        finally
        {
            // Відключення від пристрою
            if (client.IsConnected)
            {
                Console.WriteLine("Disconnecting...");
                await client.DisconnectAsync();
            }
        }
    }
}
