using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Extensions.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Associativy.Extensions.Migrations
{
    [OrchardFeature("Associativy.Extensions.ContentParts")]
    public class ContentPartsMigrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition(typeof(AssociativyGraphLinksPart).Name,
                part => part
                    .Attachable()
                    .WithDescription("Displays a \"View on graph\" link at the item pointing to the search page showing the item in the center with its neighbours.")
                );


            return 1;
        }
    }
}