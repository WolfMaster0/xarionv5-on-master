// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace OpenNos.Core
{
    public sealed class Language
    {
        #region Members

        private static Language _instance;

        private readonly ResourceManager _manager;

        private readonly CultureInfo _resourceCulture;

        private readonly StreamWriter _streamWriter;

        #endregion

        #region Instantiation

        private Language()
        {
            try
            {
                _streamWriter = new StreamWriter("MissingLanguageKeys.txt", true)
                {
                    AutoFlush = true
                };
            }
            catch (IOException)
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                Logger.Warn("MissingLanguageKeys.txt was in use, but i was able to catch this exception", null, "LanguageKeys");
            }
            _resourceCulture = new CultureInfo(ConfigurationManager.AppSettings[nameof(Language)]);
            if (Assembly.GetEntryAssembly() != null)
            {
                _manager = new ResourceManager(Assembly.GetEntryAssembly().GetName().Name + ".Resource.LocalizedResources", Assembly.GetEntryAssembly());
            }
        }

        #endregion

        #region Properties

        public static Language Instance => _instance ?? (_instance = new Language());

        #endregion

        #region Methods

        public string GetMessageFromKey(string message)
        {
            string resourceMessage = _manager != null ? _manager.GetString(message, _resourceCulture) : string.Empty;

            if (string.IsNullOrEmpty(resourceMessage))
            {
                _streamWriter?.WriteLine(message);
                return $"{message}";
            }

            return resourceMessage;
        }

        #endregion
    }
}