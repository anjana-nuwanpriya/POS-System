-- Hardware Shop POS Database - Sample/Seed Data
-- For Development and Testing Only
-- Created: 2026-01-12

-- ============================================================================
-- STORES SEED DATA
-- ============================================================================
INSERT INTO stores (id, code, name, address, phone, email, is_active)
VALUES 
    ('f0000000-0000-0000-0000-000000000001', 'STORE001', 'Main Store - Colombo', '123 Hardware Lane, Colombo', '+94112345678', 'colombo@hardware.lk', true),
    ('f0000000-0000-0000-0000-000000000002', 'STORE002', 'Branch Store - Kandy', '456 Commerce Road, Kandy', '+94812111111', 'kandy@hardware.lk', true),
    ('f0000000-0000-0000-0000-000000000003', 'STORE003', 'Branch Store - Galle', '789 Market Street, Galle', '+94912222222', 'galle@hardware.lk', true)
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- EMPLOYEES SEED DATA
-- ============================================================================
INSERT INTO employees (id, name, employee_code, email, phone, role, store_id, is_active)
VALUES
    ('f1000000-0000-0000-0000-000000000001', 'Administrator', 'admin', 'admin@hardware.lk', '+94712345678', 'admin', 'f0000000-0000-0000-0000-000000000001', true),
    ('f1000000-0000-0000-0000-000000000002', 'John Silva', 'E001', 'john@hardware.lk', '+94712345679', 'cashier', 'f0000000-0000-0000-0000-000000000001', true),
    ('f1000000-0000-0000-0000-000000000003', 'Maria Perera', 'E002', 'maria@hardware.lk', '+94712345680', 'inventory', 'f0000000-0000-0000-0000-000000000001', true),
    ('f1000000-0000-0000-0000-000000000004', 'Raj Kumar', 'E003', 'raj@hardware.lk', '+94712345681', 'manager', 'f0000000-0000-0000-0000-000000000002', true),
    ('f1000000-0000-0000-0000-000000000005', 'Amelia Fernando', 'E004', 'amelia@hardware.lk', '+94712345682', 'cashier', 'f0000000-0000-0000-0000-000000000002', true)
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- CATEGORIES SEED DATA
-- ============================================================================
INSERT INTO categories (id, name, description, is_active)
VALUES
    ('f2000000-0000-0000-0000-000000000001', 'Hand Tools', 'Manual hand tools for construction and maintenance', true),
    ('f2000000-0000-0000-0000-000000000002', 'Power Tools', 'Electric and cordless power tools', true),
    ('f2000000-0000-0000-0000-000000000003', 'Fasteners', 'Screws, nails, bolts, nuts and washers', true),
    ('f2000000-0000-0000-0000-000000000004', 'Paints & Finishes', 'Interior and exterior paints, varnishes', true),
    ('f2000000-0000-0000-0000-000000000005', 'Electrical Supplies', 'Wiring, switches, outlets and fixtures', true),
    ('f2000000-0000-0000-0000-000000000006', 'Plumbing Materials', 'Pipes, fittings, valves and fixtures', true),
    ('f2000000-0000-0000-0000-000000000007', 'Lumber & Building', 'Wood, plywood and building materials', true),
    ('f2000000-0000-0000-0000-000000000008', 'Safety Equipment', 'Helmets, gloves, masks and safety gear', true)
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- ITEMS SEED DATA
-- ============================================================================
INSERT INTO items (id, code, name, description, category_id, cost_price, retail_price, wholesale_price, barcode, hsn_code, reorder_level, tax_rate, is_active)
VALUES
    -- Hand Tools
    ('f3000000-0000-0000-0000-000000000001', 'HT001', 'Claw Hammer 16oz', 'Standard claw hammer for nails', 'f2000000-0000-0000-0000-000000000001', 450, 799, 650, '1001010101', '82030000', 5, 10, true),
    ('f3000000-0000-0000-0000-000000000002', 'HT002', 'Screwdriver Set 6pc', 'Mixed head screwdriver set', 'f2000000-0000-0000-0000-000000000001', 350, 599, 450, '1001010102', '82030000', 3, 10, true),
    ('f3000000-0000-0000-0000-000000000003', 'HT003', 'Adjustable Wrench 8 inch', 'Adjustable wrench for nuts', 'f2000000-0000-0000-0000-000000000001', 250, 449, 350, '1001010103', '82030000', 4, 10, true),
    
    -- Power Tools
    ('f3000000-0000-0000-0000-000000000004', 'PT001', 'Cordless Drill 20V', 'Powerful cordless drill driver', 'f2000000-0000-0000-0000-000000000002', 3500, 5999, 5200, '1002010101', '84667300', 2, 10, true),
    ('f3000000-0000-0000-0000-000000000005', 'PT002', 'Angle Grinder 4 inch', 'Cutting and grinding machine', 'f2000000-0000-0000-0000-000000000002', 2800, 4799, 4100, '1002010102', '84709100', 2, 10, true),
    
    -- Fasteners
    ('f3000000-0000-0000-0000-000000000006', 'FT001', 'Wood Screws Assorted 100pc', 'Mixed size wood screws', 'f2000000-0000-0000-0000-000000000003', 180, 349, 280, '1003010101', '73181500', 10, 10, true),
    ('f3000000-0000-0000-0000-000000000007', 'FT002', 'Nails 1 inch Box 500pcs', 'Common steel nails', 'f2000000-0000-0000-0000-000000000003', 120, 249, 180, '1003010102', '73159000', 15, 10, true),
    ('f3000000-0000-0000-0000-000000000008', 'FT003', 'Bolts & Nuts Assorted Kit', 'Stainless steel bolts', 'f2000000-0000-0000-0000-000000000003', 280, 549, 420, '1003010103', '73181500', 5, 10, true),
    
    -- Paints
    ('f3000000-0000-0000-0000-000000000009', 'PT003', 'Exterior Paint 1L White', 'Weather resistant exterior paint', 'f2000000-0000-0000-0000-000000000004', 850, 1499, 1200, '1004010101', '32081000', 5, 10, true),
    ('f3000000-0000-0000-0000-000000000010', 'PT004', 'Interior Primer 1L', 'All-purpose interior primer', 'f2000000-0000-0000-0000-000000000004', 420, 799, 600, '1004010102', '32081000', 3, 10, true),
    
    -- Electrical
    ('f3000000-0000-0000-0000-000000000011', 'EL001', 'Electrical Wire 2.5mm 100m', 'Copper electrical wire roll', 'f2000000-0000-0000-0000-000000000005', 2500, 4299, 3800, '1005010101', '85441000', 2, 10, true),
    ('f3000000-0000-0000-0000-000000000012', 'EL002', 'Light Switch Double', 'Plastic light switch module', 'f2000000-0000-0000-0000-000000000005', 180, 349, 280, '1005010102', '85365000', 8, 10, true),
    
    -- Plumbing
    ('f3000000-0000-0000-0000-000000000013', 'PL001', 'PVC Pipe 1 inch 6m', 'PVC water pipe', 'f2000000-0000-0000-0000-000000000006', 1200, 2099, 1750, '1006010101', '39172410', 3, 10, true),
    ('f3000000-0000-0000-0000-000000000014', 'PL002', 'Brass Tap Set', 'Bathroom tap pair', 'f2000000-0000-0000-0000-000000000006', 3200, 5499, 4500, '1006010102', '84810000', 2, 10, true),
    
    -- Lumber
    ('f3000000-0000-0000-0000-000000000015', 'LB001', 'Timber 2x4x8 inch', 'Treated lumber board', 'f2000000-0000-0000-0000-000000000007', 450, 799, 650, '1007010101', '44039010', 5, 10, true),
    ('f3000000-0000-0000-0000-000000000016', 'LB002', 'Plywood 4x8 3/4 inch', 'Pine plywood sheet', 'f2000000-0000-0000-0000-000000000007', 1850, 3199, 2750, '1007010102', '44121190', 2, 10, true),
    
    -- Safety
    ('f3000000-0000-0000-0000-000000000017', 'SF001', 'Safety Helmet Yellow', 'Hard construction helmet', 'f2000000-0000-0000-0000-000000000008', 280, 549, 420, '1008010101', '65070000', 10, 10, true),
    ('f3000000-0000-0000-0000-000000000018', 'SF002', 'Work Gloves Leather', 'Leather work gloves pair', 'f2000000-0000-0000-0000-000000000008', 180, 349, 280, '1008010102', '62031000', 8, 10, true)
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- ITEM STORE STOCK SEED DATA
-- ============================================================================
INSERT INTO item_store_stock (item_id, store_id, quantity_on_hand, reserved_quantity)
SELECT id, 'f0000000-0000-0000-0000-000000000001', 
    CASE 
        WHEN code IN ('HT001', 'HT002', 'HT003') THEN 15
        WHEN code IN ('PT001', 'PT002') THEN 5
        WHEN code IN ('FT001', 'FT002', 'FT003') THEN 50
        WHEN code IN ('PT003', 'PT004') THEN 20
        WHEN code IN ('EL001', 'EL002') THEN 25
        WHEN code IN ('PL001', 'PL002') THEN 12
        WHEN code IN ('LB001', 'LB002') THEN 10
        WHEN code IN ('SF001', 'SF002') THEN 30
    END, 0
