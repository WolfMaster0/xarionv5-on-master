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
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using System.Threading.Tasks;

namespace OpenNos.Master.Library.Client
{
    internal class CommunicationClient : ICommunicationClient
    {
        #region Methods

        public void CharacterConnected(long characterId) => Task.Run(() => CommunicationServiceClient.Instance.OnCharacterConnected(characterId));

        public void CharacterDisconnected(long characterId) => Task.Run(() => CommunicationServiceClient.Instance.OnCharacterDisconnected(characterId));

        public void KickSession(long? accountId, int? sessionId) => Task.Run(() => CommunicationServiceClient.Instance.OnKickSession(accountId, sessionId));

        public void Restart() => Task.Run(() => CommunicationServiceClient.Instance.OnRestart());

        public void RunGlobalEvent(Domain.EventType eventType) => Task.Run(() => CommunicationServiceClient.Instance.OnRunGlobalEvent(eventType));

        public void SendMessageToCharacter(ScsCharacterMessage message) => Task.Run(() => CommunicationServiceClient.Instance.OnSendMessageToCharacter(message));

        public void Shutdown() => Task.Run(() => CommunicationServiceClient.Instance.OnShutdown());

        public void UpdateBazaar(long bazaarItemId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateBazaar(bazaarItemId));

        public void UpdateFamily(long familyId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateFamily(familyId));

        public void UpdatePenaltyLog(int penaltyLogId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdatePenaltyLog(penaltyLogId));

        public void UpdateRelation(long relationId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateRelation(relationId));

        public void UpdateStaticBonus(long characterId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateStaticBonus(characterId));

        #endregion
    }
}