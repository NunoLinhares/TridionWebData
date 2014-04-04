/*************************************************************************************
 * TridionOutputHandler.cs
 * Author: Nuno Linhares (nuno.linhares@sdltridion.com)
 * Change Log: Version 1.0 
 * Change Date: 09/24/2008
 * Change Author: Nuno Linhares
 * Change: Fixed GetFieldValue to account for index differences between values.count 
 * (1-based) and value position (0-based)
 * ***********************************************************************************
 * Change Date: 09/25/2008
 * Change Author: Nuno Linhares
 * Change: Modified FindItemFieldXPathAndValue to handle Component Link Fields and 
 * Multimedia Link Fields in one go, avoiding duplication of code
 * Comment: Seems like a MultimediaLinkField is also a ComponentLinkField, some of that
 * code might be redundant... Better safe than sorry anyway.
 * ***********************************************************************************
 * Change Date: 09/25/2008
 * Change Author: Nuno Linhares
 * Change: Added public property SiteEditFormatString
 *************************************************************************************
 * Change Date: 09/29/2008
 * Change Author: Philippe CONIL
 * Change: added DateField type
 *************************************************************************************
 * Change Date: 09/30/2008
 * Change Author: Philippe CONIL
 * Change: changed Date format: ToString("yyyy-MM-ddTHH:mm:ss"));
 *************************************************************************************
 * Change Date: 02/10/2008
 * Change Author: Nuno Linhares
 * Change: FindItemFieldXPathAndValue() added check if null on fields parameter
 *************************************************************************************
 * Change Date: 10/14/2008
 * Change Author: Nuno Linhares
 * Change: Added support for Pages, Structure Groups, Publications and their Metadata
 *************************************************************************************
 * Change Date: 10/17/2008
 * Change Author: Nuno Linhares
 * Change: Added support for Folders, Added ItemType property
 *************************************************************************************
 * Change Date: 10/20/2008
 * Change Author: Nuno Linhares
 * Change: Change date format of change log to MM/DD/YYYY
 *************************************************************************************
 * Change Date: 10/20/2008
 * Change Author: Nuno Linhares
 * Change: Stopped throwing exceptions when metadata is empty. Instead all values that
 * are attempted to be retrieved from an empty metadata itemfields will return null
 *************************************************************************************
 * Change Date: 10/22/2008
 * Change Author: Nuno Linhares
 * Change: Added GetComponentLinkField() method and enabled reading Component Links in 
 * FindItemField() method
 *************************************************************************************
 * Change Date: 10/22/2008
 * Change Author: Nuno Linhares
 * Change: Added Number handling for GetFieldValue()
 *************************************************************************************
 * Change Date: 11/08/2008
 * Change Author: Nuno Linhares
 * Change: Added boolean property ReturnAllFieldValues to return all field values, using 
 * MultipleValueSeparator as the separator
 *************************************************************************************
 * Change Date: 03/23/2009
 * Change Author: Nuno Linhares
 * Change: Added Date Format string property for date field output formats
 ************************************************************************************
 * Change Date 04/10/2009
 * Change Author: Nuno Linhares
 * Change: SiteEdit Xpaths for deep embedded fields were missing the first level.
 ************************************************************************************
 * Change Date 06/11/2009
 * Change Author: Nuno Linhares
 * Change: Added GetStringValues method.
 ************************************************************************************
 * Change Date 06/12/2009
 * Change Author: Nuno Linhares
 * Change: Added support for Categories and Keywords Metadata
 ************************************************************************************
 * Change Date 09/15/2009
 * Change Author: Nuno Linhares
 * Change: Added support for Component, Page, Publication, etc to be passed as the first
 * part of a Field Path in GetStringValue ("Component.Fields", "Page.Metadata", etc
 ************************************************************************************
 * Change Date 11/25/2009
 * Change Author: Nuno Linhares
 * Change: Added support for SiteEdit 2009 by allowing <tcdl:ComponentField> format
 * as output. Client must set .SiteEditVersion to SiteEdit2009
 ************************************************************************************
 * Change Date 12/23/2009
 * Change Author: Nuno Linhares
 * Change: Added support for .Title and .Id properties
 ************************************************************************************
 * Change Date 1/26/2010
 * Change Author: Nuno Linhares
 * Change: Added HTML Escaping for Keyword Metadata values (HP request)
 ************************************************************************************/

using System;
using System.Text.RegularExpressions;
using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.CommunicationManagement;
using System.Xml;
using Tridion.ContentManager.Publishing.Rendering;
using System.Globalization;

