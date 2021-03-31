using System;

namespace Semantika.Marcell.Data
{
    public interface IMarcellEntity
    {
        string Language { get; set; }
        string Id { get; set; }
        string Text { get; set; }
        Guid InternalId { get; set; }
        int TokenCount { get; set; }
        double RecognitionQuality { get; set; }
    }

    public interface IMarcellNestedEntity : IMarcellEntity
    {
        Guid ParentId { get; set; }
    }
}