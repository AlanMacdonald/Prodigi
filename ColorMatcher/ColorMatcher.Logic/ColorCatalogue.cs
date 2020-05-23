using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ColorMatcher.Logic
{
    public interface IColorCatalogue
    {
        IReadOnlyDictionary<int, (string ColorNameFromCatalogue, Color Color)> CatalogueByArgb { get; }
        ILookup<int, (string ColorNameFromCatalogue, Color Color)> CatalogueByRgbSum { get; }
    }

    /// <summary>
    /// Catalogue of named colors that are supported
    /// </summary>
    public class ColorCatalogue : IColorCatalogue
    {
        
        private Dictionary<string, ColorWrapper> Catalogue
        {
            get => catalogue;
            set
            {
                catalogue = value;
                CatalogueByArgb = catalogue.ToDictionary(kvp => kvp.Value.Color.ToArgb(), kvp => (ColorNameFromCatalogue: kvp.Key, Color: kvp.Value.Color));
                CatalogueByRgbSum = catalogue.ToLookup(kvp => kvp.Value.Color.R + kvp.Value.Color.G + kvp.Value.Color.B, kvp => (ColorNameFromCatalogue: kvp.Key, Color: kvp.Value.Color));
            }
        }

        private Dictionary<string, ColorWrapper> catalogue;

        /// <summary>
        /// Supports finding a color by Argb with O(1) efficiency.  In effect a reverse lookup when you know the exact rgb but not the color name.
        /// </summary>
        public IReadOnlyDictionary<int, (string ColorNameFromCatalogue, Color Color)> CatalogueByArgb { get; private set; }

        /// <summary>
        /// Supports finding a color by Rgb sum.
        /// More than one color can exist in a group within the lookup.
        /// This requires further inspection to ensure the color is really as desired but facilitates fuzzy searching
        /// </summary>
        public ILookup<int, (string ColorNameFromCatalogue, Color Color)> CatalogueByRgbSum { get; private set; }

        public ColorCatalogue(Dictionary<string, ColorWrapper> catalogue)
        {
            this.Catalogue = catalogue;
        }

        public ColorCatalogue(string filePath)
        {
            var json = System.IO.File.ReadAllText("ColorCatalogue.txt");
            var jsonDeserializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, ColorWrapper>>(json);
            this.Catalogue = dictionary;
            
        }
    }
}
