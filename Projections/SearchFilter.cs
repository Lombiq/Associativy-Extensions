using System;
using System.Linq;
using System.Web.Mvc;
using Associativy.Frontends.Models.Pages.Frontends;
using Associativy.GraphDiscovery;
using Associativy.Models.Services;
using Associativy.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Tokens;
using Piedone.HelpfulLibraries.Utilities;

namespace Associativy.Extensions.Projections
{
    [OrchardFeature("Associativy.Extensions.Projections")]
    public class SearchFilter : Orchard.Projections.Services.IFilterProvider
    {
        private readonly IAssociativyServices _associativyServices;

        public Localizer T { get; set; }


        public SearchFilter(IAssociativyServices associativyServices)
        {
            _associativyServices = associativyServices;

            T = NullLocalizer.Instance;
        }


        public void Describe(DescribeFilterContext describe)
        {
            describe.For("Associativy", T("Associativy"), T("Associativy filters"))
                .Element("AssociativySearchFilter", T("Search filter"), T("Filters for items matched by a search query."),
                    ApplyFilter,
                    DisplayFilter,
                    "AssociativySearchFilter"
                );
        }

        public void ApplyFilter(FilterContext context)
        {
            if (string.IsNullOrEmpty((string)context.State.Labels) || context.State.GraphName == null) return;

            var graphContext = new GraphContext { Name = context.State.GraphName };
            var graph = _associativyServices.GraphManager.FindGraph(graphContext);
            if (graph == null) return;

            var labelsArray = AssociativyFrontendSearchFormPart.LabelsToArray((string)context.State.Labels);
            var nodes = graph.Services.NodeManager.GetByLabelQuery(labelsArray).List();

            if (!nodes.Any())
            {
                // No result
                context.Query.Where(a => a.ContentItem(), p => p.In("Id", new[] { -1 }));
                return;
            }

            var associations = graph.Services.Mind.MakeAssociations(nodes, MindSettings.Default).ToGraph();
            var vertices = associations.Vertices.ToList();
            if (context.State.IncludeSearched == null)
            {
                foreach (var nodeId in nodes.Select(item => item.Id))
                {
                    vertices.Remove(nodeId);
                }
            }

            if (vertices.Count == 0) vertices.Add(-1); // No result if no associations are found

            context.Query.WhereIdIn(vertices);
        }

        public LocalizedString DisplayFilter(FilterContext context)
        {
            return T("Content items matched by the search query \"{0}\" in the graph \"{1}\"", context.State.Labels, context.State.GraphName);
        }
    }

    [OrchardFeature("Associativy.Extensions.Projections")]
    public class SearchFilterForm : IFormProvider
    {
        private readonly dynamic _shapeFactory;
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public SearchFilterForm(IShapeFactory shapeFactory, IGraphManager graphManager)
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
                        Id: "AssociativySearchFilterForm",
                        _GraphName: _shapeFactory.SelectList(
                            Id: "GraphName", Name: "GraphName",
                            Title: T("Graph"),
                            Description: T("Select a graph to fetch the connections from."),
                            Size: 10,
                            Multiple: false
                            ),
                        _Labels: _shapeFactory.Textbox(
                            Id: "associativy-search-filter-labels", Name: "Labels",
                            Title: T("Search terms"),
                            Description: T("Enter labels of Associativy terms here."),
                            Classes: new[] { "text textMedium tokenized" }),
                        _IncludeSearched: _shapeFactory.Checkbox(
                            Id: "associativy-search-filter-include-searched", Name: "IncludeSearched",
                            Title: T("Include searched nodes"),
                            Description: T("If checked, the nodes searched will be included in the result themselves too.")),
                        _SearchForm: _shapeFactory.ProjectorFilterSearchFormDynamics()
                        );

                    foreach (var graph in _graphManager.FindGraphs(GraphContext.Empty))
                    {
                        f._GraphName.Add(new SelectListItem { Value = graph.Name, Text = graph.DisplayName.Text });
                    }


                    return f;
                };

            context.Form("AssociativySearchFilter", form);
        }
    }
}