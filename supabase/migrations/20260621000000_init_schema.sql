-- ============================================================================
-- Silk Saree Production Management Database Schema
-- Designed for Supabase / PostgreSQL
-- ============================================================================

-- Enable UUID extension (if not already enabled)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- Trigger Function for maintaining updated_at
-- ============================================================================
CREATE OR REPLACE FUNCTION public.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = clock_timestamp();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 1. Table: job_workers (JobWorkers)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.job_workers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    worker_type TEXT, -- e.g., 'Dyer', 'Weaver', 'Warping'
    place TEXT,
    cell_no TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.job_workers IS 'Details of the job workers (e.g., dyers, weavers, beam makers).';
COMMENT ON COLUMN public.job_workers.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.job_workers.name IS 'Name of the job worker.';
COMMENT ON COLUMN public.job_workers.worker_type IS 'Role/specialization of the job worker.';
COMMENT ON COLUMN public.job_workers.place IS 'Location or address details.';
COMMENT ON COLUMN public.job_workers.cell_no IS 'Contact number.';
COMMENT ON COLUMN public.job_workers.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.job_workers.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.job_workers.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for job_workers
CREATE TRIGGER tr_job_workers_updated_at
    BEFORE UPDATE ON public.job_workers
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_job_workers_deleted_at ON public.job_workers(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_job_workers_name ON public.job_workers(name);


-- ============================================================================
-- 2. Table: yarn_master (YarnMaster)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.yarn_master (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    yarn_type TEXT NOT NULL, -- e.g., 'Silk', 'Zari', 'Cotton'
    color TEXT,
    denier TEXT,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.yarn_master IS 'Master catalog of different yarn types and specs.';
COMMENT ON COLUMN public.yarn_master.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.yarn_master.yarn_type IS 'Category of yarn (Silk, Zari, Cotton, etc.).';
COMMENT ON COLUMN public.yarn_master.color IS 'Color name or code of the yarn.';
COMMENT ON COLUMN public.yarn_master.denier IS 'Linear density of yarn (thickness indicator).';
COMMENT ON COLUMN public.yarn_master.description IS 'Additional notes or description.';
COMMENT ON COLUMN public.yarn_master.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.yarn_master.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.yarn_master.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for yarn_master
CREATE TRIGGER tr_yarn_master_updated_at
    BEFORE UPDATE ON public.yarn_master
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_yarn_master_deleted_at ON public.yarn_master(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_yarn_master_type ON public.yarn_master(yarn_type);


-- ============================================================================
-- 3. Table: yarn_stock (YarnStock)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.yarn_stock (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    yarn_id UUID NOT NULL REFERENCES public.yarn_master(id),
    quantity NUMERIC(10,3) NOT NULL DEFAULT 0.000, -- in kg or standard units
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.yarn_stock IS 'Tracks current available stock of yarn.';
COMMENT ON COLUMN public.yarn_stock.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.yarn_stock.yarn_id IS 'Foreign key referencing yarn_master.';
COMMENT ON COLUMN public.yarn_stock.quantity IS 'Current stock quantity (typically in KGs).';
COMMENT ON COLUMN public.yarn_stock.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.yarn_stock.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.yarn_stock.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for yarn_stock
CREATE TRIGGER tr_yarn_stock_updated_at
    BEFORE UPDATE ON public.yarn_stock
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_yarn_stock_deleted_at ON public.yarn_stock(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_yarn_stock_yarn_id ON public.yarn_stock(yarn_id);


-- ============================================================================
-- 4. Table: dyeing_yarn_delivery (DyeingYarnDelivery)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.dyeing_yarn_delivery (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    delivery_no TEXT NOT NULL UNIQUE,
    job_worker_id UUID NOT NULL REFERENCES public.job_workers(id),
    delivery_date DATE NOT NULL DEFAULT CURRENT_DATE,
    status TEXT NOT NULL DEFAULT 'Pending', -- e.g., 'Pending', 'Partially Received', 'Received', 'Cancelled'
    remarks TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.dyeing_yarn_delivery IS 'Header records for yarn delivered to dyers.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.dyeing_yarn_delivery.delivery_no IS 'Unique alphanumeric delivery number.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.job_worker_id IS 'Foreign key referencing job_workers (the Dyer).';
COMMENT ON COLUMN public.dyeing_yarn_delivery.delivery_date IS 'Date the yarn was delivered for dyeing.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.status IS 'Current status of the delivery batch.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.remarks IS 'Any delivery-related comments.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.dyeing_yarn_delivery.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for dyeing_yarn_delivery
CREATE TRIGGER tr_dyeing_yarn_delivery_updated_at
    BEFORE UPDATE ON public.dyeing_yarn_delivery
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_delivery_deleted_at ON public.dyeing_yarn_delivery(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_delivery_job_worker ON public.dyeing_yarn_delivery(job_worker_id);
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_delivery_no ON public.dyeing_yarn_delivery(delivery_no);


-- ============================================================================
-- 5. Table: dyeing_yarn_delivery_items (DyeingYarnDeliveryItems)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.dyeing_yarn_delivery_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    delivery_id UUID NOT NULL REFERENCES public.dyeing_yarn_delivery(id) ON DELETE CASCADE,
    yarn_id UUID NOT NULL REFERENCES public.yarn_master(id),
    color_requested TEXT,
    quantity_kg NUMERIC(10,3) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.dyeing_yarn_delivery_items IS 'Detailed item lines of yarn delivered for dyeing.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.delivery_id IS 'Foreign key referencing dyeing_yarn_delivery.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.yarn_id IS 'Foreign key referencing yarn_master.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.color_requested IS 'Desired dyed color.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.quantity_kg IS 'Weight of yarn delivered in KGs.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.dyeing_yarn_delivery_items.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for dyeing_yarn_delivery_items
CREATE TRIGGER tr_dyeing_yarn_delivery_items_updated_at
    BEFORE UPDATE ON public.dyeing_yarn_delivery_items
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_del_items_deleted_at ON public.dyeing_yarn_delivery_items(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_del_items_delivery_id ON public.dyeing_yarn_delivery_items(delivery_id);
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_del_items_yarn_id ON public.dyeing_yarn_delivery_items(yarn_id);


-- ============================================================================
-- 6. Table: dyeing_yarn_receipt (DyeingYarnReceipt)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.dyeing_yarn_receipt (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    receipt_no TEXT NOT NULL UNIQUE,
    job_worker_id UUID NOT NULL REFERENCES public.job_workers(id),
    delivery_id UUID REFERENCES public.dyeing_yarn_delivery(id), -- optional link back to original delivery
    receipt_date DATE NOT NULL DEFAULT CURRENT_DATE,
    remarks TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.dyeing_yarn_receipt IS 'Header records for dyed yarn received back from dyers.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.dyeing_yarn_receipt.receipt_no IS 'Unique alphanumeric receipt number.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.job_worker_id IS 'Foreign key referencing job_workers (the Dyer).';
COMMENT ON COLUMN public.dyeing_yarn_receipt.delivery_id IS 'Optional foreign key referencing the source dyeing_yarn_delivery.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.receipt_date IS 'Date the dyed yarn was received.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.remarks IS 'Any receipt-related comments.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.dyeing_yarn_receipt.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for dyeing_yarn_receipt
CREATE TRIGGER tr_dyeing_yarn_receipt_updated_at
    BEFORE UPDATE ON public.dyeing_yarn_receipt
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_receipt_deleted_at ON public.dyeing_yarn_receipt(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_receipt_job_worker ON public.dyeing_yarn_receipt(job_worker_id);
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_receipt_delivery_id ON public.dyeing_yarn_receipt(delivery_id);
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_receipt_no ON public.dyeing_yarn_receipt(receipt_no);


-- ============================================================================
-- 7. Table: dyeing_yarn_receipt_items (DyeingYarnReceiptItems)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.dyeing_yarn_receipt_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    receipt_id UUID NOT NULL REFERENCES public.dyeing_yarn_receipt(id) ON DELETE CASCADE,
    delivery_item_id UUID REFERENCES public.dyeing_yarn_delivery_items(id), -- optional link back to sent item
    yarn_id UUID NOT NULL REFERENCES public.yarn_master(id),
    received_color TEXT,
    quantity_kg NUMERIC(10,3) NOT NULL,
    wastage_kg NUMERIC(10,3) NOT NULL DEFAULT 0.000,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.dyeing_yarn_receipt_items IS 'Detailed item lines of dyed yarn received.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.receipt_id IS 'Foreign key referencing dyeing_yarn_receipt.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.delivery_item_id IS 'Optional foreign key referencing the source dyeing_yarn_delivery_items.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.yarn_id IS 'Foreign key referencing yarn_master (dyed yarn).';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.received_color IS 'Actual color of the dyed yarn received.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.quantity_kg IS 'Weight of dyed yarn received in KGs.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.wastage_kg IS 'Wastage reported during the dyeing process.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.dyeing_yarn_receipt_items.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for dyeing_yarn_receipt_items
CREATE TRIGGER tr_dyeing_yarn_receipt_items_updated_at
    BEFORE UPDATE ON public.dyeing_yarn_receipt_items
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_rec_items_deleted_at ON public.dyeing_yarn_receipt_items(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_rec_items_receipt_id ON public.dyeing_yarn_receipt_items(receipt_id);
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_rec_items_delivery_item ON public.dyeing_yarn_receipt_items(delivery_item_id);
CREATE INDEX IF NOT EXISTS idx_dyeing_yarn_rec_items_yarn_id ON public.dyeing_yarn_receipt_items(yarn_id);


-- ============================================================================
-- 8. Table: warp_beam_master (WarpBeamMaster)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.warp_beam_master (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    beam_no TEXT NOT NULL UNIQUE,
    yarn_id UUID NOT NULL REFERENCES public.yarn_master(id),
    total_meters NUMERIC(10,2) NOT NULL,
    design_details TEXT,
    status TEXT NOT NULL DEFAULT 'Available', -- e.g., 'Available', 'Assigned', 'Completed'
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.warp_beam_master IS 'Master registry of prepared warp beams ready for weavers.';
COMMENT ON COLUMN public.warp_beam_master.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.warp_beam_master.beam_no IS 'Unique identifier or serial number for the warp beam.';
COMMENT ON COLUMN public.warp_beam_master.yarn_id IS 'Foreign key referencing yarn_master (primary warp yarn).';
COMMENT ON COLUMN public.warp_beam_master.total_meters IS 'Total length of warp wrapped on the beam (in meters).';
COMMENT ON COLUMN public.warp_beam_master.design_details IS 'Design configuration, patterns, or draft notes.';
COMMENT ON COLUMN public.warp_beam_master.status IS 'Current availability status of the beam.';
COMMENT ON COLUMN public.warp_beam_master.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.warp_beam_master.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.warp_beam_master.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for warp_beam_master
CREATE TRIGGER tr_warp_beam_master_updated_at
    BEFORE UPDATE ON public.warp_beam_master
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_warp_beam_master_deleted_at ON public.warp_beam_master(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_warp_beam_master_yarn_id ON public.warp_beam_master(yarn_id);
CREATE INDEX IF NOT EXISTS idx_warp_beam_master_no ON public.warp_beam_master(beam_no);


-- ============================================================================
-- 9. Table: weaver_issue (WeaverIssue)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.weaver_issue (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    issue_no TEXT NOT NULL UNIQUE,
    job_worker_id UUID NOT NULL REFERENCES public.job_workers(id), -- The Weaver
    warp_beam_id UUID NOT NULL REFERENCES public.warp_beam_master(id),
    issue_date DATE NOT NULL DEFAULT CURRENT_DATE,
    expected_sarees INTEGER NOT NULL,
    status TEXT NOT NULL DEFAULT 'Issued', -- e.g., 'Issued', 'Partially Received', 'Closed'
    remarks TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.weaver_issue IS 'Header records for materials and warp beams issued to weavers.';
COMMENT ON COLUMN public.weaver_issue.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.weaver_issue.issue_no IS 'Unique alphanumeric issue transaction number.';
COMMENT ON COLUMN public.weaver_issue.job_worker_id IS 'Foreign key referencing job_workers (the Weaver).';
COMMENT ON COLUMN public.weaver_issue.warp_beam_id IS 'Foreign key referencing warp_beam_master issued.';
COMMENT ON COLUMN public.weaver_issue.issue_date IS 'Date of issue.';
COMMENT ON COLUMN public.weaver_issue.expected_sarees IS 'Number of sarees expected to be produced from this beam/issue.';
COMMENT ON COLUMN public.weaver_issue.status IS 'Current status of the issue.';
COMMENT ON COLUMN public.weaver_issue.remarks IS 'Any issue-related comments.';
COMMENT ON COLUMN public.weaver_issue.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.weaver_issue.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.weaver_issue.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for weaver_issue
CREATE TRIGGER tr_weaver_issue_updated_at
    BEFORE UPDATE ON public.weaver_issue
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_weaver_issue_deleted_at ON public.weaver_issue(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_weaver_issue_job_worker ON public.weaver_issue(job_worker_id);
CREATE INDEX IF NOT EXISTS idx_weaver_issue_warp_beam ON public.weaver_issue(warp_beam_id);
CREATE INDEX IF NOT EXISTS idx_weaver_issue_no ON public.weaver_issue(issue_no);


-- ============================================================================
-- 10. Table: weaver_issue_items (WeaverIssueItems)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.weaver_issue_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    weaver_issue_id UUID NOT NULL REFERENCES public.weaver_issue(id) ON DELETE CASCADE,
    yarn_id UUID NOT NULL REFERENCES public.yarn_master(id), -- e.g., weft yarn, zari
    quantity_issued NUMERIC(10,3) NOT NULL, -- in kg or standard units
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.weaver_issue_items IS 'Line items representing additional yarn/zari issued alongside the warp beam.';
COMMENT ON COLUMN public.weaver_issue_items.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.weaver_issue_items.weaver_issue_id IS 'Foreign key referencing weaver_issue.';
COMMENT ON COLUMN public.weaver_issue_items.yarn_id IS 'Foreign key referencing yarn_master.';
COMMENT ON COLUMN public.weaver_issue_items.quantity_issued IS 'Weight/amount of yarn/material issued.';
COMMENT ON COLUMN public.weaver_issue_items.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.weaver_issue_items.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.weaver_issue_items.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for weaver_issue_items
CREATE TRIGGER tr_weaver_issue_items_updated_at
    BEFORE UPDATE ON public.weaver_issue_items
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_weaver_issue_items_deleted_at ON public.weaver_issue_items(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_weaver_issue_items_issue_id ON public.weaver_issue_items(weaver_issue_id);
CREATE INDEX IF NOT EXISTS idx_weaver_issue_items_yarn_id ON public.weaver_issue_items(yarn_id);


-- ============================================================================
-- 11. Table: saree_receipt (SareeReceipt)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.saree_receipt (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    receipt_no TEXT NOT NULL UNIQUE,
    job_worker_id UUID NOT NULL REFERENCES public.job_workers(id), -- The Weaver
    weaver_issue_id UUID NOT NULL REFERENCES public.weaver_issue(id),
    receipt_date DATE NOT NULL DEFAULT CURRENT_DATE,
    remarks TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.saree_receipt IS 'Header records for sarees received back from weavers.';
COMMENT ON COLUMN public.saree_receipt.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.saree_receipt.receipt_no IS 'Unique alphanumeric saree receipt number.';
COMMENT ON COLUMN public.saree_receipt.job_worker_id IS 'Foreign key referencing job_workers (the Weaver).';
COMMENT ON COLUMN public.saree_receipt.weaver_issue_id IS 'Foreign key referencing the original weaver_issue.';
COMMENT ON COLUMN public.saree_receipt.receipt_date IS 'Date the sarees were received.';
COMMENT ON COLUMN public.saree_receipt.remarks IS 'Any receipt-related comments.';
COMMENT ON COLUMN public.saree_receipt.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.saree_receipt.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.saree_receipt.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for saree_receipt
CREATE TRIGGER tr_saree_receipt_updated_at
    BEFORE UPDATE ON public.saree_receipt
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_saree_receipt_deleted_at ON public.saree_receipt(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_saree_receipt_job_worker ON public.saree_receipt(job_worker_id);
CREATE INDEX IF NOT EXISTS idx_saree_receipt_weaver_issue ON public.saree_receipt(weaver_issue_id);
CREATE INDEX IF NOT EXISTS idx_saree_receipt_no ON public.saree_receipt(receipt_no);


-- ============================================================================
-- 12. Table: saree_receipt_items (SareeReceiptItems)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.saree_receipt_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    saree_receipt_id UUID NOT NULL REFERENCES public.saree_receipt(id) ON DELETE CASCADE,
    saree_no TEXT, -- Unique tag/serial number for the specific saree
    saree_type TEXT, -- e.g. Design details, type
    weight_grams NUMERIC(10,2),
    status TEXT NOT NULL DEFAULT 'Received', -- e.g., 'Received', 'Quality Checked', 'In Stock', 'Sold'
    remarks TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp(),
    deleted_at TIMESTAMPTZ
);

COMMENT ON TABLE public.saree_receipt_items IS 'Detailed item lines representing individual finished sarees received.';
COMMENT ON COLUMN public.saree_receipt_items.id IS 'Primary Key (UUID).';
COMMENT ON COLUMN public.saree_receipt_items.saree_receipt_id IS 'Foreign key referencing saree_receipt.';
COMMENT ON COLUMN public.saree_receipt_items.saree_no IS 'Unique identifier code printed on the saree tag.';
COMMENT ON COLUMN public.saree_receipt_items.saree_type IS 'Design, pattern, or type classification of the saree.';
COMMENT ON COLUMN public.saree_receipt_items.weight_grams IS 'Weight of the finished saree in grams.';
COMMENT ON COLUMN public.saree_receipt_items.status IS 'Quality or inventory status of the saree.';
COMMENT ON COLUMN public.saree_receipt_items.remarks IS 'Any specific remarks (e.g. damages, details).';
COMMENT ON COLUMN public.saree_receipt_items.created_at IS 'Timestamp when the record was created.';
COMMENT ON COLUMN public.saree_receipt_items.updated_at IS 'Timestamp when the record was last updated.';
COMMENT ON COLUMN public.saree_receipt_items.deleted_at IS 'Timestamp for soft delete support.';

-- Trigger for saree_receipt_items
CREATE TRIGGER tr_saree_receipt_items_updated_at
    BEFORE UPDATE ON public.saree_receipt_items
    FOR EACH ROW
    EXECUTE FUNCTION public.update_updated_at_column();

-- Indexes
CREATE INDEX IF NOT EXISTS idx_saree_rec_items_deleted_at ON public.saree_receipt_items(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_saree_rec_items_receipt_id ON public.saree_receipt_items(saree_receipt_id);
CREATE INDEX IF NOT EXISTS idx_saree_rec_items_saree_no ON public.saree_receipt_items(saree_no);


-- ============================================================================
-- Soft Delete Views (Active Records Only)
-- These views provide a clean interface to query active records directly.
-- ============================================================================

CREATE OR REPLACE VIEW public.active_job_workers AS
SELECT * FROM public.job_workers WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_yarn_master AS
SELECT * FROM public.yarn_master WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_yarn_stock AS
SELECT * FROM public.yarn_stock WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_dyeing_yarn_delivery AS
SELECT * FROM public.dyeing_yarn_delivery WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_dyeing_yarn_delivery_items AS
SELECT * FROM public.dyeing_yarn_delivery_items WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_dyeing_yarn_receipt AS
SELECT * FROM public.dyeing_yarn_receipt WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_dyeing_yarn_receipt_items AS
SELECT * FROM public.dyeing_yarn_receipt_items WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_warp_beam_master AS
SELECT * FROM public.warp_beam_master WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_weaver_issue AS
SELECT * FROM public.weaver_issue WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_weaver_issue_items AS
SELECT * FROM public.weaver_issue_items WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_saree_receipt AS
SELECT * FROM public.saree_receipt WHERE deleted_at IS NULL;

CREATE OR REPLACE VIEW public.active_saree_receipt_items AS
SELECT * FROM public.saree_receipt_items WHERE deleted_at IS NULL;
