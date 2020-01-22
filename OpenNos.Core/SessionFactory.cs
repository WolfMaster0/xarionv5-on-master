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
namespace OpenNos.Core
{
    public sealed class SessionFactory
    {
        #region Members

        private static SessionFactory _instance;

        private int _sessionCounter;

        #endregion

        #region Instantiation

        private SessionFactory()
        {
        }

        #endregion

        #region Properties

        public static SessionFactory Instance => _instance ?? (_instance = new SessionFactory());

        #endregion

        #region Methods

        public int GenerateSessionId()
        {
            _sessionCounter += 2;
            return _sessionCounter;
        }

        #endregion
    }
}