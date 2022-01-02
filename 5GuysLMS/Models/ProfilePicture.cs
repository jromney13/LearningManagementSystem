using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Models
{
    public class ProfilePicture
    {
        public int Id { get; set; }
        public string ImageTitle { get; set; }
        public string FileExtension { get; set; }
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Gets Image URL from raw image data. use as the src of an img tag and it will display the image. 
        /// </summary>
        /// <returns>Base64 ImageData URL</returns>
        public string GenerateURL()
        {
            //remove period from file extension
            string strippedFileExtension = FileExtension.Replace(".", string.Empty);
            //convert image data to base64 string
            string imageBase64Data = Convert.ToBase64String(ImageData);
            //format url using our new converted data and our saved file extension
            string imageDataURL = string.Format("data:image/" + strippedFileExtension + ";base64,{0}", imageBase64Data);
            //return url
            return imageDataURL;
        }
    }
}
