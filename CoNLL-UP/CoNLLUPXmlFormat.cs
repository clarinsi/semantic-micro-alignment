namespace CoNLLUP
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class text
    {
        private textDoc docField;

        /// <remarks/>
        public textDoc doc
        {
            get
            {
                return this.docField;
            }
            set
            {
                this.docField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class textDoc
    {
        private textDocP[] pField;

        private System.DateTime dateField;

        private System.DateTime date_approvedField;

        private System.DateTime date_effectField;

        private string entypeField;

        private string idField;

        private string issuerField;

        private string languageField;

        private string titleField;

        private string typeField;

        private string urlField;

        private textDocPS[] sField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("p")]
        public textDocP[] p
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

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("s")]
        public textDocPS[] s
        {
            get
            {
                return this.sField;
            }
            set
            {
                this.sField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime date_approved
        {
            get
            {
                return this.date_approvedField;
            }
            set
            {
                this.date_approvedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime date_effect
        {
            get
            {
                return this.date_effectField;
            }
            set
            {
                this.date_effectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string entype
        {
            get
            {
                return this.entypeField;
            }
            set
            {
                this.entypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string issuer
        {
            get
            {
                return this.issuerField;
            }
            set
            {
                this.issuerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string language
        {
            get
            {
                return this.languageField;
            }
            set
            {
                this.languageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class textDocP
    {
        private textDocPS[] sField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("s")]
        public textDocPS[] s
        {
            get
            {
                return this.sField;
            }
            set
            {
                this.sField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class textDocPS
    {
        private textDocPSToken[] tokenField;

        private string idField;

        private string textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("token")]
        public textDocPSToken[] token
        {
            get
            {
                return this.tokenField;
            }
            set
            {
                this.tokenField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string text
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class textDocPSToken
    {
        private int idField;

        private string formField;

        private string lemmaField;

        private string uposField;

        private string xposField;

        private string featsField;

        private string headField;

        private string deprelField;

        private string depsField;

        private string miscField;

        private string marcell_neField;

        private string marcell_npField;

        private string marcell_iateField;

        private string marcell_eurovocField;

        /// <remarks/>
        public int id
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

        /// <remarks/>
        public string form
        {
            get
            {
                return this.formField;
            }
            set
            {
                this.formField = value;
            }
        }

        /// <remarks/>
        public string lemma
        {
            get
            {
                return this.lemmaField;
            }
            set
            {
                this.lemmaField = value;
            }
        }

        /// <remarks/>
        public string upos
        {
            get
            {
                return this.uposField;
            }
            set
            {
                this.uposField = value;
            }
        }

        /// <remarks/>
        public string xpos
        {
            get
            {
                return this.xposField;
            }
            set
            {
                this.xposField = value;
            }
        }

        /// <remarks/>
        public string feats
        {
            get
            {
                return this.featsField;
            }
            set
            {
                this.featsField = value;
            }
        }

        /// <remarks/>
        public string head
        {
            get
            {
                return this.headField;
            }
            set
            {
                this.headField = value;
            }
        }

        /// <remarks/>
        public string deprel
        {
            get
            {
                return this.deprelField;
            }
            set
            {
                this.deprelField = value;
            }
        }

        /// <remarks/>
        public string deps
        {
            get
            {
                return this.depsField;
            }
            set
            {
                this.depsField = value;
            }
        }

        /// <remarks/>
        public string misc
        {
            get
            {
                return this.miscField;
            }
            set
            {
                this.miscField = value;
            }
        }

        /// <remarks/>
        public string marcell_ne
        {
            get
            {
                return this.marcell_neField;
            }
            set
            {
                this.marcell_neField = value;
            }
        }

        /// <remarks/>
        public string marcell_np
        {
            get
            {
                return this.marcell_npField;
            }
            set
            {
                this.marcell_npField = value;
            }
        }

        /// <remarks/>
        public string marcell_iate
        {
            get
            {
                return this.marcell_iateField;
            }
            set
            {
                this.marcell_iateField = value;
            }
        }

        /// <remarks/>
        public string marcell_eurovoc
        {
            get
            {
                return this.marcell_eurovocField;
            }
            set
            {
                this.marcell_eurovocField = value;
            }
        }
    }
}