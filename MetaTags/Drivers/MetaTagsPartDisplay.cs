﻿using Etch.OrchardCore.SEO.MetaTags.Models;
using Etch.OrchardCore.SEO.MetaTags.Services;
using Etch.OrchardCore.SEO.MetaTags.ViewModels;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etch.OrchardCore.SEO.MetaTags.Drivers
{
    public class MetaTagsPartDisplay : ContentPartDisplayDriver<MetaTagsPart>
    {
        #region Dependencies

        private readonly IMetaTagsService _metaTagsService;

        #endregion

        #region Constructor

        public MetaTagsPartDisplay(IMetaTagsService metaTagsService)
        {
            _metaTagsService = metaTagsService;
        }

        #endregion

        #region Overrides

        public override IDisplayResult Display(MetaTagsPart metaTagsPart, BuildPartDisplayContext context)
        {
            if (context.DisplayType != "Detail")
            {
                return null;
            }

            _metaTagsService.RegisterDefaults(metaTagsPart);
            _metaTagsService.RegisterOpenGraph(metaTagsPart);
            _metaTagsService.RegisterTwitter(metaTagsPart);

            return Initialize<MetaTagsPartViewModel>("MetaTagsPart", model =>
            {
                model.Title = metaTagsPart.Title;
            })
            .Location("Detail", "Content:1");
        }

        public override IDisplayResult Edit(MetaTagsPart metaTagsPart)
        {
            var itemPaths = metaTagsPart.Images?.ToList().Select(p => new EditMediaFieldItemInfo { Path = p }) ?? new EditMediaFieldItemInfo[] { };

            return Initialize<MetaTagsPartViewModel>("MetaTagsPart_Edit", model =>
            {
                model.Description = metaTagsPart.Description;
                model.Images = JsonConvert.SerializeObject(itemPaths);
                model.Title = metaTagsPart.Title;
                return Task.CompletedTask;
            })
            .Location("Parts#SEO:10");
        }

        public override async Task<IDisplayResult> UpdateAsync(MetaTagsPart part, IUpdateModel updater)
        {
            var model = new MetaTagsPartViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, t => t.Description, t => t.Title, t => t.Images))
            {
                part.Title = model.Title;
                part.Description = model.Description;

                part.Images = string.IsNullOrWhiteSpace(model.Images)
                    ? Array.Empty<string>()
                    : JsonConvert.DeserializeObject<IList<EditMediaFieldItemInfo>>(model.Images).Select(x => x.Path).ToArray();

            }

            return Edit(part);
        }

        #endregion
    }
}