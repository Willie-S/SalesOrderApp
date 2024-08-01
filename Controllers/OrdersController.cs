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
    private readonly XmlSalesOrderRepository _xmlSalesOrderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrdersController(SalesOrderRepository salesOrderRepository, XmlSalesOrderRepository xmlSalesOrderRepository, IHttpContextAccessor httpContextAccessor)
    {
        _salesOrderRepository = salesOrderRepository;
        _xmlSalesOrderRepository = xmlSalesOrderRepository;
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

    private bool IsAdmin()
    {
        return User.IsInRole(UserRoleEnum.Admin.ToString());
    }

    public async Task<IActionResult> Index(int? selectedOrderId = null)
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

            if (selectedOrderId.HasValue && selectedOrderId.Value > 0) 
            {
                viewModel.SelectedSalesOrder = salesOrders.FirstOrDefault(so => so.Id == selectedOrderId.Value);
            }

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

    [HttpGet]
    public async Task<IActionResult> CreateOrder()
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            var viewModel = new OrdersAddViewModel
            {
                OrderHeader = new OrderHeaderViewModel(),
                OrderLines = new List<OrderLineViewModel>()
            };

            return PartialView("_AddOrderFormPartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to create an order.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order line for creating. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrdersAddViewModel model)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

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

            // Commit the changes to the SQL DB
            var savedOrder = await _salesOrderRepository.AddAsync(newOrder);
            await _salesOrderRepository.ReassignLineNumbersAsync(savedOrder.Id);

            // Commit the changes to the XML DB
            var xmlSavedOrder = await _xmlSalesOrderRepository.AddAsync(newOrder);
            await _xmlSalesOrderRepository.ReassignLineNumbersAsync(xmlSavedOrder.Id);

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

    [HttpGet]
    public async Task<IActionResult> CreateLine(int id)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            var userId = GetCurrentUserId();

            var viewModel = new OrdersAddLineViewModel
            {
                SalesOrderId = id,
                OrderLine = new OrderLineViewModel()
            };

            return PartialView("_AddOrderLineFormPartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to create an order line.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order line for creating. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateLine(OrdersAddLineViewModel model)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddOrderFormPartial", model);
            }

            var userId = GetCurrentUserId();

            var newOrderLine = new OrderLine
            {
                ProductCode = model.OrderLine.ProductCode,
                ProductTypeId = (int)model.OrderLine.ProductType,
                CostPrice = model.OrderLine.CostPrice,
                SalesPrice = model.OrderLine.SalesPrice,
                Quantity = model.OrderLine.Quantity
            };

            // Commit the changes to the SQL DB
            await _salesOrderRepository.AddOrderLineAsync(model.SalesOrderId, newOrderLine, userId);
            await _salesOrderRepository.ReassignLineNumbersAsync(model.SalesOrderId);

            // Commit the changes to the XML DB
            await _xmlSalesOrderRepository.AddOrderLineAsync(model.SalesOrderId, newOrderLine, userId);
            await _xmlSalesOrderRepository.ReassignLineNumbersAsync(model.SalesOrderId);

            return RedirectToAction("Index", "Orders", new { selectedOrderId = model.SalesOrderId });
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

    [HttpPost]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            var userId = GetCurrentUserId();
            var order = await _salesOrderRepository.GetByIdAsync(id);

            if (order == null || order.CreatedByUserId != userId)
            {
                return NotFound("Order not found or you are not authorized to delete this order.");
            }

            // Commit the changes to the SQL DB
            await _salesOrderRepository.DeleteAsync(id);

            // Commit the changes to the XML DB
            await _xmlSalesOrderRepository.DeleteAsync(id);

            var salesOrders = await _salesOrderRepository.GetAllByUserIdAsync(userId);

            var viewModel = new OrdersViewModel
            {
                SalesOrders = salesOrders.ToList(),
                SelectedSalesOrder = null
            };

            return PartialView("_OrderTablesPartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to delete this order.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the order. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteLine(int id)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            var userId = GetCurrentUserId();

            // Commit the changes to the SQL DB
            var deletedLine = await _salesOrderRepository.DeleteOrderLineAsync(id, userId);
            await _salesOrderRepository.ReassignLineNumbersAsync(deletedLine.SalesOrderId);

            // Commit the changes to the XML DB
            var xmlDeletedLine = await _xmlSalesOrderRepository.DeleteOrderLineAsync(id, userId);
            await _xmlSalesOrderRepository.ReassignLineNumbersAsync(xmlDeletedLine.SalesOrderId);

            var salesOrders = await _salesOrderRepository.GetAllByUserIdAsync(userId);
            var selectedOrder = deletedLine?.SalesOrderId > 0 ? salesOrders.FirstOrDefault(o => o.Id == deletedLine.SalesOrderId) : null;

            var viewModel = new OrdersViewModel
            {
                SalesOrders = salesOrders.ToList(),
                SelectedSalesOrder = selectedOrder
            };

            return PartialView("_OrderTablesPartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to delete this order line.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the order line. Please try again later.");
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> EditOrderHeader(int id)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            var userId = GetCurrentUserId();
            var order = await _salesOrderRepository.GetByIdAsync(id);

            if (order == null || order.CreatedByUserId != userId)
            {
                return NotFound("Order not found or you are not authorized to edit this order.");
            }

            var viewModel = new OrdersEditHeaderViewModel
            {
                SalesOrderId = id,
                OrderHeaderId = order.OrderHeader.Id,
                OrderHeader = new OrderHeaderViewModel
                {
                    OrderNumber = order.OrderHeader.OrderNumber,
                    OrderType = (OrderTypeEnum)order.OrderHeader.OrderTypeId,
                    OrderStatus = (OrderStatusEnum)order.OrderHeader.OrderStatusId,
                    CustomerName = order.OrderHeader.CustomerName,
                    CreateDate = order.OrderHeader.CreateDate
                }
            };

            return PartialView("_EditOrderHeaderPartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to edit this order header.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order header for editing. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditOrderHeader(OrdersEditHeaderViewModel model)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditOrderHeaderPartial", model);
            }

            var userId = GetCurrentUserId();
            var order = await _salesOrderRepository.GetByIdAsync(model.SalesOrderId);

            if (order == null || order.CreatedByUserId != userId)
            {
                return NotFound("Order not found or you are not authorized to update this order.");
            }

            order.OrderHeader.OrderNumber = model.OrderHeader.OrderNumber;
            order.OrderHeader.OrderTypeId = (int)model.OrderHeader.OrderType;
            order.OrderHeader.OrderStatusId = (int)model.OrderHeader.OrderStatus;
            order.OrderHeader.CustomerName = model.OrderHeader.CustomerName;
            order.OrderHeader.CreateDate = model.OrderHeader.CreateDate;

            // Commit the changes to the SQL DB
            await _salesOrderRepository.UpdateAsync(model.SalesOrderId, order.OrderHeader, userId);

            // Commit the changes to the XML DB
            await _xmlSalesOrderRepository.UpdateAsync(model.SalesOrderId, order.OrderHeader, userId);

            return RedirectToAction("Index", "Orders", new { selectedOrderId = order.Id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to update this order header.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order header. Please try again later.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditOrderLine(int id)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            var userId = GetCurrentUserId();
            var line = await _salesOrderRepository.GetOrderLineByIdAsync(id);

            if (line == null)
            {
                return NotFound("Order not found or you are not authorized to edit this order.");
            }

            var viewModel = new OrdersEditLineViewModel
            {
                SalesOrderId = line.SalesOrderId,
                OrderLineId = id,
                OrderLine = new OrderLineViewModel
                {
                    ProductCode = line.ProductCode,
                    ProductType = (ProductTypeEnum)line.ProductTypeId,
                    CostPrice = line.CostPrice,
                    SalesPrice = line.SalesPrice,
                    Quantity = line.Quantity
                }
            };

            return PartialView("_EditOrderLinePartial", viewModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to edit this order line.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order line for editing. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditOrderLine(OrdersEditLineViewModel model)
    {
        if (!IsAdmin())
        {
            TempData["ErrorMessage"] = "You are not authorized for this action.";
            return Unauthorized("You are not authorized for this action.");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditOrderLinePartial", model);
            }

            var userId = GetCurrentUserId();
            var order = await _salesOrderRepository.GetByIdAsync(model.SalesOrderId);

            if (order == null || order.CreatedByUserId != userId)
            {
                return NotFound("Order not found or you are not authorized to update this order.");
            }

            var line = order.OrderLines.FirstOrDefault(ol => ol.Id == model.OrderLineId);

            line.ProductCode = model.OrderLine.ProductCode;
            line.ProductTypeId = (int)model.OrderLine.ProductType;
            line.CostPrice = model.OrderLine.CostPrice;
            line.SalesPrice = model.OrderLine.SalesPrice;
            line.Quantity = model.OrderLine.Quantity;

            // Commit the changes to the SQL DB
            await _salesOrderRepository.UpdateOrderLineAsync(line, userId);

            // Commit the changes to the XML DB
            await _xmlSalesOrderRepository.UpdateOrderLineAsync(line, userId);

            return RedirectToAction("Index", "Orders", new { selectedOrderId = order.Id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized("You are not authorized to update this order line.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order line. Please try again later.");
        }
    }
}
