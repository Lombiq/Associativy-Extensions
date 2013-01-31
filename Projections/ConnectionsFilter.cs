using Associativy.GraphDiscovery;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Associativy.Extensions.Projections
{
    [OrchardFeature("Associativy.Extensions.Projector")]
    public class ConnectionsFilter : Orchard.Projections.Services.IFilterProvider
    {
        private readonly ITokenizer _tokenizer;
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public ConnectionsFilter(ITokenizer tokenizer, IGraphManager graphManager)
        {
            _tokenizer = tokenizer;
            _graphManager = graphManager;

            T = NullLocalizer.Instance;
        }


        public void Describe(DescribeFilterContext describe)
        {
            describe.For("Associativy", T("Associativy"), T("Associativy filters"))
                .Element("ConnectionsFilter", T("Connections filter"), T("Filters for items connected to an item."),
                    ApplyFilter,
                    DisplayFilter,
                    "ConnectionsFilter"
                );
        }

        public void ApplyFilter(FilterContext context)
        {
            if (context.State.ItemId == null || context.State.GraphName == null) return;

            var graphContext = new GraphContext { GraphName = context.State.GraphName };
            var graph = _graphManager.FindGraph(graphContext);
            if (graph == null) return;

            int itemId = int.Parse(_tokenizer.Replace(context.State.ItemId, null, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }));
            var neighbourIds = graph.PathServices.ConnectionManager.GetNeighbourIds(graphContext, itemId).ToArray();
            context.Query.Where(a => a.ContentPartRecord<CommonPartRecord>(), p => p.In("Id", neighbourIds));
        }

        public LocalizedString DisplayFilter(FilterContext context)
        {
            return T("Content items connected to the item with id {0}", context.State.ItemId);
        }
    }

    [OrchardFeature("Associativy.Extensions.Projector")]
    public class ContentTypesFilterForms : IFormProvider
    {
        private readonly dynamic _shapeFactory;
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public ContentTypesFilterForms(IShapeFactory shapeFactory, IGraphManager graphManager)
        {
            _shapeFactory = shapeFactory;
            _graphManager = graphManager;

            T = NullLocalizer.Instance;
        }


        public void Describe(Orchard.Forms.Services.DescribeContext context)
        {
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var f = _shapeFactory.Form(
                        Id: "ConnectionsFilterForm",
                        _ItemId: _shapeFactory.Textbox(
                            Id: "ItemId", Name: "ItemId",
                            Title: T("Item Id"),
                            Description: T("The numerical id of the content item whose connected items should be fetched."),
                            Classes: new[] { "tokenized" }),
                        _GraphName: _shapeFactory.SelectList(
                            Id: "GraphName", Name: "GraphName",
                            Title: T("Graph"),
                            Description: T("Select a graph to fetch the connections from."),
                            Size: 10,
                            Multiple: false
                            )
                        );

                    foreach (var graph in _graphManager.FindGraphs(GraphContext.Empty))
                    {
                        f._GraphName.Add(new SelectListItem { Value = graph.GraphName, Text = graph.DisplayGraphName.Text });
                    }


                    return f;
                };

            context.Form("ConnectionsFilter", form);
        }
    }
}