namespace Tridion.Templates.Generic.Xml
{
    /// <summary>
    /// The TridionOutputHandler class.
    /// </summary>
    public class FieldOutputHandler
    {
        #region Private Members
        private readonly Component _component = null;
        private readonly Page _page = null;
        private readonly Package _package = null;
        private readonly Engine _engine = null;
        private static readonly TemplatingLogger Log = TemplatingLogger.GetLogger(typeof(FieldOutputHandler));
        private readonly ItemFields _contentFields = null;
        private readonly ItemFields _metadataFields = null;
        private bool _siteEdit = false;
        private static readonly Regex PartRegex = new Regex(@"^([^\[]+)(\[(\d+)\])$", RegexOptions.None);
        private bool _isContent = false;
        private string _siteEditFormatString = "<span ID=\"_SE_FLD\" _SE_FLD=\"tcm:{0}/custom:{1}\">{2}</span>";
        private const string SiteEdit2009FormatString = "<tcdl:ComponentField name=\"{0}\" index=\"{1}\">{2}</tcdl:ComponentField>";
        private bool _disableSiteEdit = false;
        private readonly StructureGroup _structureGroup = null;
        private readonly Publication _publication = null;
        private readonly Folder _folder = null;
        private readonly ItemType _itemType = ItemType.None;
        private readonly Category _category = null;
        private readonly Keyword _keyword = null;
        private String _returnType = null;
        private String _multipleValueSeparator = ", ";
        private Boolean _returnAllValues = false;
        private Boolean _pushBinariesToPackage = true;
        private String _dateFormat = "yyyy-MM-ddTHH:mm:ss";
        private CultureInfo _culture = new CultureInfo("en-US");
        private Boolean _escapeOutput = false;

        private SiteEditVersion _siteEditVersion = SiteEditVersion.SiteEdit13;

        private readonly RepositoryLocalObject _currentItem = null;
        

        #endregion Private Members

        #region Public Members

