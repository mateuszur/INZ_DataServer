using DataServerService;

IHost host = Host.CreateDefaultBuilder(args)
      .UseWindowsService(options =>
      {
          options.ServiceName = ".NET DataServer Service";
      })
    .ConfigureServices(services =>
    {
        services.AddHostedService<WindowsBackgroundService>();
        services.AddSingleton<Socket_For_Data_Transmission>();
        
    })
    .Build();

await host.RunAsync();
