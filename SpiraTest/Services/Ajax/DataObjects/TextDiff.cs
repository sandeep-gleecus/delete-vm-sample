using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    [DataContract(Namespace = "tst.dataObjects")]
    public class TextDiff
    {
        [DataMember(Name = "newText", EmitDefaultValue = false)]
        public TextDiffPane NewText { get; set; }

        [DataMember(Name = "oldText", EmitDefaultValue = false)]
        public TextDiffPane OldText { get; set; }
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class TextDiffPane
    {
        [DataMember(Name = "hasDifferences", EmitDefaultValue = false)]
        public bool HasDifferences { get; set; }

        [DataMember(Name = "lines", EmitDefaultValue = false)]
        public List<TextDiffPiece> Lines { get; set; }
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class TextDiffPiece
    {
        [DataMember(Name = "position", EmitDefaultValue = false)]
        public int? Position { get; set; }

        [DataMember(Name = "subPieces", EmitDefaultValue = false)]
        public List<TextDiffPiece> SubPieces { get; set; }

        [DataMember(Name = "text", EmitDefaultValue = false)]
        public string Text { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }
    }
}