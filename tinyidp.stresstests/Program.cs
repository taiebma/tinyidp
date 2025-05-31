// See https://aka.ms/new-console-template for more information
using NBomber.CSharp;
using NBomber.Http.CSharp;

Console.WriteLine("Hello, World!");

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
var httpClient = new HttpClient(handler);

var scenario = Scenario.Create("http_scenario", async context =>
{
    MultipartFormDataContent body = new MultipartFormDataContent();
    body.Add(new StringContent("Test9"), "client_id");
    body.Add(new StringContent("VGVzdDlUZXN0OSE="), "client_secret");
    body.Add(new StringContent("client_credential"), "grant_type");
    body.Add(new StringContent("scope1"), "scope");
    var request =
        Http.CreateRequest("POST", "https://localhost:8083/oauth/token")
            .WithHeader("Accept", "*/*")
            .WithHeader("Content-Type", "multipart/form-data")
            .WithHeader("Authorization", "Basic VGVzdDk6VGVzdDlUZXN0OSE=")
            .WithHeader("User-Agent", "NBomber")
            .WithBody(body);

    var response = await Http.Send(httpClient, request);
    
    return response;
})
.WithLoadSimulations(
    Simulation.Inject(rate: 5,
                        interval: TimeSpan.FromSeconds(1),
                        during: TimeSpan.FromMinutes(1))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();
          