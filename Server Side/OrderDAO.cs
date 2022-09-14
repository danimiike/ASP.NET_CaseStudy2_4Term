using Casestudy.Helpers;
using Casestudy.DAL.DomainClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Casestudy.DAL.DAO
{
    public class OrderDAO
    {
        private AppDbContext _db;
        public OrderDAO(AppDbContext ctx)
        {
            _db = ctx;
        }
        public async Task<int> AddOrder(int userid, OrderSelectionHelper[] selections)
        {
            int orderId = -1;


            using (_db)
            {
                // we need a transaction as multiple entities involved
                using (var _trans = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Order order = new Order();
                        order.UserId = userid;
                        order.OrderDate = System.DateTime.Now;
                        order.OrderAmount = 0;

                        foreach (OrderSelectionHelper selection in selections)
                        {
                            order.OrderAmount += selection.item.MSRP * selection.Qty;

                        }
                        await _db.Orders.AddAsync(order);
                        await _db.SaveChangesAsync();

                        // then add each item to the trayitems table
                        foreach (OrderSelectionHelper selection in selections)
                        {
                            OrderLineItem tItem = new OrderLineItem();

                            if (selection.Qty <= selection.item.QtyOnHand)
                            {
                                tItem.QtySold = selection.Qty;
                                tItem.QtyOrdered = selection.Qty;
                                tItem.QtyBackOrdered = 0;
                                selection.item.QtyOnHand = selection.item.QtyOnHand - selection.Qty;

                            }
                            else if (selection.Qty > selection.item.QtyOnHand)
                            {
                                tItem.QtySold = selection.item.QtyOnHand;
                                selection.item.QtyOnHand = 0;

                                selection.item.QtyOnBackOrder += selection.Qty - selection.item.QtyOnHand;

                                tItem.QtyOrdered = selection.Qty;
                                tItem.QtyBackOrdered = selection.Qty - tItem.QtySold;

                            }
                            tItem.SellingPrice = selection.item.MSRP * selection.Qty;
                            tItem.ProductId = selection.item.Id;
                            tItem.OrderId = order.Id;

                            _db.Product.Update(selection.item);


                            await _db.OrderItems.AddAsync(tItem);
                            await _db.SaveChangesAsync();

                        }
                        await _trans.CommitAsync();
                        orderId = order.Id;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        await _trans.RollbackAsync();
                    }
                }
            }
            return orderId;
        }
        public async Task<List<Order>> GetAll(int id)
        {
            return await _db.Orders.Where(order => order.UserId == id).ToListAsync<Order>();
        }

        public async Task<List<OrderDetailsHelper>> GetOrderDetails(int tid, string email)
        {
           Customer user = _db.Customers.FirstOrDefault(user => user.Email == email);
            List<OrderDetailsHelper> allDetails = new List<OrderDetailsHelper>();
            // LINQ way of doing INNER JOINS
            var results = from o in _db.Orders
                          join oi in _db.OrderItems on o.Id equals oi.OrderId
                          join p in _db.Product on oi.ProductId equals p.Id
                          where (o.UserId == user.Id && o.Id == tid)
                          select new OrderDetailsHelper
                          {
                              OrderId = o.Id,
                              UserId = user.Id,
                              Name = p.ProductName,
                              QtyS = oi.QtySold,
                              QtyO = oi.QtyOrdered,
                              QtyB = oi.QtyBackOrdered,
                              Description = p.Description,
                              OrderItemId = oi.ProductId,
                              Extended = oi.SellingPrice,
                              DateCreated = o.OrderDate.ToString("yyyy/MM/dd - hh:mm tt")

                          };
            allDetails = await results.ToListAsync<OrderDetailsHelper>();
            return allDetails;
        }
    }
}