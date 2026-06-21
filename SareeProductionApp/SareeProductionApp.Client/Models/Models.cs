using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SareeProductionApp.Client.Models
{
    [Table("job_workers", Schema = "public")]
    public class JobWorker
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("worker_type")]
        public string? WorkerType { get; set; } // e.g., 'Dyer', 'Weaver', 'Warping'

        [Column("place")]
        public string? Place { get; set; }

        [Column("cell_no")]
        public string? CellNo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("yarn_master", Schema = "public")]
    public class YarnMaster
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("yarn_type")]
        public string YarnType { get; set; } = string.Empty; // e.g., 'Silk', 'Zari', 'Cotton'

        [Column("color")]
        public string? Color { get; set; }

        [Column("denier")]
        public string? Denier { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("yarn_stock", Schema = "public")]
    public class YarnStock
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("yarn_id")]
        public Guid YarnId { get; set; }

        [ForeignKey(nameof(YarnId))]
        public YarnMaster? Yarn { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; } = 0.000m; // weight in kg

        [Column("qty")]
        public decimal Qty { get; set; } = 0.000m; // quantity count (cones/bags/etc)

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("dyeing_yarn_delivery", Schema = "public")]
    public class DyeingYarnDelivery
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("delivery_no")]
        public string DeliveryNo { get; set; } = string.Empty;

        [Column("job_worker_id")]
        public Guid JobWorkerId { get; set; }

        [ForeignKey(nameof(JobWorkerId))]
        public JobWorker? JobWorker { get; set; }

        [Column("delivery_date")]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Partially Received', 'Received', 'Cancelled'

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public List<DyeingYarnDeliveryItem> Items { get; set; } = new();
    }

    [Table("dyeing_yarn_delivery_items", Schema = "public")]
    public class DyeingYarnDeliveryItem
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("delivery_id")]
        public Guid DeliveryId { get; set; }

        [Column("yarn_id")]
        public Guid YarnId { get; set; }

        [ForeignKey(nameof(YarnId))]
        public YarnMaster? Yarn { get; set; }

        [Column("color_requested")]
        public string? ColorRequested { get; set; }

        [Column("quantity_kg")]
        public decimal QuantityKg { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("dyeing_yarn_receipt", Schema = "public")]
    public class DyeingYarnReceipt
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("receipt_no")]
        public string ReceiptNo { get; set; } = string.Empty;

        [Column("job_worker_id")]
        public Guid JobWorkerId { get; set; }

        [ForeignKey(nameof(JobWorkerId))]
        public JobWorker? JobWorker { get; set; }

        [Column("delivery_id")]
        public Guid? DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public DyeingYarnDelivery? Delivery { get; set; }

        [Column("receipt_date")]
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public List<DyeingYarnReceiptItem> Items { get; set; } = new();
    }

    [Table("dyeing_yarn_receipt_items", Schema = "public")]
    public class DyeingYarnReceiptItem
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("receipt_id")]
        public Guid ReceiptId { get; set; }

        [Column("delivery_item_id")]
        public Guid? DeliveryItemId { get; set; }

        [ForeignKey(nameof(DeliveryItemId))]
        public DyeingYarnDeliveryItem? DeliveryItem { get; set; }

        [Column("yarn_id")]
        public Guid YarnId { get; set; }

        [ForeignKey(nameof(YarnId))]
        public YarnMaster? Yarn { get; set; }

        [Column("received_color")]
        public string? ReceivedColor { get; set; }

        [Column("quantity_kg")]
        public decimal QuantityKg { get; set; }

        [Column("wastage_kg")]
        public decimal WastageKg { get; set; } = 0.000m;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("warp_beam_master", Schema = "public")]
    public class WarpBeamMaster
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("beam_no")]
        public string BeamNo { get; set; } = string.Empty;

        [Column("yarn_id")]
        public Guid YarnId { get; set; }

        [ForeignKey(nameof(YarnId))]
        public YarnMaster? Yarn { get; set; }

        [Column("total_meters")]
        public decimal TotalMeters { get; set; }

        [Column("design_details")]
        public string? DesignDetails { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Available"; // 'Available', 'Assigned', 'Completed'

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("weaver_issue", Schema = "public")]
    public class WeaverIssue
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("issue_no")]
        public string IssueNo { get; set; } = string.Empty;

        [Column("job_worker_id")]
        public Guid JobWorkerId { get; set; } // Weaver

        [ForeignKey(nameof(JobWorkerId))]
        public JobWorker? JobWorker { get; set; }

        [Column("warp_beam_id")]
        public Guid WarpBeamId { get; set; }

        [ForeignKey(nameof(WarpBeamId))]
        public WarpBeamMaster? WarpBeam { get; set; }

        [Column("issue_date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Column("expected_sarees")]
        public int ExpectedSarees { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Issued"; // 'Issued', 'Partially Received', 'Closed'

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public List<WeaverIssueItem> Items { get; set; } = new();
    }

    [Table("weaver_issue_items", Schema = "public")]
    public class WeaverIssueItem
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("weaver_issue_id")]
        public Guid WeaverIssueId { get; set; }

        [Column("yarn_id")]
        public Guid YarnId { get; set; }

        [ForeignKey(nameof(YarnId))]
        public YarnMaster? Yarn { get; set; }

        [Column("quantity_issued")]
        public decimal QuantityIssued { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("saree_receipt", Schema = "public")]
    public class SareeReceipt
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("receipt_no")]
        public string ReceiptNo { get; set; } = string.Empty;

        [Column("job_worker_id")]
        public Guid JobWorkerId { get; set; } // Weaver

        [ForeignKey(nameof(JobWorkerId))]
        public JobWorker? JobWorker { get; set; }

        [Column("weaver_issue_id")]
        public Guid WeaverIssueId { get; set; }

        [ForeignKey(nameof(WeaverIssueId))]
        public WeaverIssue? WeaverIssue { get; set; }

        [Column("receipt_date")]
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public List<SareeReceiptItem> Items { get; set; } = new();
    }

    [Table("saree_receipt_items", Schema = "public")]
    public class SareeReceiptItem
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("saree_receipt_id")]
        public Guid SareeReceiptId { get; set; }

        [Column("saree_no")]
        public string? SareeNo { get; set; }

        [Column("saree_type")]
        public string? SareeType { get; set; }

        [Column("weight_grams")]
        public decimal? WeightGrams { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Received"; // 'Received', 'Quality Checked', 'In Stock', 'Sold'

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }

    [Table("yarn_categories", Schema = "public")]
    public class YarnCategory
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
