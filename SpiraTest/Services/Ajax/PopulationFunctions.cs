using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DiffPlex.DiffBuilder.Model;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Contains helper methods for populating data transfer objects from the internal entities
    /// </summary>
    public static class PopulationFunctions
    {
        /// <summary>
        /// Populates a data item from a Build view
        /// </summary>
        /// <param name="buildDataItem">The build data item</param>
        /// <param name="build">The build view</param>
        internal static void PopulateDataItem(DataItem buildDataItem, BuildView build)
        {
            buildDataItem.PrimaryKey = build.BuildId;

            //Fields
            DataItemField field;

            //Name
            field = new DataItemField();
            field.FieldName = "Name";
            buildDataItem.Fields.Add(field.FieldName, field);
            field.TextValue = build.Name;

            //Status
            field = new DataItemField();
            field.FieldName = "BuildStatusId";
            buildDataItem.Fields.Add(field.FieldName, field);
            field.IntValue = build.BuildStatusId;
            field.TextValue = build.BuildStatusName;
            field.CssClass = GlobalFunctions.GetBuildStatusCssClass((Build.BuildStatusEnum)build.BuildStatusId);

            //Creation Date
            field = new DataItemField();
            field.FieldName = "CreationDate";
            buildDataItem.Fields.Add(field.FieldName, field);
            field.DateValue = build.CreationDate;
            field.TextValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(build.CreationDate));

            //Release
            field = new DataItemField();
            field.FieldName = "ReleaseId";
            buildDataItem.Fields.Add(field.FieldName, field);
            field.IntValue = build.ReleaseId;
            field.TextValue = build.ReleaseVersionNumber;

            //Project
            field = new DataItemField();
            field.FieldName = "ProjectId";
            buildDataItem.Fields.Add(field.FieldName, field);
            field.IntValue = build.ProjectId;
        }

        /// <summary>
        /// Populates the data object returned by the Ajax call from the internal DIFF model
        /// </summary>
        /// <param name="textDiff">The data object returned by the Ajax call</param>
        /// <param name="sideBySideDiffModel">The model from the DIFF library</param>
        internal static void PopulateDataItem(TextDiff textDiff, SideBySideDiffModel sideBySideDiffModel)
        {
            textDiff.NewText = new TextDiffPane();
            PopulateDataItem(textDiff.NewText, sideBySideDiffModel.NewText);
            textDiff.OldText = new TextDiffPane();
            PopulateDataItem(textDiff.OldText, sideBySideDiffModel.OldText);
        }

        /// <summary>
        /// Populates the data object returned by the Ajax call from the internal DIFF model
        /// </summary>
        internal static void PopulateDataItem(TextDiffPane textDiffPane, DiffPaneModel diffPaneModel)
        {
            textDiffPane.HasDifferences = diffPaneModel.HasDifferences;
            textDiffPane.Lines = new List<TextDiffPiece>();
            foreach (DiffPiece diffPiece in diffPaneModel.Lines)
            {
                TextDiffPiece textDiffPiece = new TextDiffPiece();
                textDiffPane.Lines.Add(textDiffPiece);
                PopulateDataItem(textDiffPiece, diffPiece);
            }
        }

        /// <summary>
        /// Populates the data object returned by the Ajax call from the internal DIFF model
        /// </summary>
        internal static void PopulateDataItem(TextDiffPiece textDiffPiece, DiffPiece diffPiece)
        {
            textDiffPiece.Position = diffPiece.Position;
            textDiffPiece.Text = diffPiece.Text;
            textDiffPiece.Type = diffPiece.Type.ToString();
            if (diffPiece.SubPieces != null && diffPiece.SubPieces.Count > 0)
            {
                textDiffPiece.SubPieces = new List<TextDiffPiece>();
                foreach (DiffPiece diffPiece2 in diffPiece.SubPieces)
                {
                    TextDiffPiece textDiffPiece2 = new TextDiffPiece();
                    textDiffPiece.SubPieces.Add(textDiffPiece2);
                    PopulateDataItem(textDiffPiece2, diffPiece2);
                }
            }
        }
    }
}