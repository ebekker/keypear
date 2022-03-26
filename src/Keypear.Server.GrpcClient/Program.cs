// See https://aka.ms/new-console-template for more information
using Keypear.Server.GrpcClient;

Console.WriteLine("Hello, World!");

var address = "https://localhost:5001";

var scb = new ServiceClientBuilder
{
    Address = address
};
var sc = scb.Build();

await sc.CreateAccountAsync(new()
{
    Username = "jdoe@example.com",
});
