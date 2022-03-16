// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.ClientShared;

namespace Keypear.CliClient;

class Test
{
    public static void OldMain(string[] args)
    {
        //var dbPasswd = "95$cHY76d#D8";
        //var dbServer = "db.agsjyohfeddzbznajrgz.supabase.co";
        //var connstr = $"User Id=postgres;Password={dbPasswd};Server={dbServer};Port=5432;Database=postgres";

        //var sbClient = await Supabase.Client.InitializeAsync(
        //    "https://agsjyohfeddzbznajrgz.supabase.co",
        //    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFnc2p5b2hmZWRkemJ6bmFqcmd6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE2NDUxODg4MjMsImV4cCI6MTk2MDc2NDgyM30.mEjK4OBjUwN8y7W2lV5JM6iRXdD1iKYc4Yaa2kSApUM");

        //var instance = Supabase.Client.Instance;
        //if (instance == null)
        //{
        //    throw new Exception("failed to get Supabase client instance");
        //}

        //var accounts = await instance.From<Account>().Get();

        //Console.WriteLine("Accounts:");
        //foreach (var a in accounts.Models)
        //{
        //    Console.WriteLine($"Account: {a.Email}");
        //}

        //var sdtResponse = await instance.Rpc("GetSystemDateTime", null);
        //var sdt = JsonSerializer.Deserialize<DateTime>(sdtResponse.Content);
        //Console.WriteLine(sdt);

        string email = "jdoe@example.com";
        string mpass = "ThisIsMySecretPassword";

        var mkeyIters = 10000;
        var mkeyAlgor = HashAlgorithmName.SHA256;
        var mkeyLength = 32;

        var emailHash = SHA256.HashData(Encoding.UTF8.GetBytes(email));
        var masterKey = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(mpass), emailHash, mkeyIters, mkeyAlgor, mkeyLength);



        //Console.WriteLine(masterKey.Length);
        //Console.WriteLine(Convert.ToBase64String(masterKey));

    }
}
