using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TFGUserControls.Services
{
    /// <summary>
    /// An shared memory resource used to optimize the use of card's images. By this implementation, every single card will
    /// reference to a single BitmapImage, wich should lower the amount of memory used by the program.
    /// </summary>
    public static class ImageManager
    {
        private static Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();

        public static BitmapImage GetImage(string imageUrl)
        {
            if (!imageCache.ContainsKey(imageUrl))
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(imageUrl, UriKind.RelativeOrAbsolute));
                imageCache.Add(imageUrl, bitmapImage);
            }

            return imageCache[imageUrl];
        }
    }
}