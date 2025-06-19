using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub.Services
{
    public class InventoryService
    {
        private readonly DB _db = new DB();

        public List<InventoryItem> GetAllInventory() =>
            _db.GetAllInventory();

        public bool CreateInventory(InventoryItem item) =>
            _db.CreateInventory(
                item.Name,
                item.Quantity,
                item.Status,
                item.LastMaintenance,
                item.PurchaseDate,
                item.ResponsibleId
            );

        public bool UpdateInventory(InventoryItem item) =>
            _db.UpdateInventory(
                item.InventoryId,
                item.Name,
                item.Quantity,
                item.Status,
                item.LastMaintenance,
                item.PurchaseDate,
                item.ResponsibleId
            );

        public bool DeleteInventory(int id) =>
            _db.DeleteInventory(id);
    }
}
