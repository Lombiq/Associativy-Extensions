using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Frontends.EngineDiscovery;
using Associativy.GraphDiscovery;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;

namespace Associativy.Extensions.Models
{
    [OrchardFeature("Associativy.Extensions.ContentParts")]
    public class AssociativyGraphLinksPart : ContentPart
    {
        private readonly LazyField<IEnumerable<IGraphDescriptor>> _graphs = new LazyField<IEnumerable<IGraphDescriptor>>();
        internal LazyField<IEnumerable<IGraphDescriptor>> GraphsField { get { return _graphs; } }
        public IEnumerable<IGraphDescriptor> Graphs { get { return _graphs.Value; } }

        private readonly LazyField<IEnumerable<IEngineDescriptor>> _frontendEngines = new LazyField<IEnumerable<IEngineDescriptor>>();
        internal LazyField<IEnumerable<IEngineDescriptor>> FrontendEnginesField { get { return _frontendEngines; } }
        public IEnumerable<IEngineDescriptor> FrontendEngines { get { return _frontendEngines.Value; } }
    }
}