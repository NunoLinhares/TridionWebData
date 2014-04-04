using System;
using System.Collections.Generic;
using Tridion.ContentManager.CommunicationManagement;

namespace Tridion.Templates.Generic.Xml.Data
{
    public class TridionPage : PublishableItem
    {
        public string Url { get; set; }
        public string InformationSource { get; set; }
        public List<SimpleComponentPresentation> ComponentPresentations { get; set; }
        internal Page Page { get; set; }
        public string Id;

        internal void InitializeContent(Page page)
        {
            if (Page != null) return;
            Page = page;
            TcmId = page.Id.ItemId;
            Title = page.Title;
            MajorVersion = page.Version;
            MinorVersion = page.Revision;
            OwningPublication = page.OwningRepository.Id.ItemId;
            CreationDate = page.CreationDate;
            ModificationDate = page.RevisionDate;
            PublicationId = page.ContextRepository.Id.ItemId;
            TemplateId = page.PageTemplate.Id.ItemId;
            OrganizationalItemId = page.OrganizationalItem.Id.ItemId;
            OrganizationalItemTitle = page.OrganizationalItem.Title;
            Url = page.PublishLocationUrl;
            ComponentPresentations = new List<SimpleComponentPresentation>();
            foreach (ComponentPresentation cp in page.ComponentPresentations)
            {
                ComponentPresentations.Add(new SimpleComponentPresentation{ComponentId = cp.Component.Id.ItemId, TemplateId = cp.ComponentTemplate.Id.ItemId});
            }
            if (page.ComponentPresentations.Count > 0)
            {
                Article a = new Article(page.ComponentPresentations[0].Component, Data );
                InformationSource = a.InformationSource.Title;
            }
            PublicationDate = DateTime.Now;
            TemplateModifiedDate = page.PageTemplate.RevisionDate;

        }

    }
}
