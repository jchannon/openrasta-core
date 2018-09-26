using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Examples;
using Tests.Plugins.Hydra.Implementation;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class collection : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public collection()
    {
      server = new InMemoryHost(() =>
      {
        ResourceSpace.Uses.Hydra(options =>
        {
          options.Vocabulary = "https://schemas.example/schema#";
          options.Utf8Json = true;
        });

        ResourceSpace.Has.ResourcesOfType<List<Event>>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/")
          .HandledBy<EventHandler>();
        
        ResourceSpace.Has.ResourcesOfType<Event>()
          .Vocabulary("https://schemas.example/schema#")
          .AtUri("/events/{id}")
          .HandledBy<EventHandler>();
      });
    }

    [Fact]
    public void content_is_correct()
    {
      body["@type"].ShouldBe("hydra:Collection");
      body["totalItems"].ShouldBe(2);
      body["member"].ShouldBeOfType<JArray>();
      body["member"][0]["@id"].ShouldBe("http://localhost/events/1");
      body["member"][0]["@type"].ShouldBe("Event");
      body["member"][0]["id"].ShouldBe(1);
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/");
    }

    public async Task DisposeAsync() => server.Close();
  }
}

/*

{
  "member": [
    {
      "id": 1,
      "@type": "Event",
      "@id": "http://localhost/events/1"
    },
    {
      "id": 2,
      "@type": "Event",
      "@id": "http://localhost/events/2"
    }
  ],
  "totalItems": 2,
  "@type": "hydra:Collection",
  "@context": "http://localhost/.hydra/context.jsonld"
}




****JSON.NET****

{
  "@context": "http://localhost/.hydra/context.jsonld",
  "@type": "hydra:Collection",
  "member": [
    {
      "@id": "http://localhost/events/1",
      "@type": "Event",
      "id": 1
    },
    {
      "@id": "http://localhost/events/2",
      "@type": "Event",
      "id": 2
    }
  ],
  "totalItems": 2
}

*/