namespace TBXFormat
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbx")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:iso:std:iso:30042:ed-2", IsNullable = false, ElementName = "tbx")]
    public partial class Tbx
    {
        private TbxTbxHeader tbxHeaderField;

        private TbxText textField;

        private string typeField;

        private string styleField;

        private string langField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "tbxHeader")]
        public TbxTbxHeader Header
        {
            get
            {
                return this.tbxHeaderField;
            }
            set
            {
                this.tbxHeaderField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "text")]
        public TbxText Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "type")]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "style")]
        public string Style
        {
            get
            {
                return this.styleField;
            }
            set
            {
                this.styleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace", AttributeName = "lang")]
        public string Language
        {
            get
            {
                return this.langField;
            }
            set
            {
                this.langField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTbxHeader")]
    public partial class TbxTbxHeader
    {
        private TbxTbxHeaderFileDesc fileDescField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "fileDesc")]
        public TbxTbxHeaderFileDesc FileDescription
        {
            get
            {
                return this.fileDescField;
            }
            set
            {
                this.fileDescField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTbxHeaderFileDesc")]
    public partial class TbxTbxHeaderFileDesc
    {
        private TbxTbxHeaderFileDescTitleStmt titleStmtField;

        private TbxTbxHeaderFileDescSourceDesc sourceDescField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "titleStmt")]
        public TbxTbxHeaderFileDescTitleStmt TitleStatement
        {
            get
            {
                return this.titleStmtField;
            }
            set
            {
                this.titleStmtField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "sourceDesc")]
        public TbxTbxHeaderFileDescSourceDesc SourceDescription
        {
            get
            {
                return this.sourceDescField;
            }
            set
            {
                this.sourceDescField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTbxHeaderFileDescTitleStmt")]
    public partial class TbxTbxHeaderFileDescTitleStmt
    {
        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "title")]
        public string Title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTbxHeaderFileDescSourceDesc")]
    public partial class TbxTbxHeaderFileDescSourceDesc
    {
        private string pField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "p")]
        public string Paragraph
        {
            get
            {
                return this.pField;
            }
            set
            {
                this.pField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxText")]
    public partial class TbxText
    {
        private TbxTextConceptEntry[] bodyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("conceptEntry", IsNullable = false)]
        public TbxTextConceptEntry[] body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTextConceptEntry")]
    public partial class TbxTextConceptEntry
    {
        private TbxTextConceptEntryDescrip descripField;

        private TbxTextConceptEntryLangSec langSecField;

        private uint idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "descrip")]
        public TbxTextConceptEntryDescrip Description
        {
            get
            {
                return this.descripField;
            }
            set
            {
                this.descripField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "langSec")]
        public TbxTextConceptEntryLangSec LanguageSec
        {
            get
            {
                return this.langSecField;
            }
            set
            {
                this.langSecField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "id")]
        public uint Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTextConceptEntryDescrip")]
    public partial class TbxTextConceptEntryDescrip
    {
        private string typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "type")]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTextConceptEntryLangSec")]
    public partial class TbxTextConceptEntryLangSec
    {
        private TbxTextConceptEntryLangSecTermSec[] termSecField;

        private string langField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("termSec")]
        public TbxTextConceptEntryLangSecTermSec[] termSec
        {
            get
            {
                return this.termSecField;
            }
            set
            {
                this.termSecField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace", AttributeName = "lang")]
        public string Language
        {
            get
            {
                return this.langField;
            }
            set
            {
                this.langField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTextConceptEntryLangSecTermSec")]
    public partial class TbxTextConceptEntryLangSecTermSec
    {
        private string termField;

        private TbxTextConceptEntryLangSecTermSecTermNote[] termNoteField;

        private TbxTextConceptEntryLangSecTermSecDescrip descripField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "term")]
        public string Term
        {
            get
            {
                return this.termField;
            }
            set
            {
                this.termField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("termNote")]
        public TbxTextConceptEntryLangSecTermSecTermNote[] termNote
        {
            get
            {
                return this.termNoteField;
            }
            set
            {
                this.termNoteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(ElementName = "descrip")]
        public TbxTextConceptEntryLangSecTermSecDescrip Description
        {
            get
            {
                return this.descripField;
            }
            set
            {
                this.descripField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTextConceptEntryLangSecTermSecTermNote")]
    public partial class TbxTextConceptEntryLangSecTermSecTermNote
    {
        private string typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "type")]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:iso:std:iso:30042:ed-2", TypeName = "tbxTextConceptEntryLangSecTermSecDescrip")]
    public partial class TbxTextConceptEntryLangSecTermSecDescrip
    {
        private string typeField;

        private byte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "type")]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}