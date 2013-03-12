using Associativy.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Tokens;

namespace Associativy.Extensions.Tokens
{
    [OrchardFeature("Associativy.Extensions.Tokens")]
    public class NodeLabelTokens : ITokenProvider
    {
        public Localizer T { get; set; }


        public NodeLabelTokens()
        {
            T = NullLocalizer.Instance;
        }


        public void Describe(DescribeContext context)
        {
            context.For("Content", T("Content Items"), T("Content Items"))
                .Token("NodeLabel", T("Node label"), T("Node label if the item has an IAssociativyNodeLabelAspect implementation attached."));
        }

        public void Evaluate(EvaluateContext context)
        {
            context.For<IContent>("Content")
                .Token("NodeLabel", content =>
                {
                    var labelAspect = content.As<IAssociativyNodeLabelAspect>();
                    if (labelAspect == null) return string.Empty;
                    return labelAspect.Label;
                });
        }
    }
}