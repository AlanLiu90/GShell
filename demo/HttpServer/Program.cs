using System.Threading.Tasks;

namespace HttpServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var url = "http://localhost:12345/";
            var server = new HttpServer(url);
            await server.Run();
        }
    }
}
