using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Extensions.Models;
using Associativy.Frontends.EngineDiscovery;
using Associativy.GraphDiscovery;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment;
using Orchard.Environment.Extensions;

namespace Associativy.Extensions.Handlers
{
    [OrchardFeature("Associativy.Extensions.ContentParts")]
    public class AssociativyGraphLinksPartHandler : ContentHandler
    {
        public AssociativyGraphLinksPartHandler(Work<IGraphManager> graphManagerWork, Work<IEngineManager> engineManagerWork)
        {
            OnActivated<AssociativyGraphLinksPart>((context, part) =>
            {
                part.GraphsField.Loader(() => graphManagerWork.Value.FindGraphs(new GraphContext { ContentTypes = new [] { context.ContentType } }));
                part.FrontendEnginesField.Loader(() => engineManagerWork.Value.GetEngines());
            });
        }
    }
}