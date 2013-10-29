using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Frontends.EngineDiscovery;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Associativy.Extensions.Settings
{
    [OrchardFeature("Associativy.Extensions.ContentParts")]
    public class AssociativyGraphLinksTypePartSettings
    {
        public string FrontendEngineName { get; set; }
        public IEnumerable<IEngineDescriptor> AllFrontendEngines { get; set; }
    }


    [OrchardFeature("Associativy.Extensions.ContentParts")]
    public class AssociativyGraphLinksTypePartSettingsHooks : ContentDefinitionEditorEventsBase
    {
        private readonly IEngineManager _engineManager;


        public AssociativyGraphLinksTypePartSettingsHooks(IEngineManager engineManager)
        {
            _engineManager = engineManager;
        }
	
			
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            if (definition.PartDefinition.Name != "AssociativyGraphLinksPart")
                yield break;

            var settings = definition.Settings.GetModel<AssociativyGraphLinksTypePartSettings>();
            settings.AllFrontendEngines = _engineManager.GetEngines();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            if (builder.Name != "AssociativyGraphLinksPart")
                yield break;

            var model = new AssociativyGraphLinksTypePartSettings();
            updateModel.TryUpdateModel(model, "AssociativyGraphLinksTypePartSettings", null, null);
            builder.WithSetting("AssociativyGraphLinksTypePartSettings.FrontendEngineName", model.FrontendEngineName);

            yield return DefinitionTemplate(model);
        }
    }
}