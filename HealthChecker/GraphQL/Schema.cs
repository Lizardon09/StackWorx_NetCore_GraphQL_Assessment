using System;
using System.Collections.Generic;
using System.Configuration;
using GraphQL.Types;
using HealthCheckerHelper.Infrastructure.Models;
using HealthCheckerHelper.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HealthChecker.GraphQL
{

    public class Server
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HealthCheckUri { get; set; }
    }

    public class ServerErrorType : ObjectGraphType<ServerError>
    {
        public ServerErrorType()
        {
            Field(se => se.Status, type: typeof(StringGraphType));
            Field(se => se.Body);
        }
    }

    public class ServerType : ObjectGraphType<Server>
    {
        public ServerType(IHealthCheckerHelperService healthCheckerHelperService)
        {
            Name = "Server";
            Description = "A server to monitor";

            Field(h => h.Id);
            Field(h => h.Name);
            Field(h => h.HealthCheckUri);

            //Field<StringGraphType>(
            //    "status",
            //    // TODO: replace with health check code
            //    resolve: context => "OFFLINE"
            //);

            Field<StringGraphType>(
                "status",
                // TODO: replace with health check code
                resolve: context => {
                    var temp = healthCheckerHelperService.GetCachedServerStatus(context.Source.HealthCheckUri);
                    return temp?.Status;
                }
            );
            Field<ServerErrorType>(
                "error",
                resolve: context => {
                    var temp = healthCheckerHelperService.GetCachedServerStatus(context.Source.HealthCheckUri);
                    return temp?.Error;
                }
            );
            Field<StringGraphType>(
                "lastTimeUp",
                resolve: context => {
                    var temp = healthCheckerHelperService.GetCachedServerStatus(context.Source.HealthCheckUri);
                    return temp?.LastTimeUp;
                }
            );
        }
    }

    public class HealthCheckerQuery : ObjectGraphType<object>
    {
        private List<Server> servers = new List<Server>{
            new Server{
                Id = "1",
                Name = "stackworx.io",
                HealthCheckUri = "https://www.stackworx.io",
            },
            new Server{
                Id = "2",
                Name = "prima.run",
                HealthCheckUri = "https://prima.run",
            },
            new Server{
                Id = "3",
                Name = "google",
                HealthCheckUri = "https://www.google.com",
            },
        };

        public HealthCheckerQuery(IHealthCheckerHelperService healthCheckerHelperService, IConfiguration config)
        {
            Name = "Query";


            Func<ResolveFieldContext, string, object> serverResolver = (context, id) => this.servers;

            //Gets intervale for server status checking from app settings
            var seconds = config.GetValue<int>("ServerStatusCheckIntervalSeconds");

            //Starts async checking of server statuses
            healthCheckerHelperService.StartContinuousCheckingServers(this.servers.ConvertAll(server => server.HealthCheckUri), seconds);

            FieldDelegate<ListGraphType<ServerType>>(
                "servers",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "id", Description = "id of server" }
                ),
                resolve: serverResolver
            );

            Field<StringGraphType>(
                "hello",
                resolve: context => "world"
            );
        }
    }

    public class HealthCheckerSchema : Schema
    {
        public HealthCheckerSchema(IServiceProvider provider, IHealthCheckerHelperService healthCheckerHelperService, IConfiguration config) : base(provider)
        {
            Query = new HealthCheckerQuery(healthCheckerHelperService, config);
        }
    }
}