        /// <summary>
        /// Gets or sets a value indicating whether [push binaries to package].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [push binaries to package]; otherwise, <c>false</c>.
        /// </value>
        public Boolean PushBinariesToPackage
        {
            get { return _pushBinariesToPackage; }
            set { _pushBinariesToPackage = value; }
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <value>The component.</value>
        public Component Component
        {
            get { return _component; }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <value>The page.</value>
        public Page Page
        {
            get { return _page; }
        }

        /// <summary>
        /// Gets the structure group.
        /// </summary>
        /// <value>The structure group.</value>
        public StructureGroup StructureGroup
        {
            get { return _structureGroup; }
        }

        /// <summary>
        /// Gets the publication.
        /// </summary>
        /// <value>The publication.</value>
        public Publication Publication
        {
            get { return _publication; }
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        public Category Category
        {
            get { return _category; }
        }

        /// <summary>
        /// Gets the keyword.
        /// </summary>
        /// <value>The keyword.</value>
        public Keyword Keyword
        {
            get { return _keyword; }
        }

        /// <summary>
        /// Gets the folder.
        /// </summary>
        /// <value>The folder.</value>
        public Folder Folder
        {
            get { return _folder; }
        }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        /// <value>The type of the item.</value>
        public ItemType ItemType
        {
            get { return _itemType; }
        }

        /// <summary>
        /// Gets the package.
        /// </summary>
        /// <value>The package.</value>
        public Package Package
        {
            get { return _package; }
        }

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>The engine.</value>
        public Engine Engine
        {
            get { return _engine; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [site edit].
        /// </summary>
        /// <value><c>true</c> if [site edit]; otherwise, <c>false</c>.</value>
        public bool SiteEdit
        {
            get { return _siteEdit; }
            set { _siteEdit = value; }
        }

        /// <summary>
        /// Gets or sets the site edit format string.
        /// </summary>
        /// <value>The site edit format string.</value>
        public String SiteEditFormatString
        {
            get { return _siteEditFormatString; }
            set { _siteEditFormatString = value; }
        }

        /// <summary>
        /// Gets or sets the multiple value separator.
        /// </summary>
        /// <value>The multiple value separator.</value>
        public String MultipleValueSeparator
        {
            get { return _multipleValueSeparator; }
            set { _multipleValueSeparator = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether [return all field values].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [return all field values]; otherwise, <c>false</c>.
        /// </value>
        public Boolean ReturnAllFieldValues
        {
            get { return _returnAllValues; }
            set { _returnAllValues = value; }
        }

        /// <summary>
        /// Gets or sets the current Date Format output.
        /// </summary>
        /// <value>The date format.</value>
        public String DateFormat
        {
            get { return _dateFormat; }
            set { _dateFormat = value; }
        }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>The culture.</value>
        public String Culture
        {
            get { return _culture.ToString(); }
            set { _culture = new CultureInfo(value); }
        }

        public enum SiteEditVersion
        {
            SiteEdit13, 
            SiteEdit2009 
        }

        /// <summary>
        /// Gets or sets the site edit version.
        /// </summary>
        /// <value>The site edit version.</value>
        public SiteEditVersion SeVersion
        {
            get { return _siteEditVersion; }
            set { _siteEditVersion = value; }
        }

        public Boolean EscapeOutput
        {
            get { return _escapeOutput; }
            set { _escapeOutput = value; }
        }

        #endregion Public Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>TridionOutputHandler</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        /// <param name="tcmUri">The TCM URI.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="package">The package.</param>
        public FieldOutputHandler(TcmUri tcmUri, Engine engine, Package package)
        {
            if (!TcmUri.IsValid(tcmUri.ToString()))
            {
                Log.Error(tcmUri + " is not a valid URI. Failed to initialize Output Handler!");
            }
            else
            {
                switch (tcmUri.ItemType)
                {
                    case ItemType.Component:
                        try
                        {
                            _component = new Component(tcmUri, engine.GetSession());
                            _currentItem = _component;
                            if (_component.ComponentType == ComponentType.Normal)
                            {
                                _contentFields = new ItemFields(_component.Content, _component.Schema);
                            }
                            if (_component.Metadata != null)
                                _metadataFields = new ItemFields(_component.Metadata, _component.Schema);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to iniatilize fields for component with ID " + tcmUri + "\r\n Exception: " + ex);
                        }
                        break;
                    case ItemType.Page:
                        try
                        {
                            _page = new Page(tcmUri, engine.GetSession());
                            _currentItem = _page;
                            if (_page.Metadata != null)
                            {
                                _metadataFields = new ItemFields(_page.Metadata, _page.MetadataSchema);
                            }
                            else
                            {
                                Log.Error("Only pages with metadata are allowed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to initialize fields for page with ID " + tcmUri + "\r\n Exception: " + ex);
                        }
                        break;
                    case ItemType.Publication:
                        try
                        {
                            _publication = new Publication(tcmUri, engine.GetSession());
                            _currentItem = null;
                            if (_publication.Metadata != null)
                            {
                                _metadataFields = new ItemFields(_publication.Metadata, _publication.MetadataSchema);
                            }
                            else
                            {
                                Log.Error("Only Publications with Metadata are supported!");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to initialize fields for publication with ID " + tcmUri + "\r\n Exception: " + ex);
                        }
                        break;
                    case ItemType.StructureGroup:
                        try
                        {
                            _structureGroup = new StructureGroup(tcmUri, engine.GetSession());
                            _currentItem = _structureGroup;
                            if (_structureGroup.Metadata != null)
                            {
                                _metadataFields = new ItemFields(_structureGroup.Metadata, _structureGroup.MetadataSchema);
                            }
                            else
                            {
                                Log.Error("Only Structure Groups with Metadata are supported!");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to initialize fields for Structure Group with ID " + tcmUri + "\r\n Exception: " + ex);
                        }
                        break;
                    case ItemType.Folder:
                        try
                        {
                            
                            _folder = new Folder(tcmUri, engine.GetSession());
                            _currentItem = _folder;
                            if (_folder.Metadata != null)
                            {
                                _metadataFields = new ItemFields(_folder.Metadata, _folder.MetadataSchema);
                            }
                            else
                            {
                                Log.Error("Only Folders with Metadata are supported!");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to initialize fields for Folder with ID " + tcmUri + "\r\n Exception: " + ex);
                        }
                        break;
                    case ItemType.Category:
                        try
                        {
                            _category = new Category(tcmUri, engine.GetSession());
                            _currentItem = _category;
                            if (_category.Metadata != null)
                            {
                                _metadataFields = new ItemFields(_category.Metadata, _category.MetadataSchema);
                            }
                            else
                            {
                                Log.Error("Only Categories with Metadata are supported!");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to initialize fields for Category with ID " + tcmUri + " \r\n Exception: " + ex);
                        }
                        break;
                    case ItemType.Keyword:
                        try
                        {
                            _keyword = new Keyword(tcmUri, engine.GetSession());
                            _currentItem = _keyword;
                            if (_keyword.Metadata != null)
                            {
                                _metadataFields = new ItemFields(_keyword.Metadata, _keyword.MetadataSchema);
                                _escapeOutput = true;
                            }
                            else
                            {
                                Log.Error("Only Keywords with Metadata are supported!");
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to initialize fields for Keyword with ID " + tcmUri + " \r\n Exception: " + ex);
                        }
                        break;

                }
                
                _engine = engine;
                _package = package;
                _itemType = tcmUri.ItemType;
            }
        }
        #endregion Constructor

        /// <summary>
        /// Gets the component link field.
        /// </summary>
        /// <param name="qualifiedFieldName">Qualified name of the Component link field.</param>
        /// <returns></returns>
        public ComponentLinkField GetComponentLinkField(string qualifiedFieldName)
        {
            _returnType = "ComponentLinkField";
            ItemField value = null;
            if (_component != null || _metadataFields != null)
            {
                string firstPart = SourceUtilities.GetFirstQualifiedName(qualifiedFieldName);
                
                bool checkContentFields = false;
                bool checkMetaFields = false;
                bool skipFirstPart = false;
                bool isMultiValue;
                if (firstPart.Equals("Metadata"))
                {
                    skipFirstPart = true;
                    checkMetaFields = true;
                }
                else if (firstPart.Equals("Fields") && _component != null)
                {
                    skipFirstPart = true;
                    checkContentFields = true;
                    _isContent = true;
                }
                else
                {
                    checkContentFields = true;
                    checkMetaFields = true;
                }
                string[] parts;
                if (skipFirstPart)
                {
                    parts = qualifiedFieldName.Substring(qualifiedFieldName.IndexOf(SourceUtilities.QualifiedNameSeparator) + 1).Split(SourceUtilities.QualifiedNameSeparator);
                }
                else
                {
                    parts = qualifiedFieldName.Split(SourceUtilities.QualifiedNameSeparator);
                }
                if (checkContentFields)
                {
                    value = _contentFields != null ? FindItemField(parts, 0, _contentFields, out isMultiValue) : null;
                }
                if (value == null && checkMetaFields)
                {
                    if (_metadataFields != null)
                    {
                        _isContent = false;
                        value = FindItemField(parts, 0, _metadataFields, out isMultiValue);
                    }
                    else
                        value = null;
                }
            }
            if (value is ComponentLinkField)
                return (ComponentLinkField)value;
            return null;
        }

        /// <summary>
        /// Gets the embedded schema field.
        /// </summary>
        /// <param name="qualifiedFieldName">Name of the qualified field.</param>
        /// <returns>An EmbeddedSchemaField</returns>
        public EmbeddedSchemaField GetEmbeddedSchemaField(string qualifiedFieldName)
        {
            _returnType = "EmbeddedSchemaField";
            ItemField value = null;
            if (_component != null || _metadataFields != null)
            {
                string firstPart = SourceUtilities.GetFirstQualifiedName(qualifiedFieldName);
                bool checkContentFields = false;
                bool checkMetaFields = false;
                bool skipFirstPart = false;
                bool isMultiValue = false;
                if (firstPart.Equals("Metadata"))
                {
                    skipFirstPart = true;
                    checkMetaFields = true;
                }
                else if (firstPart.Equals("Fields") && _component != null)
                {
                    skipFirstPart = true;
                    checkContentFields = true;
                    _isContent = true;
                }
                else
                {
                    checkContentFields = true;
                    checkMetaFields = true;
                }
                string[] parts;
                if (skipFirstPart)
                {
                    parts = qualifiedFieldName.Substring(qualifiedFieldName.IndexOf(SourceUtilities.QualifiedNameSeparator) + 1).Split(SourceUtilities.QualifiedNameSeparator);
                }
                else
                {
                    parts = qualifiedFieldName.Split(SourceUtilities.QualifiedNameSeparator);
                }

                if (checkContentFields)
                {
                    if (_contentFields != null)
                        value = FindItemField(parts, 0, _contentFields, out isMultiValue);
                    else
                        value = null;
                }
                if (value == null && checkMetaFields)
                {
                    if (_metadataFields != null)
                    {
                        _isContent = false;
                        value = FindItemField(parts, 0, _metadataFields, out isMultiValue);
                    }
                    else
                        value = null;
                }
            }
            if (value is EmbeddedSchemaField)
                return (EmbeddedSchemaField)value;
            else
                return null;
        }

        /// <summary>
        /// Finds the item field.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="currentIndex">Index of the current.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="isMultiValue">if set to <c>true</c> [is multi value].</param>
        /// <returns></returns>
        private ItemField FindItemField(string[] parts, int currentIndex, ItemFields fields, out bool isMultiValue)
        {
            string input = parts[currentIndex];
            int num = -1;
            Match match = PartRegex.Match(input);
            if (match.Success)
            {
                input = match.Groups[1].Value;
                num = Convert.ToInt32(match.Groups[3].Value);
            }
            if (fields.Contains(input))
            {
                ItemField field = fields[input];
                if ((parts.Length - 1) == currentIndex)
                {
                    isMultiValue = field.Definition.MaxOccurs != 1;
                    return field;
                }
                else
                {
                    if (field is EmbeddedSchemaField)
                    {
                        EmbeddedSchemaField schema = field as EmbeddedSchemaField;
                        if (schema != null)
                            return (FindItemField(parts, currentIndex + 1, ((num >= 0) ? (schema.Values[num]) : schema.Value), out isMultiValue));
                    }
                    else if (field is ComponentLinkField || field is MultimediaLinkField)
                    {
                        Component LinkedComponent = null;
                        if (field is ComponentLinkField)
                        {
                            ComponentLinkField componentLink = field as ComponentLinkField;
                            LinkedComponent = new Component(((num >= 0) ? (componentLink.Values[num].Id) : componentLink.Value.Id), _engine.GetSession());
                        }
                        else
                        {
                            MultimediaLinkField componentLink = field as MultimediaLinkField;
                            LinkedComponent = new Component(((num >= 0) ? (componentLink.Values[num].Id) : componentLink.Value.Id), _engine.GetSession());
                        }

                        FieldOutputHandler subHandler = new FieldOutputHandler(LinkedComponent.Id, _engine, _package);
                        subHandler.SiteEdit = false;
                        _disableSiteEdit = true;
                        subHandler.EscapeOutput = _escapeOutput;
                        subHandler.DateFormat = _dateFormat;
                        subHandler.Culture = _culture.ToString();
                        subHandler.PushBinariesToPackage = _pushBinariesToPackage;
                        String QualifiedFieldName = String.Empty;
                        for (int i = currentIndex + 1; i < parts.Length; i++)
                        {
                            QualifiedFieldName += parts[i];
                            if (i < (parts.Length - 1))
                                QualifiedFieldName += ".";
                        }
                        isMultiValue = field.Definition.MaxOccurs != 1;
                        if (_returnType == "ComponentLinkField")
                            return subHandler.GetComponentLinkField(QualifiedFieldName);
                        else if (_returnType == "EmbeddedSchemaField")
                            return subHandler.GetEmbeddedSchemaField(QualifiedFieldName);
                        else
                            return null;
                    }

                    Log.Warning(string.Format("Part \"{0}\" in qualified field name was expected to refer to an embedded schema!", field.Name));

                }
            }
            isMultiValue = false;
            return null;
        }

        /// <summary>
        /// Gets the string values.
        /// </summary>
        /// <param name="QualifiedFieldName">Name of the qualified field.</param>
        /// <returns></returns>
        public string[] GetStringValues(string QualifiedFieldName)
        {
            String currentseparator = _multipleValueSeparator;
            Boolean currentValueSetting = _returnAllValues;
            _multipleValueSeparator = "\\";
            String[] output = GetStringValue(QualifiedFieldName).Split('\\');

            _multipleValueSeparator = currentseparator;
            _returnAllValues = currentValueSetting;

            return output;
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="QualifiedFieldName">Name of the qualified field.</param>
        /// <returns>A field's value as a String.</returns>
        public string GetStringValue(string QualifiedFieldName)
        {

            string xpath = null;
            if (_component != null || _metadataFields != null)
            {
                string firstPart = SourceUtilities.GetFirstQualifiedName(QualifiedFieldName);
                switch (firstPart)
                {
                    case "Publication":
                    case "Folder":
                    case "Component":
                    case "Page":
                    case "StructureGroup":
                        {
                            QualifiedFieldName = QualifiedFieldName.Replace(firstPart + ".", "");

                            firstPart = SourceUtilities.GetFirstQualifiedName(QualifiedFieldName);
                        }
                        break;
                }
                bool checkContentFields = false;
                bool checkMetaFields = false;
                bool skipFirstPart = false;
                bool isMultiValue = false;
                if (firstPart.Equals("Metadata"))
                {
                    skipFirstPart = true;
                    checkMetaFields = true;
                }
                else if (firstPart.Equals("Fields"))
                {
                    skipFirstPart = true;
                    checkContentFields = true;
                    _isContent = true;
                }
                else
                {
                    if (_currentItem != null)
                    {
                        if (firstPart.Equals("Title"))
                            if(_escapeOutput)
                                return System.Security.SecurityElement.Escape(_currentItem.Title);
                            else
                                return _currentItem.Title;
                        else if (firstPart.Equals("Id"))
                            return _currentItem.Id.ToString();
                    }
                    else
                    {
                        if (_publication != null)
                        {
                            if (firstPart.Equals("Title"))
                                if(_escapeOutput)
                                    return System.Security.SecurityElement.Escape(_publication.Title);
                                else
                                    return _publication.Title;
                            else if (firstPart.Equals("Id"))
                                return _publication.Id.ToString();
                        }
                    }
                    checkContentFields = true;
                    checkMetaFields = true;
                }

                string[] parts;
                if (skipFirstPart)
                {
                    parts = QualifiedFieldName.Substring(QualifiedFieldName.IndexOf(SourceUtilities.QualifiedNameSeparator) + 1).Split(SourceUtilities.QualifiedNameSeparator);
                }
                else
                {
                    parts = QualifiedFieldName.Split(SourceUtilities.QualifiedNameSeparator);
                }

                if (checkContentFields)
                {
                    xpath = FindItemFieldXPathAndValue(parts, 0, string.Empty, _contentFields, out isMultiValue);
                }
                if (xpath == null && checkMetaFields)
                {
                    _isContent = false;
                    xpath = FindItemFieldXPathAndValue(parts, 0, string.Empty, _metadataFields, out isMultiValue);
                }


            }
            _disableSiteEdit = false;
            return xpath;
        }


        
        /// <summary>
        /// Finds the item field X path and value.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="currentIndex">Index of the current.</param>
        /// <param name="currentXpath">The current xpath.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="isMultiValue">if set to <c>true</c> [is multi value].</param>
        /// <returns>The field's value wrapped in SiteEdit span tags if SiteEdit is set to <c>true</c>.</returns>
        private string FindItemFieldXPathAndValue(string[] parts, int currentIndex, string currentXpath, ItemFields fields, out bool isMultiValue)
        {
            if (_itemType != ItemType.Component)
                _disableSiteEdit = true;

            string input = parts[currentIndex];
            int num = -1;
            Match match = PartRegex.Match(input);
            if (match.Success)
            {
                input = match.Groups[1].Value;
                num = Convert.ToInt32(match.Groups[3].Value);
            }
            if (fields != null)
            {
                if (fields.Contains(input))
                {
                    ItemField field = fields[input];
                    if ((parts.Length - 1) == currentIndex)
                    {
                        isMultiValue = field.Definition.MaxOccurs != 1;
                        string fieldValue = GetFieldValue(field, ((num == -1) ? 0 : num));
                        if (_siteEdit && !_disableSiteEdit)
                            if (_siteEditVersion == SiteEditVersion.SiteEdit13)
                                return (String.Format(_siteEditFormatString, ((_isContent) ? "Content" : "Metadata"), (((_isContent) ? _component.Content.LocalName : _component.Metadata.LocalName) + "/" + currentXpath) + "custom:" + field.Name + ((num >= 0) ? ("[" + (num + 1) + "]") : ""), fieldValue));
                            else
                                return (String.Format(SiteEdit2009FormatString, ((_isContent) ? "Fields" : "MetaData") + "." + currentXpath + field.Name, ((num >= 0) ? num : 0), fieldValue));
                        else
                            return fieldValue;
                    }
                    EmbeddedSchemaField schema = field as EmbeddedSchemaField;
                    if (schema != null && schema is EmbeddedSchemaField)
                    {
                        return (this.FindItemFieldXPathAndValue(parts, currentIndex + 1, currentXpath + "custom:" + field.Name + ((num >= 0) ? ("[" + (num + 1) + "]") : "") + "/", ((num >= 0) ? (schema.Values[num]) : schema.Value), out isMultiValue));
                    }
                    else if (field is ComponentLinkField || field is MultimediaLinkField)
                    {
                        Component LinkedComponent = null;
                        if (field is ComponentLinkField)
                        {
                            ComponentLinkField componentLink = field as ComponentLinkField;
                            LinkedComponent = new Component(((num >= 0) ? (componentLink.Values[num].Id) : componentLink.Value.Id), _engine.GetSession());
                        }
                        else
                        {
                            MultimediaLinkField componentLink = field as MultimediaLinkField;
                            LinkedComponent = new Component(((num >= 0) ? (componentLink.Values[num].Id) : componentLink.Value.Id), _engine.GetSession());
                        }

                        FieldOutputHandler subHandler = new FieldOutputHandler(LinkedComponent.Id, _engine, _package);
                        subHandler.SiteEdit = false;
                        _disableSiteEdit = true;
                        subHandler.EscapeOutput = _escapeOutput;
                        subHandler.DateFormat = _dateFormat;
                        subHandler.Culture = _culture.ToString();
                        subHandler.PushBinariesToPackage = _pushBinariesToPackage;
                        String QualifiedFieldName = String.Empty;
                        for (int i = currentIndex + 1; i < parts.Length; i++)
                        {
                            QualifiedFieldName += parts[i];
                            if (i < (parts.Length - 1))
                                QualifiedFieldName += ".";
                        }
                        isMultiValue = field.Definition.MaxOccurs != 1;
                        String SubHandlerResult = subHandler.GetStringValue(QualifiedFieldName);
                        return SubHandlerResult;
                    }
                    else if (field is KeywordField)
                    {
                        KeywordField Keyfield = (KeywordField)field;
                        FieldOutputHandler subHandler = new FieldOutputHandler(Keyfield.Values[0].Id, _engine, _package);
                        subHandler.SiteEdit = false;
                        _disableSiteEdit = true;
                        subHandler.EscapeOutput = _escapeOutput;
                        subHandler.DateFormat = _dateFormat;
                        subHandler.Culture = _culture.ToString();
                        subHandler.PushBinariesToPackage = _pushBinariesToPackage;
                        String QualifiedFieldName = String.Empty;
                        for (int i = currentIndex + 1; i < parts.Length; i++)
                        {
                            QualifiedFieldName += parts[i];
                            if (i < (parts.Length - 1))
                                QualifiedFieldName += ".";
                        }
                        isMultiValue = field.Definition.MaxOccurs != 1;
                        String SubHandlerResult = subHandler.GetStringValue(QualifiedFieldName);
                        return SubHandlerResult;
                        
                    }

                    Log.Warning(string.Format("Part {0} in qualified field name was expected to refer to an embedded schema!", field.Name));
                }
            }
            isMultiValue = false;
            return null;
        }



        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="position">The position.</param>
        /// <returns>The field's value[position]</returns>
        internal string GetFieldValue(ItemField field, int position)
        {
            string value = String.Empty;
            if (field != null)
            {
                bool MultiValue = field.Definition.MaxOccurs != 1;
                switch (field.GetType().Name)
                {
                    case "ExternalLinkField":
                        ExternalLinkField extfield = (ExternalLinkField)field;
                        value = extfield.Value;
                        break;
                    case "NumberField":
                        NumberField numField = (NumberField)field;
                        if (_returnAllValues)
                        {
                            foreach (Double fieldValue in numField.Values)
                            {
                                value += Convert.ToString(fieldValue);
                                value += _multipleValueSeparator;
                            }
                            if (value.EndsWith(_multipleValueSeparator))
                                value = value.Substring(0, value.LastIndexOf(_multipleValueSeparator));
                        }
                        else if (numField.Values.Count > position)
                            value = Convert.ToString(numField.Values[position]);
                        break;
                    case "SingleLineTextField":
                    case "MultiLineTextField":
                        TextField textField = (TextField)field;
                        if (_returnAllValues)
                        {
                            foreach (String fieldValue in textField.Values)
                            {
                                value += fieldValue;
                                value += _multipleValueSeparator;
                            }
                            if (value.EndsWith(_multipleValueSeparator))
                                value = value.Substring(0, value.LastIndexOf(_multipleValueSeparator));
                        }
                        else if (textField.Values.Count > position)
                            value = textField.Values[position];

                        if (_escapeOutput)
                            value = System.Security.SecurityElement.Escape(value);
                        break;
                    case "XhtmlField":
                        XhtmlField XhtmlField = (XhtmlField)field;
                        if (_returnAllValues)
                        {
                            foreach (String fieldValue in XhtmlField.Values)
                            {
                                value += fieldValue;
                                value += _multipleValueSeparator;
                            }
                            if (value.EndsWith(_multipleValueSeparator))
                                value = value.Substring(0, value.LastIndexOf(_multipleValueSeparator));
                        }
                        else if (XhtmlField.Values.Count > position)
                        {
                            String XhtmlSource = XhtmlField.Values[position];
                            value = ConvertLinksInRtf(XhtmlSource);
                            value = TemplateUtilities.ResolveRichTextFieldXhtml(value);
                        }  
                        break;
                    case "KeywordField":
                        KeywordField keywordField = (KeywordField)field;
                        if (_returnAllValues)
                        {
                            foreach (Keyword fieldValue in keywordField.Values)
                            {
                                value += fieldValue.Title;
                                value += _multipleValueSeparator;
                            }
                            if (value.EndsWith(_multipleValueSeparator))
                                value = value.Substring(0, value.LastIndexOf(_multipleValueSeparator));
                        }
                        else if (keywordField.Values.Count > position)
                            value = keywordField.Values[position].Title;

                        if (_escapeOutput)
                            value = System.Security.SecurityElement.Escape(value);
                        break;
                    case "MultimediaLinkField":
                        MultimediaLinkField mmField = (MultimediaLinkField)field;
                        if (mmField.Values.Count > position)
                        {
                            if (_pushBinariesToPackage)
                                _package.PushItem(_package.CreateMultimediaItem(mmField.Values[position].Id));
                            value = mmField.Values[position].Id.ToString();
                        }
                        _disableSiteEdit = true;
                        break;
                    case "ComponentLinkField":
                        ComponentLinkField clField = (ComponentLinkField)field;
                        if (clField.Values.Count > position)
                        {
                            if (clField.Values[position].ComponentType == ComponentType.Multimedia)
                                _package.PushItem(_package.CreateMultimediaItem(clField.Values[position].Id));
                            value = clField.Values[position].Id.ToString();
                        }
                        _disableSiteEdit = true;
                        break;
                    case "DateField":
                        DateField dateField = (DateField)field;
                        if (_returnAllValues)
                        {
                            foreach (DateTime fieldValue in dateField.Values)
                            {
                                value += fieldValue.ToString(_dateFormat, _culture);
                                value += _multipleValueSeparator;
                            }
                            if (value.EndsWith(_multipleValueSeparator))
                                value = value.Substring(0, value.LastIndexOf(_multipleValueSeparator));
                        }
                        if (dateField.Values.Count > position)
                        {
                            value = dateField.Values[position].ToString(_dateFormat, _culture);
                        }
                        break;
                }
            }
            return value;
        }

        private String ConvertLinksInRtf(String rtfContent)
        {
            // Get rid quickly of those we don't need to touch
            if (rtfContent.IndexOf("xlink:href") < 1)
                return rtfContent;

            XmlDocument XmlContent = new XmlDocument();
            XmlNamespaceManager nm = new XmlNamespaceManager(new NameTable());
            nm.AddNamespace(Constants.XlinkPrefix, Constants.XlinkNamespace);
            nm.AddNamespace(Constants.XhtmlPrefix, Constants.XhtmlNamespace);
            nm.AddNamespace(Constants.TcmPrefix, Constants.TcmtNamespace);

            XmlContent.LoadXml("<root>" + rtfContent + "</root>");


            foreach (XmlNode node in XmlContent.SelectNodes("//xhtml:a[@xlink:href]", nm))
            {
                TcmUri item = new TcmUri(node.Attributes["href", Constants.XlinkNamespace].Value);
                if (item.ItemType == ItemType.Component)
                {
                    Component c = _engine.GetObject(item) as Component;
                    if (c.BinaryContent != null)
                    {
                        // Item is a binary item, must publish and push to package
                        // then what?
                        // Replace with Publish binary?
                        _package.PushItem(_package.CreateMultimediaItem(item));
                        Binary binary = _engine.PublishingContext.RenderedItem.AddBinary(c, "");
                        node.Attributes.RemoveAll();
                        String PublishedPath = GetUrlForBinary(binary.Url);
                        XmlAttribute attr = XmlContent.CreateAttribute("href");
                        attr.Value = PublishedPath;
                        node.Attributes.SetNamedItem(attr);

                        attr = XmlContent.CreateAttribute("title");
                        attr.Value = c.Title;
                        node.Attributes.SetNamedItem(attr);
                    }
                }
            }
            return XmlContent["root"].InnerXml;
        }
        private static String GetUrlForBinary(String Url)
        {
            return Regex.Replace(Url, @"_tcm[\d]*-[\d]*.", ".");
        }
    }
}
