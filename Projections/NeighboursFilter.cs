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
    public class NeighboursFilter : Orchard.Projections.Services.IFilterProvider
    {
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public NeighboursFilter(IGraphManager graphManager)
        {
            _graphManager = graphManager;

            T = NullLocalizer.Instance;
        }


        public void Describe(DescribeFilterContext describe)
        {
            describe.For("Associativy", T("Associativy"), T("Associativy filters"))
                .Element("AssociativyNeighboursFilter", T("Neighbours filter"), T("Filters for items connected to an item."),
                    ApplyFilter,
                    DisplayFilter,
                    "AssociativyNeighboursFilter"
                );
        }

        public void ApplyFilter(FilterContext context)
        {
            if (string.IsNullOrEmpty((string)context.State.ItemId) || context.State.GraphName == null) return;

            var graphContext = new GraphContext { Name = context.State.GraphName };
            var graph = _graphManager.FindGraph(graphContext);
            if (graph == null) return;

            var neighbourIds = graph.Services.ConnectionManager.GetNeighbourIds(int.Parse((string)context.State.ItemId)).ToArray();

            if (neighbourIds.Length == 0) neighbourIds = new[] { -1 }; // No result if no neighbours are found

            context.Query.Where(a => a.ContentItem(), p => p.In("Id", neighbourIds));
        }

        public LocalizedString DisplayFilter(FilterContext context)
        {
            return T("Content items connected to the item with id {0}", context.State.ItemId);
        }
    }

    [OrchardFeature("Associativy.Extensions.Projections")]
    public class NeighboursFilterForm : IFormProvider
    {
        private readonly dynamic _shapeFactory;
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public NeighboursFilterForm(IShapeFactory shapeFactory, IGraphManager graphManager)
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
                        Id: "AssociativyNeighboursFilterForm",
                        _ItemId: _shapeFactory.Textbox(
                            Id: "ItemId", Name: "ItemId",
                            Title: T("Item Id"),
                            Description: T("The numerical id of the content item whose connected items should be fetched."),
                            Classes: new[] { "tokenized textMedium" }),
                        _GraphName: _shapeFactory.SelectList(
                            Id: "GraphName", Name: "GraphName",
                            Title: T("Graph"),
                            Description: T("Select a graph to fetch the neighbours from."),
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

            context.Form("AssociativyNeighboursFilter", form);
        }
    }
}