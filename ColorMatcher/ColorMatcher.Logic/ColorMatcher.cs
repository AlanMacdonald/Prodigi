using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;

namespace ColorMatcher.Logic
{
    public interface IColorMatcher
    {
        (bool MatchFound, string ColorNameFromCatalogue, Color? Color) MatchPixel(Bitmap image, int x, int y, int RgbFuzziness);
    }

    public class ColorMatcher : IColorMatcher
    {
        /// <summary>
        /// Supporting class to make fuzzy searching easier
        /// </summary>
        private class CandidateColor
        {
            internal decimal AverageRgbDeviation { get; private set; }
            internal int RDeviation { get; private set; }
            internal int GDeviation { get; private set; }
            internal int BDeviation { get; private set; }

            internal string ColorNameFromCatalogue { get; private set; }
            internal Color Color { get; private set; }

            internal CandidateColor(string colorNameFromCatalogue, Color color, Color colorToMatch)
            {
                (ColorNameFromCatalogue, Color) = (colorNameFromCatalogue, color);

                RDeviation = Math.Abs(Color.R - colorToMatch.R);
                GDeviation = Math.Abs(Color.G - colorToMatch.G);
                BDeviation = Math.Abs(Color.B - colorToMatch.B);
                AverageRgbDeviation = (RDeviation + GDeviation + BDeviation) / 3;
            }
        }

        IColorCatalogue colorCatalogue;


        public ColorMatcher(IColorCatalogue colorCatalogue)
        {
            this.colorCatalogue = colorCatalogue;
        }

        /// <summary>
        /// Matches the color of the specified pixel position of the image to a color from the color catalog within the allowed rgb fuzziness.
        /// </summary>
        /// <param name="image">The image to color match against</param>
        /// <param name="x">The x position of the pixel to be color sampled</param>
        /// <param name="y">The y position of the pixel to be color sampled</param>
        /// <param name="rgbFuzziness">Allowed RGB variance e.g a fuzziness of 2 means (122, 118, 121) can match (120, 120, 120) since ALL R, G and B values are withing 2.</param>
        /// <returns>A tuple indicating successs and if success the nameo of the color and color itself from the color backlog</returns>
        public (bool MatchFound, string ColorNameFromCatalogue, Color? Color) MatchPixel(Bitmap image, int x, int y, int rgbFuzziness)
        {
            var pixel = image.GetPixel(x, y);
            var colorToMatchArgb = pixel.ToArgb();

            //Try and find an exact color match using O(1) dictionary lookup
            if (colorCatalogue.CatalogueByArgb.ContainsKey(colorToMatchArgb))
            {
                var match = colorCatalogue.CatalogueByArgb[colorToMatchArgb];

                return (MatchFound: true, ColorNameFromCatalogue: match.ColorNameFromCatalogue, Color: match.Color);
            }
            else
            {
                return FuzzySearch(rgbFuzziness, colorToMatchArgb);
            }
        }

        private (bool MatchFound, string ColorNameFromCatalogue, Color? Color) FuzzySearch(int rgbFuzziness, int colorToMatchArgb)
        {
            if (rgbFuzziness > 0)
            {
                //Perform fuzzy search to find a close number where all RGB values are within the RgbFuzziness range from a catalogue color 
                var colorToMatch = Color.FromArgb(colorToMatchArgb);
                var rgbTotal = colorToMatch.R + colorToMatch.G + colorToMatch.B;
                var totalAllowedFuzzyVariance = 3 * rgbFuzziness;

                //Rapidly narrow down the catalogue using the key to only the colors with rgb totals close to what we're interested in
                //This doesn't mean they neccessarily qualify but should be a fast way to discard most of the catalogue
                var groupsOfInterest = colorCatalogue.CatalogueByRgbSum
                                                    .Where(group => Math.Abs(group.Key - rgbTotal) <= totalAllowedFuzzyVariance)
                                                    .ToList();

                var candidateColors = new List<CandidateColor>();
                foreach (var group in groupsOfInterest)
                {
                    //Just because the total of the rgbs is close doesn't mean each of the values is close enough
                    //so now make sure they are
                    var qualifyingCanidates = group.Select(candidate => new CandidateColor(
                                                                                colorNameFromCatalogue: candidate.ColorNameFromCatalogue,
                                                                                color: candidate.Color,
                                                                                colorToMatch: colorToMatch
                                                                                )
                                                            )
                                                            .Where(candidate =>
                                                                        candidate.RDeviation <= rgbFuzziness &&
                                                                        candidate.GDeviation <= rgbFuzziness &&
                                                                        candidate.BDeviation <= rgbFuzziness
                                                                   )
                                                                   .ToList();

                    candidateColors.AddRange(qualifyingCanidates);
                }

                var bestCandidate = candidateColors.OrderBy(candidate => candidate.AverageRgbDeviation)
                                                    .FirstOrDefault();

                if (bestCandidate != null)
                    return (MatchFound: true, ColorNameFromCatalogue: bestCandidate.ColorNameFromCatalogue, Color: bestCandidate.Color);
            }

            return (MatchFound: false, ColorNameFromCatalogue: null, Color: null);
        }
    }
}
