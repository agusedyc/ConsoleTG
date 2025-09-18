// using System;
// using System.Net.Http;
using System.Globalization;
using System.Text.Json;
// using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

class Program
{
    private static readonly string BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? throw new Exception("BOT_TOKEN is not set");
    private static readonly string ChatId = Environment.GetEnvironmentVariable("CHAT_ID") ?? throw new Exception("CHAT_ID is not set");

    private static readonly string ApiUrl = $"https://api.myquran.com/v2/sholat/jadwal/1212/{DateTime.Now:yyyy/MM/dd}"; // 1212 adalah ID kota Semarang


    static async Task Main()
    {
        var botClient = new TelegramBotClient(BotToken);

        Console.WriteLine("Bot aktif, mengambil jadwal sholat untuk Semarang...");

        DateTime sekarang = DateTime.Now;
        var culture = new CultureInfo("id-ID");
        string date_string = sekarang.ToString("dddd, dd MMMM yyyy 'Pukul' HH:mm 'WIB'", culture);

        string ipAddress = GetLocalIPAddress();

        var jadwal = await GetJadwalSholat();
        if (jadwal != null)
        {
            string message =
                             $"*{date_string}*\n\n" +
                             $"*This Is Your IP Address : {ipAddress}*\n\n" +
                             $"🕌 *Jadwal Sholat Semarang*\n\n" +
                             $"Hari: {jadwal.Date}\n\n" +
                             $"Fajr (Subuh): {jadwal.Subuh}\n" +
                             $"Dhuhr (Dzuhur): {jadwal.Dzuhur}\n" +
                             $"Asr (Ashar): {jadwal.Ashar}\n" +
                             $"Maghrib: {jadwal.Maghrib}\n" +
                             $"Isha (Isya): {jadwal.Isya}";

            await botClient.SendMessage(ChatId, message, ParseMode.Markdown);
            Console.WriteLine("Pesan terkirim ke Telegram.");
        }
        else
        {
            Console.WriteLine("Gagal mengambil jadwal sholat.");
        }
    }

    private static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        return ip?.ToString() ?? "IP Tidak Ditemukan";
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private static async Task<JadwalSholat?> GetJadwalSholat()
    {
        using HttpClient client = new HttpClient();
        try
        {
            var response = await client.GetStringAsync(ApiUrl);
            using JsonDocument doc = JsonDocument.Parse(response);
            var jadwal = doc.RootElement.GetProperty("data").GetProperty("jadwal");

            return new JadwalSholat
            {
                Date = jadwal.GetProperty("tanggal").GetString(),
                Subuh = jadwal.GetProperty("subuh").GetString(),
                Dzuhur = jadwal.GetProperty("dzuhur").GetString(),
                Ashar = jadwal.GetProperty("ashar").GetString(),
                Maghrib = jadwal.GetProperty("maghrib").GetString(),
                Isya = jadwal.GetProperty("isya").GetString()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    class JadwalSholat
    {
        public string? Date { get; set; }
        public string? Subuh { get; set; }
        public string? Dzuhur { get; set; }
        public string? Ashar { get; set; }
        public string? Maghrib { get; set; }
        public string? Isya { get; set; }
    }
}
