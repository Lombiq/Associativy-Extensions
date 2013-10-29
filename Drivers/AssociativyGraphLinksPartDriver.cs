using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Extensions.Models;
using Associativy.GraphDiscovery;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Associativy.Extensions.Drivers
{
    [OrchardFeature("Associativy.Extensions.ContentParts")]
    public class AssociativyGraphLinksPartDriver : ContentPartDriver<AssociativyGraphLinksPart>
    {
        protected override DriverResult Display(AssociativyGraphLinksPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AssociativyGraphLinks",
                () => shapeHelper.Parts_AssociativyGraphLinks());
        }
    }
}