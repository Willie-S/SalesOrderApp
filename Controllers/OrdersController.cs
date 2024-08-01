using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;
using SalesOrderApp.ViewModels;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SalesOrderApp.Repositories;

public class OrdersController : Controller
{
    private readonly SalesOrderRepository _salesOrderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrdersController(SalesOrderRepository salesOrderRepository, IHttpContextAccessor httpContextAccessor)
    {
        _salesOrderRepository = salesOrderRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID could not be determined.");
        }
        return userId;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var salesOrders = await _salesOrderRepository.GetAllByUserIdAsync(userId);
            var viewModel = new OrdersViewModel
            {
                SalesOrders = salesOrders.ToList(),
                SelectedSalesOrder = null
            };

            return View(viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Log exception and show error message
            // Log.Error(ex, "Unauthorized access while fetching orders.");
            TempData["ErrorMessage"] = "You are not authorized to access this resource.";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // Log exception and show error message
            // Log.Error(ex, "An error occurred while fetching orders.");
            TempData["ErrorMessage"] = "An error occurred while retrieving orders. Please try again later.";
            return RedirectToAction("Index", "Orders");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSelectedOrder([FromBody] int selectedOrderId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var salesOrders = await _salesOrderRepository.GetAllByUserIdAsync(userId);
            var selectedOrder = salesOrders.FirstOrDefault(o => o.Id == selectedOrderId);

            if (selectedOrderId > 0 && selectedOrder == null) return BadRequest("Selected order not found.");

            var viewModel = new OrdersViewModel
            {
                SalesOrders = salesOrders.ToList(),
                SelectedSalesOrder = selectedOrder
            };

            return PartialView("_OrderTablesPartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Log exception and show error message
            // Log.Error(ex, "Unauthorized access while updating selected order.");
            return Unauthorized("User is not authorized to update order.");
        }
        catch (Exception ex)
        {
            // Log exception and show error message
            // Log.Error(ex, "An error occurred while updating the selected order.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrdersAddViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddOrderFormPartial", model);
            }

            var userId = GetCurrentUserId();

            var newOrder = new SalesOrder
            {
                CreatedByUserId = userId,
                UpdatedByUserId = userId,
                OrderHeader = new OrderHeader
                {
                    OrderNumber = model.OrderHeader.OrderNumber,
                    OrderTypeId = (int)model.OrderHeader.OrderType,
                    OrderStatusId = (int)model.OrderHeader.OrderStatus,
                    CustomerName = model.OrderHeader.CustomerName,
                    CreateDate = model.OrderHeader.CreateDate
                },
                OrderLines = model.OrderLines.Select(line => new OrderLine
                {
                    ProductCode = line.ProductCode,
                    ProductTypeId = (int)line.ProductType,
                    CostPrice = line.CostPrice,
                    SalesPrice = line.SalesPrice,
                    Quantity = line.Quantity
                }).ToList()
            };

            var savedOrder = await _salesOrderRepository.AddAsync(newOrder);
            await _salesOrderRepository.ReassignLineNumbersAsync(savedOrder.Id);

            var salesOrders = await _salesOrderRepository.GetAllByUserIdAsync(userId);

            var viewModel = new OrdersViewModel
            {
                SalesOrders = salesOrders.ToList(),
                SelectedSalesOrder = null
            };

            return RedirectToAction("Index", "Orders");
        }
        catch (UnauthorizedAccessException ex)
        {
            // Log exception and show error message
            TempData["ErrorMessage"] = "You are not authorized to add orders.";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // Log exception and show error message
            TempData["ErrorMessage"] = "An error occurred while adding the order. Please try again later.";
            return RedirectToAction("Index", "Orders");
        }
    }
}
