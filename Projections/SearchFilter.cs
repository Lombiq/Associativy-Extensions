﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Associativy.Frontends.Models.Pages.Frontends;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Tokens;

namespace Associativy.Extensions.Projections
{
    [OrchardFeature("Associativy.Extensions.Projections")]
    public class SearchFilter : Orchard.Projections.Services.IFilterProvider
    {
        private readonly ITokenizer _tokenizer;
        private readonly IAssociativyServices _associativyServices;

        public Localizer T { get; set; }


        public SearchFilter(ITokenizer tokenizer, IAssociativyServices associativyServices)
        {
            _tokenizer = tokenizer;
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
            if (context.State.Labels == null || context.State.GraphName == null) return;

            var graphContext = new GraphContext { GraphName = context.State.GraphName };
            var graph = _associativyServices.GraphManager.FindGraph(graphContext);
            if (graph == null) return;

            string labels = _tokenizer.Replace(context.State.Labels, null, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            var nodes = _associativyServices.NodeManager.GetManyByLabelQuery(graphContext, (IEnumerable<string>)context.State.Labels).List();
            var associations = _associativyServices.Mind.MakeAssociations(graphContext, nodes);
            context.Query.Where(a => a.ContentPartRecord<CommonPartRecord>(), p => p.In("Id", associations.Vertices.Select(content => content.ContentItem.Id).ToArray()));
        }

        public LocalizedString DisplayFilter(FilterContext context)
        {
            if (context.State.Labels == null) context.State.Labels = Enumerable.Empty<string>();
            return T("Content items matched by the search query \"{0}\" in the graph \"{1}\"", string.Join(", ", context.State.Labels), context.State.GraphName);
        }
    }

    [OrchardFeature("Associativy.Extensions.Projections")]
    public class SearchFilterForm : IFormProvider
    {
        private readonly dynamic _shapeFactory;
        private readonly IContentManager _contentManager;
        private readonly IGraphManager _graphManager;

        public Localizer T { get; set; }


        public SearchFilterForm(IShapeFactory shapeFactory, IContentManager contentManager, IGraphManager graphManager)
        {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            _graphManager = graphManager;

            T = NullLocalizer.Instance;
        }


        public void Describe(Orchard.Forms.Services.DescribeContext context)
        {
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var page = _contentManager.New("AssociativyProjectorSearchFilter");
                    page.Weld(new AssociativyFrontendSearchFormPart());
                    

                    var f = _shapeFactory.Form(
                        Id: "AssociativySearchFilterForm",
                        _GraphName: _shapeFactory.SelectList(
                            Id: "GraphName", Name: "GraphName",
                            Title: T("Graph"),
                            Description: T("Select a graph to fetch the connections from."),
                            Size: 10,
                            Multiple: false
                            ),
                        _SearchForm: _contentManager.BuildDisplay(page)
                        );

                    foreach (var graph in _graphManager.FindGraphs(GraphContext.Empty))
                    {
                        f._GraphName.Add(new SelectListItem { Value = graph.GraphName, Text = graph.DisplayGraphName.Text });
                    }


                    return f;
                };

            context.Form("AssociativySearchFilter", form);
        }
    }
}