FROM items
WHERE is_active = true
ON CONFLICT (item_id, store_id) DO NOTHING;

-- Add stock for other stores
INSERT INTO item_store_stock (item_id, store_id, quantity_on_hand, reserved_quantity)
SELECT id, 'f0000000-0000-0000-0000-000000000002', 
    CASE 
        WHEN code IN ('HT001', 'HT002', 'HT003') THEN 12
        WHEN code IN ('PT001', 'PT002') THEN 3
        WHEN code IN ('FT001', 'FT002', 'FT003') THEN 40
        WHEN code IN ('PT003', 'PT004') THEN 15
        WHEN code IN ('EL001', 'EL002') THEN 20
        WHEN code IN ('PL001', 'PL002') THEN 8
        WHEN code IN ('LB001', 'LB002') THEN 8
        WHEN code IN ('SF001', 'SF002') THEN 25
    END, 0
FROM items
WHERE is_active = true
ON CONFLICT (item_id, store_id) DO NOTHING;

-- ============================================================================
-- CUSTOMERS SEED DATA
-- ============================================================================
INSERT INTO customers (id, name, type, contact_person, phone, email, address, is_active)
VALUES
    ('f4000000-0000-0000-0000-000000000001', 'Walk-In Customer', 'retail', NULL, NULL, NULL, NULL, true),
    ('f4000000-0000-0000-0000-000000000002', 'ABC Construction Ltd', 'wholesale', 'Mr. Amarasiri', '+94712222222', 'info@abcconstruction.lk', '100 Main Street, Colombo', true),
    ('f4000000-0000-0000-0000-000000000003', 'XYZ Builders', 'wholesale', 'Ms. Nirmala', '+94713333333', 'contact@xyzbuilders.lk', '200 High Road, Kandy', true),
    ('f4000000-0000-0000-0000-000000000004', 'John Contractor', 'retail', 'John', '+94714444444', 'john@email.com', '50 Oak Lane, Galle', true)
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- SUPPLIERS SEED DATA
-- ============================================================================
INSERT INTO suppliers (id, name, contact_person, phone, email, address, is_active)
VALUES
    ('f5000000-0000-0000-0000-000000000001', 'Global Hardware Traders', 'Mr. Kumar', '+94115555555', 'sales@globalhw.com', '150 Commerce Park, Colombo', true),
    ('f5000000-0000-0000-0000-000000000002', 'Quality Paints Limited', 'Ms. Jayani', '+94116666666', 'order@qualitypaints.lk', '300 Industrial Zone, Colombo', true),
    ('f5000000-0000-0000-0000-000000000003', 'Electrical Supplies Co', 'Mr. Sunil', '+94117777777', 'supply@elecco.lk', '400 Trade Street, Colombo', true),
    ('f5000000-0000-0000-0000-000000000004', 'Timber & Lumber Mart', 'Mr. Deepak', '+94118888888', 'sales@timbmart.lk', '500 Forest Road, Colombo', true)
