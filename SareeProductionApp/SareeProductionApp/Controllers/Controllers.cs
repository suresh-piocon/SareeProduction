using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SareeProductionApp.Client.Models;
using SareeProductionApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SareeProductionApp.Controllers
{
    // ============================================================================
    // 1. Job Workers Controller
    // ============================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class JobWorkersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobWorkersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobWorker>>> GetJobWorkers()
        {
            return await _context.JobWorkers.OrderBy(w => w.Name).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobWorker>> GetJobWorker(Guid id)
        {
            var worker = await _context.JobWorkers.FindAsync(id);
            if (worker == null) return NotFound();
            return worker;
        }

        [HttpPost]
        public async Task<ActionResult<JobWorker>> CreateJobWorker(JobWorker worker)
        {
            worker.Id = Guid.NewGuid();
            worker.CreatedAt = DateTime.UtcNow;
            worker.UpdatedAt = DateTime.UtcNow;
            _context.JobWorkers.Add(worker);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetJobWorker), new { id = worker.Id }, worker);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobWorker(Guid id, JobWorker worker)
        {
            if (id != worker.Id) return BadRequest();
            
            var existing = await _context.JobWorkers.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = worker.Name;
            existing.WorkerType = worker.WorkerType;
            existing.Place = worker.Place;
            existing.CellNo = worker.CellNo;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobWorker(Guid id)
        {
            var worker = await _context.JobWorkers.FindAsync(id);
            if (worker == null) return NotFound();

            // Soft delete
            worker.DeletedAt = DateTime.UtcNow;
            _context.Entry(worker).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // ============================================================================
    // 2. Yarn & Stock Controller
    // ============================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class YarnController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public YarnController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<YarnMaster>>> GetYarns()
        {
            return await _context.YarnMasters.OrderBy(y => y.YarnType).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<YarnMaster>> GetYarn(Guid id)
        {
            var yarn = await _context.YarnMasters.FindAsync(id);
            if (yarn == null) return NotFound();
            return yarn;
        }

        [HttpPost]
        public async Task<ActionResult<YarnMaster>> CreateYarn(YarnMaster yarn)
        {
            yarn.Id = Guid.NewGuid();
            yarn.CreatedAt = DateTime.UtcNow;
            yarn.UpdatedAt = DateTime.UtcNow;
            _context.YarnMasters.Add(yarn);

            // Automatically initialize yarn stock to 0
            var stock = new YarnStock
            {
                Id = Guid.NewGuid(),
                YarnId = yarn.Id,
                Quantity = 0.000m,
                Qty = 0.000m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.YarnStocks.Add(stock);

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetYarn), new { id = yarn.Id }, yarn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateYarn(Guid id, YarnMaster yarn)
        {
            if (id != yarn.Id) return BadRequest();

            var existing = await _context.YarnMasters.FindAsync(id);
            if (existing == null) return NotFound();

            existing.YarnType = yarn.YarnType;
            existing.Color = yarn.Color;
            existing.Denier = yarn.Denier;
            existing.Description = yarn.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteYarn(Guid id)
        {
            var yarn = await _context.YarnMasters.FindAsync(id);
            if (yarn == null) return NotFound();

            yarn.DeletedAt = DateTime.UtcNow;
            _context.Entry(yarn).State = EntityState.Modified;

            // Soft delete corresponding stock
            var stock = await _context.YarnStocks.FirstOrDefaultAsync(s => s.YarnId == id);
            if (stock != null)
            {
                stock.DeletedAt = DateTime.UtcNow;
                _context.Entry(stock).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Stock APIs ---
        [HttpGet("stock")]
        public async Task<ActionResult<IEnumerable<YarnStock>>> GetStockList()
        {
            return await _context.YarnStocks
                .Include(s => s.Yarn)
                .OrderBy(s => s.Yarn!.YarnType)
                .ToListAsync();
        }

        [HttpPost("stock/adjust")]
        public async Task<IActionResult> AdjustStock([FromQuery] Guid yarnId, [FromQuery] decimal quantity, [FromQuery] decimal qty)
        {
            var stock = await _context.YarnStocks.FirstOrDefaultAsync(s => s.YarnId == yarnId);
            if (stock == null)
            {
                stock = new YarnStock
                {
                    Id = Guid.NewGuid(),
                    YarnId = yarnId,
                    Quantity = quantity,
                    Qty = qty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.YarnStocks.Add(stock);
            }
            else
            {
                stock.Quantity += quantity;
                stock.Qty += qty;
                stock.UpdatedAt = DateTime.UtcNow;
                _context.Entry(stock).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return Ok(stock);
        }

        [HttpPut("stock/{id}")]
        public async Task<IActionResult> UpdateStock(Guid id, YarnStock stock)
        {
            if (id != stock.Id) return BadRequest();

            var existing = await _context.YarnStocks.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Quantity = stock.Quantity;
            existing.Qty = stock.Qty;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("stock/reset-all")]
        public async Task<IActionResult> ResetAllStock()
        {
            var stocks = await _context.YarnStocks.Where(s => s.DeletedAt == null).ToListAsync();
            foreach (var stock in stocks)
            {
                stock.Quantity = 0.000m;
                stock.Qty = 0.000m;
                stock.UpdatedAt = DateTime.UtcNow;
                _context.Entry(stock).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // ============================================================================
    // 3. Dyeing (Delivery & Receipt) Controller
    // ============================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class DyeingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DyeingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Deliveries ---
        [HttpGet("deliveries")]
        public async Task<ActionResult<IEnumerable<DyeingYarnDelivery>>> GetDeliveries()
        {
            return await _context.DyeingYarnDeliveries
                .Include(d => d.JobWorker)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Yarn)
                .OrderByDescending(d => d.DeliveryDate)
                .ToListAsync();
        }

        [HttpPost("deliveries")]
        public async Task<ActionResult<DyeingYarnDelivery>> CreateDelivery(DyeingYarnDelivery delivery)
        {
            delivery.Id = Guid.NewGuid();
            delivery.CreatedAt = DateTime.UtcNow;
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.Status = "Pending";

            foreach (var item in delivery.Items)
            {
                item.Id = Guid.NewGuid();
                item.DeliveryId = delivery.Id;
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                // Adjust yarn stock: Subtract issued raw yarn
                var stock = await _context.YarnStocks.FirstOrDefaultAsync(s => s.YarnId == item.YarnId);
                if (stock != null)
                {
                    stock.Quantity -= item.QuantityKg;
                    stock.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(stock).State = EntityState.Modified;
                }
            }

            _context.DyeingYarnDeliveries.Add(delivery);
            await _context.SaveChangesAsync();
            return Ok(delivery);
        }

        // --- Receipts ---
        [HttpGet("receipts")]
        public async Task<ActionResult<IEnumerable<DyeingYarnReceipt>>> GetReceipts()
        {
            return await _context.DyeingYarnReceipts
                .Include(r => r.JobWorker)
                .Include(r => r.Delivery)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Yarn)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }

        [HttpPost("receipts")]
        public async Task<ActionResult<DyeingYarnReceipt>> CreateReceipt(DyeingYarnReceipt receipt)
        {
            receipt.Id = Guid.NewGuid();
            receipt.CreatedAt = DateTime.UtcNow;
            receipt.UpdatedAt = DateTime.UtcNow;

            foreach (var item in receipt.Items)
            {
                item.Id = Guid.NewGuid();
                item.ReceiptId = receipt.Id;
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                // Adjust yarn stock: Add received dyed yarn back
                var stock = await _context.YarnStocks.FirstOrDefaultAsync(s => s.YarnId == item.YarnId);
                if (stock != null)
                {
                    stock.Quantity += item.QuantityKg;
                    stock.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(stock).State = EntityState.Modified;
                }
            }

            _context.DyeingYarnReceipts.Add(receipt);

            // Update associated Delivery status
            if (receipt.DeliveryId.HasValue)
            {
                var delivery = await _context.DyeingYarnDeliveries.FindAsync(receipt.DeliveryId.Value);
                if (delivery != null)
                {
                    delivery.Status = "Received";
                    delivery.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(delivery).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(receipt);
        }
    }

    // ============================================================================
    // 4. Weaving & Production Controller
    // ============================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class WeavingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WeavingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Warp Beams ---
        [HttpGet("beams")]
        public async Task<ActionResult<IEnumerable<WarpBeamMaster>>> GetBeams()
        {
            return await _context.WarpBeamMasters
                .Include(b => b.Yarn)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        [HttpPost("beams")]
        public async Task<ActionResult<WarpBeamMaster>> CreateBeam(WarpBeamMaster beam)
        {
            beam.Id = Guid.NewGuid();
            beam.CreatedAt = DateTime.UtcNow;
            beam.UpdatedAt = DateTime.UtcNow;
            beam.Status = "Available";

            // Subtract yarn used for preparing the beam (simulated as subtracting from warp yarn stock)
            var stock = await _context.YarnStocks.FirstOrDefaultAsync(s => s.YarnId == beam.YarnId);
            if (stock != null)
            {
                // Simple assumption: 1 meter of warp uses some yarn (e.g. 0.05 kg per meter for illustration)
                stock.Quantity -= (beam.TotalMeters * 0.05m);
                stock.UpdatedAt = DateTime.UtcNow;
                _context.Entry(stock).State = EntityState.Modified;
            }

            _context.WarpBeamMasters.Add(beam);
            await _context.SaveChangesAsync();
            return Ok(beam);
        }

        // --- Weaver Issues ---
        [HttpGet("issues")]
        public async Task<ActionResult<IEnumerable<WeaverIssue>>> GetIssues()
        {
            return await _context.WeaverIssues
                .Include(i => i.JobWorker)
                .Include(i => i.WarpBeam)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Yarn)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        [HttpPost("issues")]
        public async Task<ActionResult<WeaverIssue>> CreateIssue(WeaverIssue issue)
        {
            issue.Id = Guid.NewGuid();
            issue.CreatedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;
            issue.Status = "Issued";

            // Set warp beam to Assigned
            var beam = await _context.WarpBeamMasters.FindAsync(issue.WarpBeamId);
            if (beam != null)
            {
                beam.Status = "Assigned";
                beam.UpdatedAt = DateTime.UtcNow;
                _context.Entry(beam).State = EntityState.Modified;
            }

            // Subtract additional issued yarn (weft/zari) from stock
            foreach (var item in issue.Items)
            {
                item.Id = Guid.NewGuid();
                item.WeaverIssueId = issue.Id;
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                var stock = await _context.YarnStocks.FirstOrDefaultAsync(s => s.YarnId == item.YarnId);
                if (stock != null)
                {
                    stock.Quantity -= item.QuantityIssued;
                    stock.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(stock).State = EntityState.Modified;
                }
            }

            _context.WeaverIssues.Add(issue);
            await _context.SaveChangesAsync();
            return Ok(issue);
        }

        // --- Saree Receipts ---
        [HttpGet("saree-receipts")]
        public async Task<ActionResult<IEnumerable<SareeReceipt>>> GetSareeReceipts()
        {
            return await _context.SareeReceipts
                .Include(r => r.JobWorker)
                .Include(r => r.WeaverIssue)
                    .ThenInclude(i => i.WarpBeam)
                .Include(r => r.Items)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }

        [HttpPost("saree-receipts")]
        public async Task<ActionResult<SareeReceipt>> CreateSareeReceipt(SareeReceipt receipt)
        {
            receipt.Id = Guid.NewGuid();
            receipt.CreatedAt = DateTime.UtcNow;
            receipt.UpdatedAt = DateTime.UtcNow;

            foreach (var item in receipt.Items)
            {
                item.Id = Guid.NewGuid();
                item.SareeReceiptId = receipt.Id;
                item.Status = "Received";
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
            }

            _context.SareeReceipts.Add(receipt);

            // Update Weaver Issue status and check if Warp Beam is completed
            var issue = await _context.WeaverIssues
                .Include(i => i.WarpBeam)
                .FirstOrDefaultAsync(i => i.Id == receipt.WeaverIssueId);

            if (issue != null)
            {
                // Count already received sarees
                var prevReceivedCount = await _context.SareeReceiptItems
                    .CountAsync(sri => _context.SareeReceipts
                        .Where(sr => sr.WeaverIssueId == issue.Id)
                        .Select(sr => sr.Id)
                        .Contains(sri.SareeReceiptId));

                int newReceivedCount = receipt.Items.Count;
                int totalReceived = prevReceivedCount + newReceivedCount;

                if (totalReceived >= issue.ExpectedSarees)
                {
                    issue.Status = "Closed";
                    if (issue.WarpBeam != null)
                    {
                        issue.WarpBeam.Status = "Completed";
                        _context.Entry(issue.WarpBeam).State = EntityState.Modified;
                    }
                }
                else
                {
                    issue.Status = "Partially Received";
                }

                issue.UpdatedAt = DateTime.UtcNow;
                _context.Entry(issue).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return Ok(receipt);
        }
    }

    // ============================================================================
    // 5. Dashboard Controller
    // ============================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var workersCount = await _context.JobWorkers.CountAsync();
            var activeDeliveries = await _context.DyeingYarnDeliveries.CountAsync(d => d.Status == "Pending");
            var activeWeavers = await _context.WeaverIssues.CountAsync(i => i.Status == "Issued" || i.Status == "Partially Received");
            
            // Total finished sarees received
            var totalSareesCount = await _context.SareeReceiptItems.CountAsync();

            // Low stock warning list (less than 5.000 kg)
            var lowStock = await _context.YarnStocks
                .Include(s => s.Yarn)
                .Where(s => s.Quantity < 5.000m)
                .Select(s => new
                {
                    YarnType = s.Yarn!.YarnType,
                    Color = s.Yarn.Color,
                    Quantity = s.Quantity
                })
                .ToListAsync();

            return Ok(new
            {
                WorkersCount = workersCount,
                ActiveDeliveries = activeDeliveries,
                ActiveWeavers = activeWeavers,
                TotalSareesCount = totalSareesCount,
                LowStock = lowStock
            });
        }
    }
}
