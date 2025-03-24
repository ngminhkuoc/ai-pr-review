var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AiPrReviewer_WebHooks>("aiprreviewer-webhooks");

builder.Build().Run();