ON CONFLICT (id) DO NOTHING;

-- ============================================================================
-- SYSTEM SETTINGS SEED DATA
-- ============================================================================
INSERT INTO system_settings (setting_key, setting_value, setting_type, description)
VALUES
    ('company_name', 'Hardware Shop POS', 'text', 'Company name for receipts'),
    ('receipt_header', 'HARDWARE SHOP', 'text', 'Header text on thermal receipt'),
    ('receipt_footer', 'Thank you for your business!', 'text', 'Footer text on thermal receipt'),
    ('default_discount_percent', '0', 'decimal', 'Default discount percentage'),
    ('tax_enabled', 'true', 'boolean', 'Enable tax calculation'),
    ('default_tax_rate', '10', 'decimal', 'Default tax rate percentage'),
    ('low_stock_threshold', '10', 'integer', 'Quantity threshold for low stock alert'),
    ('currency_symbol', 'Rs.', 'text', 'Currency display symbol'),
    ('currency_code', 'LKR', 'text', 'ISO 4217 currency code'),
    ('invoice_number_prefix', 'INV', 'text', 'Prefix for invoice numbers'),
    ('po_number_prefix', 'PO', 'text', 'Prefix for purchase orders'),
    ('grn_number_prefix', 'GRN', 'text', 'Prefix for goods receipt notes')
ON CONFLICT (setting_key) DO NOTHING;

-- ============================================================================
-- END OF SEED DATA
-- ============================================================================