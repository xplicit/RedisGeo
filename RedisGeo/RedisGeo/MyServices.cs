using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack;
using ServiceStack.Redis;

namespace RedisGeo.ServiceInterface
{
    [Route("/getgeo")]
    public class GetGeo : IReturn<GeoResponse>
    {
        public double Lng { get; set; }

        public double Lat { get; set; }

        public double Radius { get; set; }
    }

    public class GeoResponse
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }

    [Route("/addgeo")]
    public class AddGeo : IReturn<string>
    {
        public double Lng { get; set; }

        public double Lat { get; set; }
    }


    public class MyServices : Service
    {
        const string driverKey = "driver";
        public string user = "user";

        public object Any(AddGeo request)
        {
            using (var client = TryResolve<IRedisClientsManager>().GetClient())
            {
                using (var pipeline = client.CreateTransaction())
                {
                    pipeline.QueueCommand(r => r.AddGeoMember(driverKey, request.Lng, request.Lat, user));
                    var commited = pipeline.Commit();
                    return commited ? 1 : 0;
                }
            }
        }

        public object Any(GetGeo request)
        {
            using (var client = TryResolve<IRedisClientsManager>().GetClient())
            {
                var nativeClient = (RedisNativeClient)client;
                var drivers = nativeClient.GeoRadius(driverKey,
                    request.Lng, request.Lat, request.Radius, RedisGeoUnit.Kilometers, withCoords: true, asc: true
                    );

                Console.WriteLine(drivers[0].Latitude);
                Console.WriteLine(drivers[0].Longitude);

                return new GeoResponse
                {
                    Latitude = drivers[0].Latitude,
                    Longitude = drivers[0].Longitude
                };
            }
        }
    }
}