using System;
using System.Collections.Generic;
using System.Net;
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
            Field<IntGraphType>(
                "status",
                resolve : context =>
                {
                    var temp = (int)(HttpStatusCode)context.Source.Status;
                    return temp;
                }
            );
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

            /*
             * All three fields will pull from the cache of the helper service where it is consitently
             * checking the servers it needs to
             */

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

            //Gets intervale for server status checking from app settings
            var seconds = config.GetValue<int>("ServerStatusCheckIntervalSeconds");

            //Starts async checking of server statuses
            healthCheckerHelperService.StartContinuousCheckingServers(this.servers.ConvertAll(server => server.HealthCheckUri), seconds);

            //Resolver for getting current servers (With filters)
            Func<ResolveFieldContext, string, object, object> serverResolver = (context, id, name) => {
                if (id!=null) return this.servers.FindAll(s => s.Id.Equals(id));
                if (name!=null) return this.servers.FindAll(s => s.Name.Equals(name));
                return this.servers;
            };

            //Resolver for mutation to stop checking server
            Func<ResolveFieldContext, string, object> stopCheckingServerResolver = (context, servername) => {

                var server = this.servers.Find(s => s.Name.Equals(servername));
                if (server != null) healthCheckerHelperService.StopCheckingServer(server.HealthCheckUri);
                return this.servers;
            };

            //Resolver for mutation to start checking server
            Func<ResolveFieldContext, string, object> startCheckingServerResolver = (context, servername) => {
                var server = this.servers.Find(s => s.Name.Equals(servername));
                if (server != null) healthCheckerHelperService.StartCheckingServer(server.HealthCheckUri, seconds);
                return this.servers;
            };

            //Added server name filter
            FieldDelegate<ListGraphType<ServerType>>(
                "servers",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "id", Description = "id of server" },
                    new QueryArgument<StringGraphType> { Name = "name" }
                ),
                resolve: serverResolver
            );

            //Mutation to stop the checking of a sever
            FieldDelegate<ListGraphType<ServerType>>(
                "stopCheckingServer",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "servername" }
                ),
                resolve: stopCheckingServerResolver
            );

            //Mutation to start the checking of a sever (from the server list)
            FieldDelegate<ListGraphType<ServerType>>(
                "startCheckingServer",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "servername" }
                ),
                resolve: startCheckingServerResolver
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
