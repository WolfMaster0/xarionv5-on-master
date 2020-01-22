// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.Core;
using OpenNos.Core.Threading;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace OpenNos.Bazaar.Server
{
    public class BazaarManager : IDisposable
    {
        #region Members

        private static BazaarManager _instance;

        private bool _disposed;

        #endregion

        #region Instantiation

        public BazaarManager()
        {
            // TODO: Implement autosave observable
            AuthentificatedClients = new List<long>();
            AuthentificationServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["AuthentificationServiceAuthKey"]);
        }

        #endregion

        #region Properties

        public static BazaarManager Instance => _instance ?? (_instance = new BazaarManager());

        public List<long> AuthentificatedClients { get; set; }

        public ThreadSafeSortedList<long, BazaarItemLink> BazaarList { get; set; }

        public bool InBazaarRefreshMode { get; set; }

        #endregion

        #region Methods

        public List<BazaarItemDTO> RetrieveItems() => null;

        public void AddToBazaar(BazaarItemDTO bazaarItem)
        {
            // we are not supposed to save each addition! we will only save them on Save() function.
            // that might be hard to overcome i guess...

            // we should never add more than 1 instance of iteminstance to bazaar!
            if (bazaarItem != null && !BazaarList.Any(s => s.BazaarItem.ItemInstanceId.Equals(bazaarItem.ItemInstanceId)))
            {
                ItemInstance item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bazaarItem.ItemInstanceId));
                string name = DAOFactory.CharacterDAO.LoadById(bazaarItem.SellerId)?.Name;
                if (item != null && !string.IsNullOrEmpty(name))
                {
                    BazaarList[bazaarItem.BazaarItemId] = new BazaarItemLink()
                    {
                        BazaarItem = bazaarItem,
                        Item = item,
                        OwnerName = name
                    };

                    // we will propably need to also update the iteminstance.
                }
            }
            else
            {
                // log or return something
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            BazaarList = new ThreadSafeSortedList<long, BazaarItemLink>();
            OrderablePartitioner<BazaarItemDTO> bazaarPartitioner = Partitioner.Create(DAOFactory.BazaarItemDAO.LoadAll(), EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(bazaarPartitioner, new ParallelOptions { MaxDegreeOfParallelism = 8 }, bazaarItem =>
            {
                BazaarItemLink item = new BazaarItemLink
                {
                    BazaarItem = bazaarItem
                };
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadById(bazaarItem.SellerId);
                if (chara != null)
                {
                    item.OwnerName = chara.Name;
                    item.Item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bazaarItem.ItemInstanceId));
                }
                BazaarList[item.BazaarItem.BazaarItemId] = item;
            });
            Logger.Info(string.Format(Language.Instance.GetMessageFromKey("BAZAAR_LOADED"), BazaarList.Count));
        }

        public void RefreshBazaar(long bazaarItemId)
        {
            BazaarItemDTO bzdto = DAOFactory.BazaarItemDAO.LoadById(bazaarItemId);
            BazaarItemLink bzlink = BazaarList[bazaarItemId];

            // maybe we shouldnt lock the entire list.
            lock (BazaarList)
            {
                if (bzdto != null)
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadById(bzdto.SellerId);
                    if (bzlink != null)
                    {
                        // why do we do this update? this is a copy of code from ServerManager
                        BazaarList.Remove(bzlink);
                        bzlink.BazaarItem = bzdto;
                        bzlink.OwnerName = chara.Name;
                        bzlink.Item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bzdto.ItemInstanceId));
                        BazaarList[bazaarItemId] = bzlink;
                    }
                    else
                    {
                        BazaarItemLink item = new BazaarItemLink
                        {
                            BazaarItem = bzdto
                        };
                        if (chara != null)
                        {
                            item.OwnerName = chara.Name;
                            item.Item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bzdto.ItemInstanceId));
                        }
                        BazaarList[bazaarItemId] = item;
                    }
                }
                else if (bzlink != null)
                {
                    BazaarList.Remove(bzlink);
                }
            }
            InBazaarRefreshMode = false;
        }

        public void RemoveFromBazaar(long bazaarItemId)
        {
            // we are not supposed to save each removal! we will only save them on Save() function.

            // there should never be 2 objects with same id in the bazaar!
            BazaarItemLink bazaaritem = BazaarList.SingleOrDefault(s => s.BazaarItem.BazaarItemId.Equals(bazaarItemId));
            if (bazaaritem != null)
            {
                BazaarList.Remove(bazaaritem);

                // we will propably need to also update the iteminstance.
            }
            else
            {
                // something is wrong, log as suspicious activity or some kind of weird behavior. the
                // item if sent by game should always be in the BazaarList, unless it was somehow
                // bought before the client got updated info.
            }
        }

        // this SHOULD return null if not found or if more that 1 item found!
        public BazaarItemLink RetrieveItemLink(long bazaarItemId) => BazaarList[bazaarItemId];

        public void Save()
        {
            // this will need extensive testing, it needs to be the most stable piece of code in the
            // entire project!

            // add
            foreach (BazaarItemLink link in BazaarList.GetAllItems())
            {
                if (DAOFactory.BazaarItemDAO.LoadById(link.BazaarItem.BazaarItemId) == null)
                {
                    BazaarItemDTO item = link.BazaarItem;
                    DAOFactory.BazaarItemDAO.InsertOrUpdate(ref item);
                    DAOFactory.ItemInstanceDAO.InsertOrUpdate(link.Item);
                }
            }

            // remove
            foreach (BazaarItemDTO item in DAOFactory.BazaarItemDAO.LoadAll())
            {
                if (!BazaarList.Any(s => s.BazaarItem.ItemInstanceId.Equals(item.ItemInstanceId)))
                {
                    DAOFactory.BazaarItemDAO.Delete(item.BazaarItemId);
                    DAOFactory.ItemInstanceDAO.Delete(item.ItemInstanceId);
                }
            }
            DAOFactory.BazaarItemDAO.RemoveOutDated();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    BazaarList.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}