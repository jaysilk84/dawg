using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

using JaySilk.Dawg.Api.Hubs;

namespace JaySilk.Dawg.Api
{
    public class Startup
    {
        class PointConverter : JsonConverter
        {
            public override void WriteJson(
                JsonWriter writer, object value, JsonSerializer serializer) {
                var point = (Point)value;

                serializer.Serialize(
                    writer, new JObject { { "x", point.X }, { "y", point.Y } });
            }

            public override object ReadJson(
                JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer) {
                var jObject = serializer.Deserialize<JObject>(reader);

                return new Point((int)jObject["x"], (int)jObject["y"]);
            }

            public override bool CanConvert(Type objectType) {
                return objectType == typeof(Point);
            }
        }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddCors(options =>
            {
                options.AddPolicy("ApprovedSites",
                builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST")
                        .AllowCredentials(); ;
                });
            });

            services.AddSignalR().AddNewtonsoftJsonProtocol();

            services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.Converters.Add(new PointConverter()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("ApprovedSites");

            //            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GraphHub>("/graphhub");
            });
        }
    }
}
