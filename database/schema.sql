-- Hardware Shop POS Database Schema
-- Created: 2026-01-12
-- Description: Complete database structure for Hardware Shop Point of Sale System

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- STORES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS stores (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    address TEXT,
    phone TEXT,
    email TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- EMPLOYEES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS employees (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    employee_code VARCHAR(50) UNIQUE,
    phone TEXT,
    email TEXT,
    address TEXT,
    role VARCHAR(50) DEFAULT 'staff',
    permissions JSONB DEFAULT '[]'::jsonb,
    store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- CATEGORIES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    description TEXT,
    category_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    cost_price NUMERIC DEFAULT 0,
    retail_price NUMERIC DEFAULT 0,
    wholesale_price NUMERIC DEFAULT 0,
    unit_of_measure VARCHAR(50) DEFAULT 'piece',
    barcode VARCHAR(100),
    hsn_code VARCHAR(50),
    reorder_level INTEGER DEFAULT 10,
    tax_method VARCHAR(50) DEFAULT 'exclusive',
    tax_rate NUMERIC DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_items_barcode ON items(barcode) WHERE barcode IS NOT NULL;

-- ============================================================================
-- ITEM STORE STOCK TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS item_store_stock (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    quantity_on_hand NUMERIC DEFAULT 0,
    reserved_quantity NUMERIC DEFAULT 0,
    last_restock_date DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(item_id, store_id)
);

CREATE INDEX IF NOT EXISTS idx_item_store_stock_item_id ON item_store_stock(item_id);
CREATE INDEX IF NOT EXISTS idx_item_store_stock_store_id ON item_store_stock(store_id);

-- ============================================================================
-- CUSTOMERS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    type VARCHAR(50) DEFAULT 'retail',
    contact_person TEXT,
    phone TEXT,
    email TEXT,
    address TEXT,
    tax_number VARCHAR(50),
    credit_limit NUMERIC DEFAULT 0,
    customer_since_date DATE,
    opening_balance NUMERIC DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_customers_name ON customers(name);
CREATE INDEX IF NOT EXISTS idx_customers_is_active ON customers(is_active);

-- ============================================================================
-- SUPPLIERS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS suppliers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    contact_person TEXT,
    phone TEXT,
    email TEXT,
    address TEXT,
    tax_number VARCHAR(50),
    payment_terms VARCHAR(100),
    opening_balance NUMERIC DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_suppliers_name ON suppliers(name);
CREATE INDEX IF NOT EXISTS idx_suppliers_is_active ON suppliers(is_active);

-- ============================================================================
-- SALES RETAIL TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_retail (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    invoice_number VARCHAR(100) NOT NULL UNIQUE,
    invoice_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    sale_date DATE DEFAULT CURRENT_DATE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    payment_method VARCHAR(50),
    payment_status VARCHAR(50) DEFAULT 'unpaid',
    subtotal NUMERIC NOT NULL DEFAULT 0,
    discount NUMERIC DEFAULT 0,
    tax NUMERIC DEFAULT 0,
    total_amount NUMERIC NOT NULL DEFAULT 0,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_sales_retail_invoice_number ON sales_retail(invoice_number);
CREATE INDEX IF NOT EXISTS idx_sales_retail_payment_status ON sales_retail(payment_status);
CREATE INDEX IF NOT EXISTS idx_sales_retail_store_id ON sales_retail(store_id);
CREATE INDEX IF NOT EXISTS idx_sales_retail_customer_id ON sales_retail(customer_id);
CREATE INDEX IF NOT EXISTS idx_sales_retail_sale_date ON sales_retail(sale_date);

-- ============================================================================
-- SALES RETAIL ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_retail_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sales_retail_id UUID NOT NULL REFERENCES sales_retail(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    quantity NUMERIC NOT NULL,
    unit_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    tax_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_sales_retail_items_sales_id ON sales_retail_items(sales_retail_id);
CREATE INDEX IF NOT EXISTS idx_sales_retail_items_item_id ON sales_retail_items(item_id);

-- ============================================================================
-- SALES WHOLESALE TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_wholesale (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    invoice_number VARCHAR(100) NOT NULL UNIQUE,
    invoice_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    sale_date DATE DEFAULT CURRENT_DATE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    payment_method VARCHAR(50),
    payment_status VARCHAR(50) DEFAULT 'unpaid',
    subtotal NUMERIC NOT NULL DEFAULT 0,
    discount NUMERIC DEFAULT 0,
    tax NUMERIC DEFAULT 0,
    total_amount NUMERIC NOT NULL DEFAULT 0,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_sales_wholesale_invoice_number ON sales_wholesale(invoice_number);

-- ============================================================================
-- SALES WHOLESALE ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_wholesale_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sales_wholesale_id UUID NOT NULL REFERENCES sales_wholesale(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    quantity NUMERIC NOT NULL,
    unit_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    tax_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_sales_wholesale_items_sales_id ON sales_wholesale_items(sales_wholesale_id);

-- ============================================================================
-- INVENTORY TRANSACTIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS inventory_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    transaction_type VARCHAR(50) NOT NULL,
    quantity NUMERIC NOT NULL,
    batch_no VARCHAR(100),
    batch_expiry DATE,
    reference_id UUID,
    reference_type VARCHAR(50),
    notes TEXT,
    created_by UUID REFERENCES employees(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_inventory_transactions_item_id ON inventory_transactions(item_id);
CREATE INDEX IF NOT EXISTS idx_inventory_transactions_store_id ON inventory_transactions(store_id);
CREATE INDEX IF NOT EXISTS idx_inventory_transactions_created_at ON inventory_transactions(created_at);

-- ============================================================================
-- OPENING STOCK ENTRIES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS opening_stock_entries (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ref_number VARCHAR(100) NOT NULL UNIQUE,
    entry_date DATE NOT NULL DEFAULT CURRENT_DATE,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    supplier_id UUID REFERENCES suppliers(id) ON DELETE SET NULL,
    description TEXT,
    total_value NUMERIC DEFAULT 0,
    total_discount NUMERIC DEFAULT 0,
    net_total NUMERIC DEFAULT 0,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_opening_stock_entries_ref_number ON opening_stock_entries(ref_number);

-- ============================================================================
-- OPENING STOCK ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS opening_stock_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    opening_stock_entry_id UUID NOT NULL REFERENCES opening_stock_entries(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    batch_expiry DATE,
    quantity NUMERIC NOT NULL,
    cost_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- STOCK ADJUSTMENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS stock_adjustments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    adjustment_number VARCHAR(100) NOT NULL UNIQUE,
    adjustment_date DATE NOT NULL DEFAULT CURRENT_DATE,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    description TEXT,
    reason TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_stock_adjustments_adjustment_number ON stock_adjustments(adjustment_number);

-- ============================================================================
-- STOCK ADJUSTMENT ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS stock_adjustment_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    stock_adjustment_id UUID NOT NULL REFERENCES stock_adjustments(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    current_stock NUMERIC,
    adjustment_qty NUMERIC NOT NULL,
    adjustment_reason VARCHAR(100),
    remarks TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- PURCHASE ORDERS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS purchase_orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    po_number VARCHAR(100) NOT NULL UNIQUE,
    po_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    expected_delivery_date DATE,
    status VARCHAR(50) DEFAULT 'pending',
    subtotal NUMERIC NOT NULL DEFAULT 0,
    discount NUMERIC DEFAULT 0,
    tax NUMERIC DEFAULT 0,
    total_amount NUMERIC NOT NULL DEFAULT 0,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_purchase_orders_po_number ON purchase_orders(po_number);

-- ============================================================================
-- PURCHASE ORDER ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS purchase_order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    quantity NUMERIC NOT NULL,
    unit_cost NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    received_qty NUMERIC DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- PURCHASE GRN (GOODS RECEIPT NOTE) TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS purchase_grns (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    grn_number VARCHAR(100) NOT NULL UNIQUE,
    grn_date DATE NOT NULL DEFAULT CURRENT_DATE,
    supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    po_reference_id UUID REFERENCES purchase_orders(id) ON DELETE SET NULL,
    invoice_number VARCHAR(100),
    invoice_date DATE,
    invoice_amount NUMERIC,
    total_amount NUMERIC NOT NULL DEFAULT 0,
    payment_status VARCHAR(50) DEFAULT 'unpaid',
    description TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_purchase_grns_grn_number ON purchase_grns(grn_number);

-- ============================================================================
-- PURCHASE GRN ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS purchase_grn_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_grn_id UUID NOT NULL REFERENCES purchase_grns(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    ordered_qty NUMERIC,
    batch_no VARCHAR(100),
    batch_expiry DATE,
    received_qty NUMERIC NOT NULL,
    cost_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- PURCHASE RETURNS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS purchase_returns (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    return_number VARCHAR(100) NOT NULL UNIQUE,
    return_date DATE NOT NULL DEFAULT CURRENT_DATE,
    supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    grn_reference_id UUID REFERENCES purchase_grns(id) ON DELETE SET NULL,
    return_reason VARCHAR(100) NOT NULL,
    total_amount NUMERIC NOT NULL DEFAULT 0,
    description TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_purchase_returns_return_number ON purchase_returns(return_number);

-- ============================================================================
-- PURCHASE RETURN ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS purchase_return_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_return_id UUID NOT NULL REFERENCES purchase_returns(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    available_qty NUMERIC,
    return_qty NUMERIC NOT NULL,
    cost_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- SALES RETURNS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_returns (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    return_number VARCHAR(100) NOT NULL UNIQUE,
    return_date DATE NOT NULL DEFAULT CURRENT_DATE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    sales_retail_id UUID REFERENCES sales_retail(id) ON DELETE SET NULL,
    return_reason VARCHAR(100) NOT NULL,
    refund_method VARCHAR(50),
    total_refund_amount NUMERIC NOT NULL DEFAULT 0,
    description TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_sales_returns_return_number ON sales_returns(return_number);

-- ============================================================================
-- SALES RETURN ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_return_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sales_return_id UUID NOT NULL REFERENCES sales_returns(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    original_qty NUMERIC,
    return_qty NUMERIC NOT NULL,
    unit_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    refund_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- CUSTOMER PAYMENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS customer_payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    receipt_number VARCHAR(100) NOT NULL UNIQUE,
    payment_date DATE NOT NULL DEFAULT CURRENT_DATE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    payment_method VARCHAR(50) NOT NULL,
    reference_number VARCHAR(100),
    cheque_number VARCHAR(100),
    cheque_status VARCHAR(50) DEFAULT 'received',
    total_payment_amount NUMERIC NOT NULL,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_customer_payments_receipt_number ON customer_payments(receipt_number);

-- ============================================================================
-- CUSTOMER PAYMENT ALLOCATIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS customer_payment_allocations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_payment_id UUID NOT NULL REFERENCES customer_payments(id) ON DELETE CASCADE,
    sales_retail_id UUID REFERENCES sales_retail(id) ON DELETE SET NULL,
    sales_wholesale_id UUID REFERENCES sales_wholesale(id) ON DELETE SET NULL,
    invoice_number VARCHAR(100),
    invoice_date DATE,
    invoice_amount NUMERIC,
    paid_amount NUMERIC,
    outstanding NUMERIC,
    allocation_amount NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- SUPPLIER PAYMENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS supplier_payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_number VARCHAR(100) NOT NULL UNIQUE,
    payment_date DATE NOT NULL DEFAULT CURRENT_DATE,
    supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
    payment_method VARCHAR(50) NOT NULL,
    reference_number VARCHAR(100),
    cheque_number VARCHAR(100),
    cheque_status VARCHAR(50) DEFAULT 'received',
    total_payment_amount NUMERIC NOT NULL,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_supplier_payments_payment_number ON supplier_payments(payment_number);

-- ============================================================================
-- SUPPLIER PAYMENT ALLOCATIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS supplier_payment_allocations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    supplier_payment_id UUID NOT NULL REFERENCES supplier_payments(id) ON DELETE CASCADE,
    purchase_grn_id UUID REFERENCES purchase_grns(id) ON DELETE SET NULL,
    invoice_number VARCHAR(100),
    invoice_date DATE,
    invoice_amount NUMERIC,
    paid_amount NUMERIC,
    outstanding NUMERIC,
    allocation_amount NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- CUSTOMER OPENING BALANCES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS customer_opening_balances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entry_number VARCHAR(100) NOT NULL UNIQUE,
    entry_date DATE NOT NULL DEFAULT CURRENT_DATE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    amount NUMERIC NOT NULL,
    balance_type VARCHAR(50) NOT NULL,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_customer_opening_balances_entry_number ON customer_opening_balances(entry_number);

-- ============================================================================
-- SUPPLIER OPENING BALANCES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS supplier_opening_balances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entry_number VARCHAR(100) NOT NULL UNIQUE,
    entry_date DATE NOT NULL DEFAULT CURRENT_DATE,
    supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
    amount NUMERIC NOT NULL,
    balance_type VARCHAR(50) NOT NULL,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_supplier_opening_balances_entry_number ON supplier_opening_balances(entry_number);

-- ============================================================================
-- QUOTATIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS quotations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    quotation_number VARCHAR(100) NOT NULL UNIQUE,
    quotation_date DATE NOT NULL DEFAULT CURRENT_DATE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    valid_until DATE NOT NULL,
    subtotal NUMERIC DEFAULT 0,
    discount NUMERIC DEFAULT 0,
    tax NUMERIC DEFAULT 0,
    total_amount NUMERIC DEFAULT 0,
    status VARCHAR(50) DEFAULT 'active',
    terms_conditions TEXT,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_quotations_quotation_number ON quotations(quotation_number);

-- ============================================================================
-- QUOTATION ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS quotation_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    quotation_id UUID NOT NULL REFERENCES quotations(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    batch_no VARCHAR(100),
    quantity NUMERIC NOT NULL,
    unit_price NUMERIC NOT NULL,
    discount_percent NUMERIC DEFAULT 0,
    discount_value NUMERIC DEFAULT 0,
    tax_value NUMERIC DEFAULT 0,
    net_value NUMERIC NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- ITEM DISPATCH NOTES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS item_dispatch_notes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    dispatch_number VARCHAR(100) NOT NULL UNIQUE,
    dispatch_date DATE NOT NULL DEFAULT CURRENT_DATE,
    from_store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    to_store_id UUID NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    total_items INTEGER DEFAULT 0,
    total_quantity NUMERIC DEFAULT 0,
    description TEXT,
    notes TEXT,
    employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_item_dispatch_notes_dispatch_number ON item_dispatch_notes(dispatch_number);

-- ============================================================================
-- ITEM DISPATCH ITEMS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS item_dispatch_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    dispatch_id UUID NOT NULL REFERENCES item_dispatch_notes(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    quantity NUMERIC NOT NULL,
    batch_no VARCHAR(100),
    batch_expiry DATE,
    cost_price NUMERIC NOT NULL,
    retail_price NUMERIC NOT NULL,
    wholesale_price NUMERIC NOT NULL,
    unit_of_measure VARCHAR(50),
    dispatch_value NUMERIC NOT NULL DEFAULT 0,
    remarks TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- AUDIT LOGS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES employees(id) ON DELETE SET NULL,
    action VARCHAR(50) NOT NULL,
    table_name VARCHAR(100),
    record_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address VARCHAR(50),
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_table_name ON audit_logs(table_name);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON audit_logs(created_at);

-- ============================================================================
-- SYSTEM SETTINGS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS system_settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    setting_key VARCHAR(100) NOT NULL UNIQUE,
    setting_value TEXT,
    setting_type VARCHAR(50),
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_system_settings_setting_key ON system_settings(setting_key);

-- ============================================================================
-- SAMPLE DATA (Optional - For Development/Testing)
-- ============================================================================
-- Insert default store
INSERT INTO stores (code, name, address, phone, email)
VALUES ('STORE001', 'Main Store', '123 Hardware Lane', '+94112345678', 'store@hardware.lk')
ON CONFLICT (code) DO NOTHING;

-- Insert admin employee
INSERT INTO employees (name, employee_code, role, is_active)
SELECT 'Administrator', 'admin', 'admin', true
WHERE NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'admin');

-- Insert sample categories
INSERT INTO categories (name, description) VALUES
('Hardware Tools', 'Various hardware tools'),
('Fasteners', 'Screws, nails, bolts'),
('Paints & Finishes', 'Paints and finishing materials'),
('Electrical', 'Electrical supplies and fixtures'),
('Plumbing', 'Plumbing materials and fixtures')
ON CONFLICT (name) DO NOTHING;

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================