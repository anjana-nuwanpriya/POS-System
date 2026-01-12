# Hardware Shop POS - Database Setup Guide

## Overview

This folder contains all database scripts for the Hardware Shop POS system using PostgreSQL. These scripts define the complete schema and provide sample data for development and testing.

## Files

- **schema.sql** - Complete database schema with all tables, indexes, and constraints
- **seed_data.sql** - Sample data for development and testing
- **README.md** - This file

## Prerequisites

- PostgreSQL 12 or higher installed
- PostgreSQL client tools (`psql` command-line tool)
- Sufficient permissions to create databases and users

## Installation Steps

### Step 1: Create Database

Open PostgreSQL command prompt and create the database:

```sql
CREATE DATABASE hardware_shop;
```

### Step 2: Apply Schema

Apply the schema to create all tables:

```bash
psql -U postgres -d hardware_shop -f schema.sql
```

Or from within psql:

```sql
\c hardware_shop
\i schema.sql
```

### Step 3: Load Sample Data (Optional)

Load sample data for development:

```bash
psql -U postgres -d hardware_shop -f seed_data.sql
```

Or from within psql:

```sql
\i seed_data.sql
```

### Step 4: Verify Installation

Check if tables were created successfully:

```bash
psql -U postgres -d hardware_shop -c "\dt"
```

You should see 39 tables listed.

## Database Structure

### Core Tables

#### Stores & Employees
- `stores` - Store locations/branches
- `employees` - Staff members with roles and permissions

#### Products
- `items` - Product catalog
- `categories` - Product categories
- `item_store_stock` - Stock levels per store

#### Sales
- `sales_retail` - Retail sales transactions
- `sales_retail_items` - Line items for retail sales
- `sales_wholesale` - Wholesale sales transactions
- `sales_wholesale_items` - Line items for wholesale sales
- `sales_returns` - Customer returns
- `sales_return_items` - Line items for returns

#### Purchasing
- `suppliers` - Supplier information
- `purchase_orders` - Purchase orders to suppliers
- `purchase_order_items` - Line items for POs
- `purchase_grns` - Goods receipt notes
- `purchase_grn_items` - Line items for GRNs
- `purchase_returns` - Returns to suppliers
- `purchase_return_items` - Line items for supplier returns

#### Inventory
- `inventory_transactions` - All stock movements
- `opening_stock_entries` - Opening stock entries
- `opening_stock_items` - Items in opening stock
- `stock_adjustments` - Stock corrections
- `stock_adjustment_items` - Adjustment details

#### Customers & Payments
- `customers` - Customer master data
- `customer_payments` - Payments received from customers
- `customer_payment_allocations` - Payment allocations to invoices
- `customer_opening_balances` - Opening balance for credit

#### Suppliers & Payments
- `supplier_payments` - Payments to suppliers
- `supplier_payment_allocations` - Payment allocations
- `supplier_opening_balances` - Opening balance for credit

#### Quotations & Dispatch
- `quotations` - Sales quotations
- `quotation_items` - Quotation line items
- `item_dispatch_notes` - Inter-store dispatch
- `item_dispatch_items` - Dispatch items

#### Audit & Settings
- `audit_logs` - Complete audit trail
- `system_settings` - System configuration

## Connection String

Update your application configuration with:

```
Host=localhost
Port=5432
Database=hardware_shop
Username=postgres
Password=your_password
```

For .NET applications in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hardware_shop;Username=postgres;Password=your_password"
  }
}
```

## Default Data

### Admin User
- **Employee Code**: admin
- **Name**: Administrator
- **Role**: admin

### Default Store
- **Code**: STORE001
- **Name**: Main Store - Colombo

### Sample Products
The database includes 18 sample products across 8 categories for testing.

### Sample Customers & Suppliers
Includes demo customers and suppliers for testing transactions.

## Key Features

### 1. Multi-Store Support
- Separate stock levels per store
- Store-wise sales reporting
- Inter-store dispatches

### 2. Complete Audit Trail
- All user actions logged
- Before/after values stored
- User and timestamp tracking

### 3. Flexible Pricing
- Retail, wholesale, and cost prices
- Per-item tax configuration
- Category-based organization

### 4. Inventory Management
- Real-time stock tracking
- Low stock alerts (reorder level)
- Batch and expiry tracking
- Stock adjustments with reasons

### 5. Financial Management
- Customer credit limits and balances
- Supplier payment tracking
- Payment allocations
- Opening balances

### 6. Sales & Returns
- Retail and wholesale sales
- Complete return management
- Quotation system
- Invoice tracking

## Important Notes

⚠️ **Do NOT modify schema.sql directly in production**

When schema changes are needed:
1. Create a new migration script
2. Document the changes
3. Test in development first
4. Apply carefully in production with backups

## Resetting the Database

To completely reset (development only):

```bash
# Drop the database
psql -U postgres -c "DROP DATABASE hardware_shop;"

# Create fresh
psql -U postgres -c "CREATE DATABASE hardware_shop;"

# Reapply schema
psql -U postgres -d hardware_shop -f schema.sql

# Load sample data
psql -U postgres -d hardware_shop -f seed_data.sql
```

## Backup & Restore

### Backup
```bash
pg_dump -U postgres -d hardware_shop > backup_hardware_shop.sql
```

### Restore
```bash
psql -U postgres -d hardware_shop < backup_hardware_shop.sql
```

## Troubleshooting

### Error: "database does not exist"
- Ensure you created the database first
- Check database name spelling

### Error: "permission denied"
- Verify PostgreSQL user permissions
- Check user has CREATE TABLE privileges

### Table already exists
- Schema already applied
- Use `DROP DATABASE` if you want to reset

### Connection refused
- PostgreSQL service not running
- Check host/port settings
- Firewall may be blocking port 5432

## Performance Tips

1. Regular backups
2. Monitor index usage
3. Archive old audit logs periodically
4. Update table statistics: `ANALYZE;`
5. Vacuum regularly: `VACUUM;`

## Support

For issues or questions:
1. Check PostgreSQL logs
2. Verify connection strings
3. Ensure all prerequisites are met
4. Test schema.sql separately

## Version History

- **v1.0** - Initial schema (2026-01-12)
  - 39 tables
  - Complete POS functionality
  - Multi-store support
  - Audit logging

---

**Last Updated**: 2026-01-12
**Database Version**: 1.0
**PostgreSQL Version**: 12+