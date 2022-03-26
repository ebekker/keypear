// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using Keypear.CliClient;

var cli = new Cli();

await cli.InitAsync();

return await cli.InvokeAsync(args);
