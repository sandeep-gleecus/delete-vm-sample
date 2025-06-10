using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extends the CustomProperty entity
    /// </summary>
    public partial class CustomProperty : Entity
    {
        public const string FIELD_PREPEND = "Custom_";
        public const int MAX_NUMBER_ARTIFACT_PROPERTIES = 99;

        #region Enumerations

        /// <summary>
        /// The various custom property types
        /// </summary>
        public enum CustomPropertyTypeEnum
        {
            Text = 1,
            Integer = 2,
            Decimal = 3,
            Boolean = 4,
            Date = 5,
            List = 6,
            MultiList = 7,
            User = 8
        }

        /// <summary>
        /// The various custom property options
        /// </summary>
        public enum CustomPropertyOptionEnum
        {
            AllowEmpty = 1,
            MaxLength = 2,
            MinLength = 3,
            RichText = 4,
            Default = 5,
            MaxValue = 6,
            MinValue = 7,
            Precision = 8
        }

        #endregion

        /// <summary>
        /// Returns the type of the field that is defined in the TST_ARTIFACT_FIELD_TYPE table.
        /// </summary>
        public Artifact.ArtifactFieldTypeEnum FieldTypeId
        {
            get
            {
                Artifact.ArtifactFieldTypeEnum retType;
                switch (this.CustomPropertyTypeId)
                {
                    case (int)CustomPropertyTypeEnum.Text:
                        {
                            if (this.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomPropertyOptionEnum.RichText).Count() > 0)
                            {
                                CustomPropertyOptionValue optValue = this.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomPropertyOptionEnum.RichText).Single();
                                if (optValue != null)
                                {
                                    bool? isRich = optValue.Value.FromDatabaseSerialization_Boolean();
                                    if (isRich.HasValue && isRich.Value)
                                        retType = Artifact.ArtifactFieldTypeEnum.Html;
                                    else
                                        retType = Artifact.ArtifactFieldTypeEnum.Text;
                                }
                                else
                                {
                                    retType = Artifact.ArtifactFieldTypeEnum.Text;
                                }
                            }
                            else
                            {
                                retType = Artifact.ArtifactFieldTypeEnum.Text;
                            }
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.Integer:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.Integer;
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.Decimal:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.Decimal;
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.Boolean:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.Flag;
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.Date:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.CustomPropertyDate;
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.List:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.Lookup;
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.MultiList:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.CustomPropertyMultiList;
                        }
                        break;

                    case (int)CustomPropertyTypeEnum.User:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.Integer;
                        }
                        break;


                    default:
                        {
                            retType = Artifact.ArtifactFieldTypeEnum.Text;
                        }
                        break;
                }

                return retType;
            }
        }

        #region Shortcut Properties

        /// <summary>
        /// Returns the custom property's Type Name ('List', 'User', etc.)
        /// </summary>
        public string CustomPropertyTypeName
        {
            get
            {
                if (this.Type != null)
                {
                    return this.Type.Name;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Returns the corresponding field name (Custom_01, Custom_02, etc.)
        /// </summary>
        public string CustomPropertyFieldName
        {
            get
            {
                return FIELD_PREPEND + PropertyNumber.ToString("00");
            }
        }

        #endregion
    }
}
