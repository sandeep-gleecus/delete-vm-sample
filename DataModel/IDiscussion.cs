using System;
namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// The interface all the discussion entities need to implement
    /// </summary>
    public interface IDiscussion
    {
        int ArtifactId { get; set; }
        DateTime CreationDate { get; set; }
        int CreatorId { get; set; }
        int DiscussionId { get; set; }
        bool IsDeleted { get; set; }
        bool IsPermanent { get; set; }
        string Text { get; set; }
        string CreatorName { get; }
    }
}
