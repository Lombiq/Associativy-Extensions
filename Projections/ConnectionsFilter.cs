using System;
using System.Linq;
using System.Web.Mvc;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Tokens;

namespace Associativy.Extensions.Projections
{
    [OrchardFeature("Associativy.Extensions.Projections")]
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
                .Element("AssociativyConnectionsFilter", T("Connections filter"), T("Filters for items connected to an item."),
                    ApplyFilter,
                    DisplayFilter,
                    "AssociativyConnectionsFilter"
                );
        }

        public void ApplyFilter(FilterContext context)
        {
            if (context.State.ItemId == null || context.State.GraphName == null) return;

            var graphContext = new GraphContext { Name = context.State.GraphName };
            var graph = _graphManager.FindGraph(graphContext);
            if (graph == null) return;

            int itemId = int.Parse(_tokenizer.Replace((string)context.State.ItemId, null, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }));
            var neighbourIds = graph.Services.ConnectionManager.GetNeighbourIds(itemId).ToArray();
            context.Query.Where(a => a.ContentItem(), p => p.In("Id", neighbourIds));
        }

        public LocalizedString DisplayFilter(FilterContext context)
        {
            return T("Content items connected to the item with id {0}", context.State.ItemId);
        }
    }

    [OrchardFeature("Associativy.Extensions.Projections")]
    public class ConnectionsFilterForm : IFormProvider
    {
        private readonly dynamic _shapeFactory;
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public ConnectionsFilterForm(IShapeFactory shapeFactory, IGraphManager graphManager)
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
                        Id: "AssociativyConnectionsFilterForm",
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
                        f._GraphName.Add(new SelectListItem { Value = graph.Name, Text = graph.DisplayName.Text });
                    }


                    return f;
                };

            context.Form("AssociativyConnectionsFilter", form);
        }
    }
}