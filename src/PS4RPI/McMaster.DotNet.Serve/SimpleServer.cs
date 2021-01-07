// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//Code adapted from https://github.com/natemcmaster/dotnet-serve

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace McMaster.DotNet.Serve
{
    class SimpleServer
    {
        private int? port;
        private IPAddress[] addresses;
        private IWebHost host;
        private string directory;

        public SimpleServer(IPAddress[] addresses, int? port, string folder)
        {
            this.addresses = addresses;
            this.port = port;
            this.directory = folder;
        }


        public ValueTask Start(CancellationToken cancellationToken)
        {
            if (host != null)
                return new ValueTask();

            host = new WebHostBuilder()
            .ConfigureLogging(l =>
            {
                l.SetMinimumLevel(LogLevel.None);
                l.AddConsole();
            })
            .PreferHostingUrls(false)
            .UseKestrel(o =>
            {
                foreach (var a in addresses)
                    o.Listen(a, port.GetValueOrDefault());
            })
            .UseWebRoot(directory)
            .UseContentRoot(directory)
            .UseEnvironment("Production")
            .SuppressStatusMessages(true)
            .UseStartup<Startup>()
            .Build();

            return new ValueTask(host.StartAsync(cancellationToken));
        }
        public ValueTask Stop(CancellationToken cancellationToken)
        {
            if (host == null)
                return new ValueTask();

            var t = host.StopAsync(cancellationToken);
            host = null;
            return new ValueTask(t);
        }
    }
}
