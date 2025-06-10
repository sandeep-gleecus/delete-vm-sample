namespace Inflectra.OAuth2
{
    using System;

    public class ProviderType
    {
        /// <summary>Unique ID set by the provider.</summary>
        public Guid Identification { get; set; }

        /// <summary>Pretty name set by the provider.</summary>
        public string Name { get; set; }

        /// <summary>The description of the Provider. Used in Admin pages.</summary>
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            return (
                obj is ProviderType &&
                ((ProviderType)obj)?.Identification == Identification
            );
        }

        public override int GetHashCode()
        {
            return Identification.GetHashCode();
        }
    }
}
