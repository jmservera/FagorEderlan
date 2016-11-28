using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    public class Utils
    {
        /// <summary>
        /// Checks if Internet connection is available or not.
        /// </summary>
        /// <returns>Returns true if Internet connection is available, otherwise returns false.</returns>
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("https://portal.azure.com"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
