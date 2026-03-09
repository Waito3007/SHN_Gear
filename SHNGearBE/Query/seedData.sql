-- =====================================================
-- SHNGear Database Seed Script (Fixed UUIDs)
-- Run this in pgAdmin or psql after migrations
-- =====================================================

-- =====================================================
-- 1. PERMISSIONS (Quyền hạn)
-- =====================================================
INSERT INTO "Permissions" ("Id", "Name", "Description", "CreateAt", "UpdateAt", "IsDelete") VALUES
-- Account permissions
('a1000001-0000-0000-0000-000000000001', 'account.view', 'View account list', NOW(), NULL, false),
('a1000001-0000-0000-0000-000000000002', 'account.create', 'Create new account', NOW(), NULL, false),
('a1000001-0000-0000-0000-000000000003', 'account.update', 'Update account info', NOW(), NULL, false),
('a1000001-0000-0000-0000-000000000004', 'account.delete', 'Delete account', NOW(), NULL, false),
-- Role permissions
('a1000002-0000-0000-0000-000000000001', 'role.view', 'View role list', NOW(), NULL, false),
('a1000002-0000-0000-0000-000000000002', 'role.create', 'Create new role', NOW(), NULL, false),
('a1000002-0000-0000-0000-000000000003', 'role.update', 'Update role', NOW(), NULL, false),
('a1000002-0000-0000-0000-000000000004', 'role.delete', 'Delete role', NOW(), NULL, false),
-- Permission management
('a1000003-0000-0000-0000-000000000001', 'permission.view', 'View permission list', NOW(), NULL, false),
('a1000003-0000-0000-0000-000000000002', 'permission.assign', 'Assign permissions to roles', NOW(), NULL, false),
-- Product permissions
('a1000004-0000-0000-0000-000000000001', 'product.view', 'View product list', NOW(), NULL, false),
('a1000004-0000-0000-0000-000000000002', 'product.create', 'Create new product', NOW(), NULL, false),
('a1000004-0000-0000-0000-000000000003', 'product.update', 'Update product', NOW(), NULL, false),
('a1000004-0000-0000-0000-000000000004', 'product.delete', 'Delete product', NOW(), NULL, false),
-- Category permissions
('a1000005-0000-0000-0000-000000000001', 'category.view', 'View category list', NOW(), NULL, false),
('a1000005-0000-0000-0000-000000000002', 'category.manage', 'Manage categories', NOW(), NULL, false),
-- Brand permissions
('a1000006-0000-0000-0000-000000000001', 'brand.view', 'View brand list', NOW(), NULL, false),
('a1000006-0000-0000-0000-000000000002', 'brand.manage', 'Manage brands', NOW(), NULL, false);

-- =====================================================
-- 2. ROLES (Vai trò)
-- =====================================================
INSERT INTO "Roles" ("Id", "Name", "Description", "CreateAt", "UpdateAt", "IsDelete") VALUES
('b1000001-0000-0000-0000-000000000001', 'SuperAdmin', 'Full system access', NOW(), NULL, false),
('b1000001-0000-0000-0000-000000000002', 'Admin', 'Administrative access', NOW(), NULL, false),
('b1000001-0000-0000-0000-000000000003', 'Manager', 'Product and order management', NOW(), NULL, false),
('b1000001-0000-0000-0000-000000000004', 'Staff', 'Basic staff access', NOW(), NULL, false),
('b1000001-0000-0000-0000-000000000005', 'Customer', 'Customer access', NOW(), NULL, false);

-- =====================================================
-- 3. ROLE_PERMISSIONS (Gán quyền cho vai trò)
-- =====================================================
-- SuperAdmin: All permissions
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") 
SELECT 'b1000001-0000-0000-0000-000000000001', "Id" FROM "Permissions";

-- Admin: All except role/permission management
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES
('b1000001-0000-0000-0000-000000000002', 'a1000001-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000002', 'a1000001-0000-0000-0000-000000000002'),
('b1000001-0000-0000-0000-000000000002', 'a1000001-0000-0000-0000-000000000003'),
('b1000001-0000-0000-0000-000000000002', 'a1000004-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000002', 'a1000004-0000-0000-0000-000000000002'),
('b1000001-0000-0000-0000-000000000002', 'a1000004-0000-0000-0000-000000000003'),
('b1000001-0000-0000-0000-000000000002', 'a1000004-0000-0000-0000-000000000004'),
('b1000001-0000-0000-0000-000000000002', 'a1000005-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000002', 'a1000005-0000-0000-0000-000000000002'),
('b1000001-0000-0000-0000-000000000002', 'a1000006-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000002', 'a1000006-0000-0000-0000-000000000002');

-- Manager: Product management only
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES
('b1000001-0000-0000-0000-000000000003', 'a1000004-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000003', 'a1000004-0000-0000-0000-000000000002'),
('b1000001-0000-0000-0000-000000000003', 'a1000004-0000-0000-0000-000000000003'),
('b1000001-0000-0000-0000-000000000003', 'a1000005-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000003', 'a1000006-0000-0000-0000-000000000001');

-- Staff: View only
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES
('b1000001-0000-0000-0000-000000000004', 'a1000004-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000004', 'a1000005-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000004', 'a1000006-0000-0000-0000-000000000001');

-- Customer: View products only
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES
('b1000001-0000-0000-0000-000000000005', 'a1000004-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000005', 'a1000005-0000-0000-0000-000000000001'),
('b1000001-0000-0000-0000-000000000005', 'a1000006-0000-0000-0000-000000000001');

-- =====================================================
-- 4. ACCOUNTS (Tài khoản)
-- Password: Admin@123 (hash with BCrypt)
-- =====================================================
INSERT INTO "Accounts" ("Id", "Username", "Email", "PasswordHash", "Salt", "CreateAt", "UpdateAt", "IsDelete") VALUES
('c1000001-0000-0000-0000-000000000001', 'superadmin', 'superadmin@shngear.com', '$2a$11$Kp5qOPOHPGOBp.Chn3v9p.9DQe7P4GQBm9hZz7qZ2tGVJcPL5Gx4W', 'randomsalt1', NOW(), NULL, false),
('c1000001-0000-0000-0000-000000000002', 'admin', 'admin@shngear.com', '$2a$11$Kp5qOPOHPGOBp.Chn3v9p.9DQe7P4GQBm9hZz7qZ2tGVJcPL5Gx4W', 'randomsalt2', NOW(), NULL, false),
('c1000001-0000-0000-0000-000000000003', 'manager', 'manager@shngear.com', '$2a$11$Kp5qOPOHPGOBp.Chn3v9p.9DQe7P4GQBm9hZz7qZ2tGVJcPL5Gx4W', 'randomsalt3', NOW(), NULL, false),
('c1000001-0000-0000-0000-000000000004', 'staff', 'staff@shngear.com', '$2a$11$Kp5qOPOHPGOBp.Chn3v9p.9DQe7P4GQBm9hZz7qZ2tGVJcPL5Gx4W', 'randomsalt4', NOW(), NULL, false),
('c1000001-0000-0000-0000-000000000005', 'customer1', 'customer1@gmail.com', '$2a$11$Kp5qOPOHPGOBp.Chn3v9p.9DQe7P4GQBm9hZz7qZ2tGVJcPL5Gx4W', 'randomsalt5', NOW(), NULL, false);

-- =====================================================
-- 5. ACCOUNT_DETAILS (Thông tin chi tiết)
-- =====================================================
INSERT INTO "AccountDetails" ("Id", "AccountId", "FirstName", "Name", "PhoneNumber", "Address", "CreateAt", "UpdateAt", "IsDelete") VALUES
('d1000001-0000-0000-0000-000000000001', 'c1000001-0000-0000-0000-000000000001', 'Super', 'Admin', '0901234567', 'HCM City, Vietnam', NOW(), NULL, false),
('d1000001-0000-0000-0000-000000000002', 'c1000001-0000-0000-0000-000000000002', 'System', 'Admin', '0901234568', 'HCM City, Vietnam', NOW(), NULL, false),
('d1000001-0000-0000-0000-000000000003', 'c1000001-0000-0000-0000-000000000003', 'Product', 'Manager', '0901234569', 'Ha Noi, Vietnam', NOW(), NULL, false),
('d1000001-0000-0000-0000-000000000004', 'c1000001-0000-0000-0000-000000000004', 'Sales', 'Staff', '0901234570', 'Da Nang, Vietnam', NOW(), NULL, false),
('d1000001-0000-0000-0000-000000000005', 'c1000001-0000-0000-0000-000000000005', 'Nguyen', 'Van A', '0901234571', '123 Le Loi, Q1, HCM', NOW(), NULL, false);

-- =====================================================
-- 6. ACCOUNT_ROLES (Gán vai trò cho tài khoản)
-- =====================================================
INSERT INTO "AccountRoles" ("AccountId", "RoleId") VALUES
('c1000001-0000-0000-0000-000000000001', 'b1000001-0000-0000-0000-000000000001'),
('c1000001-0000-0000-0000-000000000002', 'b1000001-0000-0000-0000-000000000002'),
('c1000001-0000-0000-0000-000000000003', 'b1000001-0000-0000-0000-000000000003'),
('c1000001-0000-0000-0000-000000000004', 'b1000001-0000-0000-0000-000000000004'),
('c1000001-0000-0000-0000-000000000005', 'b1000001-0000-0000-0000-000000000005');

-- =====================================================
-- 7. CATEGORIES (Danh mục sản phẩm)
-- =====================================================
INSERT INTO "Categories" ("Id", "Name", "Slug", "ParentCategoryId", "CreateAt", "UpdateAt", "IsDelete") VALUES
-- Main categories
('e1000001-0000-0000-0000-000000000001', 'Gaming Gear', 'gaming-gear', NULL, NOW(), NULL, false),
('e1000001-0000-0000-0000-000000000002', 'PC Components', 'pc-components', NULL, NOW(), NULL, false),
('e1000001-0000-0000-0000-000000000003', 'Laptops', 'laptops', NULL, NOW(), NULL, false),
('e1000001-0000-0000-0000-000000000004', 'Accessories', 'accessories', NULL, NOW(), NULL, false),
-- Gaming Gear subcategories
('e1000002-0000-0000-0000-000000000001', 'Gaming Mouse', 'gaming-mouse', 'e1000001-0000-0000-0000-000000000001', NOW(), NULL, false),
('e1000002-0000-0000-0000-000000000002', 'Gaming Keyboard', 'gaming-keyboard', 'e1000001-0000-0000-0000-000000000001', NOW(), NULL, false),
('e1000002-0000-0000-0000-000000000003', 'Gaming Headset', 'gaming-headset', 'e1000001-0000-0000-0000-000000000001', NOW(), NULL, false),
('e1000002-0000-0000-0000-000000000004', 'Gaming Mousepad', 'gaming-mousepad', 'e1000001-0000-0000-0000-000000000001', NOW(), NULL, false),
-- PC Components subcategories
('e1000003-0000-0000-0000-000000000001', 'Graphics Cards', 'graphics-cards', 'e1000001-0000-0000-0000-000000000002', NOW(), NULL, false),
('e1000003-0000-0000-0000-000000000002', 'Processors', 'processors', 'e1000001-0000-0000-0000-000000000002', NOW(), NULL, false),
('e1000003-0000-0000-0000-000000000003', 'RAM', 'ram', 'e1000001-0000-0000-0000-000000000002', NOW(), NULL, false),
('e1000003-0000-0000-0000-000000000004', 'Storage', 'storage', 'e1000001-0000-0000-0000-000000000002', NOW(), NULL, false);

-- =====================================================
-- 8. BRANDS (Thương hiệu)
-- =====================================================
INSERT INTO "Brands" ("Id", "Name", "Description", "CreateAt", "UpdateAt", "IsDelete") VALUES
('f1000001-0000-0000-0000-000000000001', 'Logitech', 'Swiss-American technology company', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000002', 'Razer', 'American-Singaporean gaming hardware company', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000003', 'SteelSeries', 'Danish gaming peripherals brand', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000004', 'Corsair', 'American computer peripherals and hardware company', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000005', 'HyperX', 'Gaming division of Kingston Technology', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000006', 'ASUS', 'Taiwanese multinational computer hardware company', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000007', 'MSI', 'Taiwanese multinational corporation', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000008', 'NVIDIA', 'American multinational technology company', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000009', 'AMD', 'American multinational semiconductor company', NOW(), NULL, false),
('f1000001-0000-0000-0000-000000000010', 'Intel', 'American multinational corporation', NOW(), NULL, false);

-- =====================================================
-- 9. TAGS (Thẻ sản phẩm) - Fixed: a-f hex only
-- =====================================================
INSERT INTO "Tags" ("Id", "Name", "CreateAt", "UpdateAt", "IsDelete") VALUES
('aa000001-0000-0000-0000-000000000001', 'bestseller', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000002', 'new-arrival', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000003', 'hot-deal', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000004', 'limited-edition', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000005', 'rgb', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000006', 'wireless', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000007', 'esports', NOW(), NULL, false),
('aa000001-0000-0000-0000-000000000008', 'streaming', NOW(), NULL, false);

-- =====================================================
-- 10. PRODUCT_ATTRIBUTE_DEFINITIONS (Định nghĩa thuộc tính)
-- =====================================================
INSERT INTO "ProductAttributeDefinitions" ("Id", "Name", "DataType", "CreateAt", "UpdateAt", "IsDelete") VALUES
('ab000001-0000-0000-0000-000000000001', 'Color', 3, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000002', 'Size', 3, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000003', 'Weight', 1, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000004', 'DPI', 1, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000005', 'Connectivity', 3, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000006', 'Switch Type', 3, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000007', 'RGB Lighting', 2, NOW(), NULL, false),
('ab000001-0000-0000-0000-000000000008', 'Warranty', 0, NOW(), NULL, false);

-- =====================================================
-- 11. PRODUCTS (Sản phẩm)
-- =====================================================
INSERT INTO "Products" ("Id", "Code", "Name", "Slug", "Description", "CategoryId", "BrandId", "IsFeatured", "SoldCount", "ViewCount", "CreateAt", "UpdateAt", "IsDelete") VALUES
-- Gaming Mice
('ac000001-0000-0000-0000-000000000001', 'LG-GPW2', 'Logitech G Pro X Superlight 2', 'logitech-g-pro-x-superlight-2', 'Ultra-lightweight wireless gaming mouse with HERO 2 sensor, 60g weight', 'e1000002-0000-0000-0000-000000000001', 'f1000001-0000-0000-0000-000000000001', true, 150, 2500, NOW(), NULL, false),
('ac000001-0000-0000-0000-000000000002', 'RZ-DAV3', 'Razer DeathAdder V3 Pro', 'razer-deathadder-v3-pro', 'Ergonomic esports mouse with Focus Pro 30K sensor', 'e1000002-0000-0000-0000-000000000001', 'f1000001-0000-0000-0000-000000000002', true, 120, 1800, NOW(), NULL, false),
('ac000001-0000-0000-0000-000000000003', 'SS-PRIME', 'SteelSeries Prime Wireless', 'steelseries-prime-wireless', 'Pro-grade wireless mouse with TrueMove Air sensor', 'e1000002-0000-0000-0000-000000000001', 'f1000001-0000-0000-0000-000000000003', false, 80, 1200, NOW(), NULL, false),
-- Gaming Keyboards
('ac000001-0000-0000-0000-000000000004', 'LG-G915', 'Logitech G915 TKL', 'logitech-g915-tkl', 'Wireless mechanical gaming keyboard with low-profile switches', 'e1000002-0000-0000-0000-000000000002', 'f1000001-0000-0000-0000-000000000001', true, 90, 1600, NOW(), NULL, false),
('ac000001-0000-0000-0000-000000000005', 'RZ-HV3P', 'Razer Huntsman V3 Pro', 'razer-huntsman-v3-pro', 'Analog optical gaming keyboard with adjustable actuation', 'e1000002-0000-0000-0000-000000000002', 'f1000001-0000-0000-0000-000000000002', true, 75, 1400, NOW(), NULL, false),
('ac000001-0000-0000-0000-000000000006', 'CR-K70P', 'Corsair K70 RGB Pro', 'corsair-k70-rgb-pro', 'Mechanical gaming keyboard with Cherry MX switches', 'e1000002-0000-0000-0000-000000000002', 'f1000001-0000-0000-0000-000000000004', false, 110, 1900, NOW(), NULL, false),
-- Gaming Headsets
('ac000001-0000-0000-0000-000000000007', 'LG-GPH', 'Logitech G Pro X 2', 'logitech-g-pro-x-2', 'Professional gaming headset with Blue VO!CE technology', 'e1000002-0000-0000-0000-000000000003', 'f1000001-0000-0000-0000-000000000001', true, 130, 2100, NOW(), NULL, false),
('ac000001-0000-0000-0000-000000000008', 'SS-NOVA', 'SteelSeries Arctis Nova Pro', 'steelseries-arctis-nova-pro', 'Premium hi-res gaming headset with active noise cancellation', 'e1000002-0000-0000-0000-000000000003', 'f1000001-0000-0000-0000-000000000003', true, 65, 980, NOW(), NULL, false),
('ac000001-0000-0000-0000-000000000009', 'HX-CLDII', 'HyperX Cloud II Wireless', 'hyperx-cloud-ii-wireless', '7.1 surround sound wireless gaming headset', 'e1000002-0000-0000-0000-000000000003', 'f1000001-0000-0000-0000-000000000005', false, 200, 3200, NOW(), NULL, false),
-- Mousepads
('ac000001-0000-0000-0000-000000000010', 'LG-G640', 'Logitech G640 Large', 'logitech-g640-large', 'Large cloth gaming mousepad with consistent surface texture', 'e1000002-0000-0000-0000-000000000004', 'f1000001-0000-0000-0000-000000000001', false, 250, 1500, NOW(), NULL, false);

-- =====================================================
-- 12. PRODUCT_IMAGES (Hình ảnh sản phẩm)
-- =====================================================
INSERT INTO "ProductImages" ("Id", "ProductId", "Url", "IsPrimary", "SortOrder", "CreateAt", "UpdateAt", "IsDelete") VALUES
('ad000001-0000-0000-0000-000000000001', 'ac000001-0000-0000-0000-000000000001', '/images/products/gpw2-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000002', 'ac000001-0000-0000-0000-000000000001', '/images/products/gpw2-side.jpg', false, 1, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000003', 'ac000001-0000-0000-0000-000000000002', '/images/products/dav3-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000004', 'ac000001-0000-0000-0000-000000000003', '/images/products/prime-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000005', 'ac000001-0000-0000-0000-000000000004', '/images/products/g915-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000006', 'ac000001-0000-0000-0000-000000000005', '/images/products/hv3p-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000007', 'ac000001-0000-0000-0000-000000000006', '/images/products/k70p-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000008', 'ac000001-0000-0000-0000-000000000007', '/images/products/gpx2-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000009', 'ac000001-0000-0000-0000-000000000008', '/images/products/nova-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000010', 'ac000001-0000-0000-0000-000000000009', '/images/products/cldii-main.jpg', true, 0, NOW(), NULL, false),
('ad000001-0000-0000-0000-000000000011', 'ac000001-0000-0000-0000-000000000010', '/images/products/g640-main.jpg', true, 0, NOW(), NULL, false);

-- =====================================================
-- 13. PRODUCT_VARIANTS (Biến thể sản phẩm)
-- =====================================================
INSERT INTO "ProductVariants" ("Id", "ProductId", "Sku", "Name", "Quantity", "ReservedStock", "SafetyStock", "CreateAt", "UpdateAt", "IsDelete") VALUES
-- Logitech G Pro X Superlight 2
('ae000001-0000-0000-0000-000000000001', 'ac000001-0000-0000-0000-000000000001', 'LG-GPW2-BLK', 'Black', 100, 10, 5, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000002', 'ac000001-0000-0000-0000-000000000001', 'LG-GPW2-WHT', 'White', 80, 5, 5, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000003', 'ac000001-0000-0000-0000-000000000001', 'LG-GPW2-PNK', 'Pink', 50, 3, 5, NOW(), NULL, false),
-- Razer DeathAdder V3 Pro
('ae000001-0000-0000-0000-000000000004', 'ac000001-0000-0000-0000-000000000002', 'RZ-DAV3-BLK', 'Black', 120, 15, 5, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000005', 'ac000001-0000-0000-0000-000000000002', 'RZ-DAV3-WHT', 'White', 90, 10, 5, NOW(), NULL, false),
-- SteelSeries Prime
('ae000001-0000-0000-0000-000000000006', 'ac000001-0000-0000-0000-000000000003', 'SS-PRIME-BLK', 'Black', 70, 5, 5, NOW(), NULL, false),
-- Logitech G915 TKL
('ae000001-0000-0000-0000-000000000007', 'ac000001-0000-0000-0000-000000000004', 'LG-G915-CLK', 'Clicky', 40, 3, 3, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000008', 'ac000001-0000-0000-0000-000000000004', 'LG-G915-TAC', 'Tactile', 50, 5, 3, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000009', 'ac000001-0000-0000-0000-000000000004', 'LG-G915-LIN', 'Linear', 60, 5, 3, NOW(), NULL, false),
-- Razer Huntsman V3 Pro
('ae000001-0000-0000-0000-000000000010', 'ac000001-0000-0000-0000-000000000005', 'RZ-HV3P-US', 'US Layout', 45, 5, 3, NOW(), NULL, false),
-- Corsair K70 Pro
('ae000001-0000-0000-0000-000000000011', 'ac000001-0000-0000-0000-000000000006', 'CR-K70P-RED', 'Cherry MX Red', 80, 10, 5, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000012', 'ac000001-0000-0000-0000-000000000006', 'CR-K70P-BRN', 'Cherry MX Brown', 70, 8, 5, NOW(), NULL, false),
-- Logitech G Pro X 2
('ae000001-0000-0000-0000-000000000013', 'ac000001-0000-0000-0000-000000000007', 'LG-GPH-BLK', 'Black', 90, 10, 5, NOW(), NULL, false),
-- SteelSeries Arctis Nova Pro
('ae000001-0000-0000-0000-000000000014', 'ac000001-0000-0000-0000-000000000008', 'SS-NOVA-BLK', 'Black', 35, 3, 3, NOW(), NULL, false),
-- HyperX Cloud II
('ae000001-0000-0000-0000-000000000015', 'ac000001-0000-0000-0000-000000000009', 'HX-CLDII-BLK', 'Black', 150, 20, 10, NOW(), NULL, false),
('ae000001-0000-0000-0000-000000000016', 'ac000001-0000-0000-0000-000000000009', 'HX-CLDII-RED', 'Red', 100, 15, 10, NOW(), NULL, false),
-- Logitech G640
('ae000001-0000-0000-0000-000000000017', 'ac000001-0000-0000-0000-000000000010', 'LG-G640-STD', 'Standard', 200, 20, 10, NOW(), NULL, false);

-- =====================================================
-- 14. PRODUCT_VARIANT_PRICES (Giá sản phẩm - VND)
-- =====================================================
INSERT INTO "ProductVariantPrices" ("Id", "ProductVariantId", "Currency", "BasePrice", "SalePrice", "ValidFrom", "ValidTo", "CreateAt", "UpdateAt", "IsDelete") VALUES
-- Logitech G Pro X Superlight 2
('af000001-0000-0000-0000-000000000001', 'ae000001-0000-0000-0000-000000000001', 'VND', 3990000, 3690000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000002', 'ae000001-0000-0000-0000-000000000002', 'VND', 3990000, 3690000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000003', 'ae000001-0000-0000-0000-000000000003', 'VND', 4190000, NULL, NOW(), NULL, NOW(), NULL, false),
-- Razer DeathAdder V3 Pro
('af000001-0000-0000-0000-000000000004', 'ae000001-0000-0000-0000-000000000004', 'VND', 3690000, 3290000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000005', 'ae000001-0000-0000-0000-000000000005', 'VND', 3690000, NULL, NOW(), NULL, NOW(), NULL, false),
-- SteelSeries Prime
('af000001-0000-0000-0000-000000000006', 'ae000001-0000-0000-0000-000000000006', 'VND', 2990000, 2490000, NOW(), NULL, NOW(), NULL, false),
-- Logitech G915 TKL
('af000001-0000-0000-0000-000000000007', 'ae000001-0000-0000-0000-000000000007', 'VND', 5490000, 4990000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000008', 'ae000001-0000-0000-0000-000000000008', 'VND', 5490000, 4990000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000009', 'ae000001-0000-0000-0000-000000000009', 'VND', 5490000, NULL, NOW(), NULL, NOW(), NULL, false),
-- Razer Huntsman V3 Pro
('af000001-0000-0000-0000-000000000010', 'ae000001-0000-0000-0000-000000000010', 'VND', 6490000, 5990000, NOW(), NULL, NOW(), NULL, false),
-- Corsair K70 Pro
('af000001-0000-0000-0000-000000000011', 'ae000001-0000-0000-0000-000000000011', 'VND', 4290000, 3890000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000012', 'ae000001-0000-0000-0000-000000000012', 'VND', 4290000, NULL, NOW(), NULL, NOW(), NULL, false),
-- Logitech G Pro X 2
('af000001-0000-0000-0000-000000000013', 'ae000001-0000-0000-0000-000000000013', 'VND', 5990000, 5490000, NOW(), NULL, NOW(), NULL, false),
-- SteelSeries Arctis Nova Pro
('af000001-0000-0000-0000-000000000014', 'ae000001-0000-0000-0000-000000000014', 'VND', 8990000, NULL, NOW(), NULL, NOW(), NULL, false),
-- HyperX Cloud II
('af000001-0000-0000-0000-000000000015', 'ae000001-0000-0000-0000-000000000015', 'VND', 2490000, 1990000, NOW(), NULL, NOW(), NULL, false),
('af000001-0000-0000-0000-000000000016', 'ae000001-0000-0000-0000-000000000016', 'VND', 2490000, 1990000, NOW(), NULL, NOW(), NULL, false),
-- Logitech G640
('af000001-0000-0000-0000-000000000017', 'ae000001-0000-0000-0000-000000000017', 'VND', 890000, 690000, NOW(), NULL, NOW(), NULL, false);

-- =====================================================
-- 15. PRODUCT_TAGS (Gắn tag cho sản phẩm)
-- =====================================================
INSERT INTO "ProductTags" ("ProductId", "TagId") VALUES
('ac000001-0000-0000-0000-000000000001', 'aa000001-0000-0000-0000-000000000001'),
('ac000001-0000-0000-0000-000000000001', 'aa000001-0000-0000-0000-000000000002'),
('ac000001-0000-0000-0000-000000000001', 'aa000001-0000-0000-0000-000000000006'),
('ac000001-0000-0000-0000-000000000001', 'aa000001-0000-0000-0000-000000000007'),
('ac000001-0000-0000-0000-000000000002', 'aa000001-0000-0000-0000-000000000001'),
('ac000001-0000-0000-0000-000000000002', 'aa000001-0000-0000-0000-000000000006'),
('ac000001-0000-0000-0000-000000000002', 'aa000001-0000-0000-0000-000000000007'),
('ac000001-0000-0000-0000-000000000004', 'aa000001-0000-0000-0000-000000000005'),
('ac000001-0000-0000-0000-000000000004', 'aa000001-0000-0000-0000-000000000006'),
('ac000001-0000-0000-0000-000000000005', 'aa000001-0000-0000-0000-000000000002'),
('ac000001-0000-0000-0000-000000000005', 'aa000001-0000-0000-0000-000000000005'),
('ac000001-0000-0000-0000-000000000006', 'aa000001-0000-0000-0000-000000000003'),
('ac000001-0000-0000-0000-000000000006', 'aa000001-0000-0000-0000-000000000005'),
('ac000001-0000-0000-0000-000000000007', 'aa000001-0000-0000-0000-000000000007'),
('ac000001-0000-0000-0000-000000000007', 'aa000001-0000-0000-0000-000000000008'),
('ac000001-0000-0000-0000-000000000008', 'aa000001-0000-0000-0000-000000000004'),
('ac000001-0000-0000-0000-000000000009', 'aa000001-0000-0000-0000-000000000001'),
('ac000001-0000-0000-0000-000000000009', 'aa000001-0000-0000-0000-000000000003'),
('ac000001-0000-0000-0000-000000000010', 'aa000001-0000-0000-0000-000000000003');

-- =====================================================
-- 16. PRODUCT_ATTRIBUTES (Thuộc tính sản phẩm)
-- =====================================================
INSERT INTO "ProductAttributes" ("Id", "ProductId", "ProductAttributeDefinitionId", "Value", "CreateAt", "UpdateAt", "IsDelete") VALUES
-- Logitech G Pro X Superlight 2
('ba000001-0000-0000-0000-000000000001', 'ac000001-0000-0000-0000-000000000001', 'ab000001-0000-0000-0000-000000000003', '60', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000002', 'ac000001-0000-0000-0000-000000000001', 'ab000001-0000-0000-0000-000000000004', '32000', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000003', 'ac000001-0000-0000-0000-000000000001', 'ab000001-0000-0000-0000-000000000005', 'Wireless/USB', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000004', 'ac000001-0000-0000-0000-000000000001', 'ab000001-0000-0000-0000-000000000008', '2 years', NOW(), NULL, false),
-- Razer DeathAdder V3 Pro
('ba000001-0000-0000-0000-000000000005', 'ac000001-0000-0000-0000-000000000002', 'ab000001-0000-0000-0000-000000000003', '63', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000006', 'ac000001-0000-0000-0000-000000000002', 'ab000001-0000-0000-0000-000000000004', '30000', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000007', 'ac000001-0000-0000-0000-000000000002', 'ab000001-0000-0000-0000-000000000005', 'Wireless/USB', NOW(), NULL, false),
-- Logitech G915 TKL
('ba000001-0000-0000-0000-000000000008', 'ac000001-0000-0000-0000-000000000004', 'ab000001-0000-0000-0000-000000000005', 'Wireless/Bluetooth/USB', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000009', 'ac000001-0000-0000-0000-000000000004', 'ab000001-0000-0000-0000-000000000007', 'true', NOW(), NULL, false),
-- HyperX Cloud II
('ba000001-0000-0000-0000-000000000010', 'ac000001-0000-0000-0000-000000000009', 'ab000001-0000-0000-0000-000000000005', 'Wireless 2.4GHz', NOW(), NULL, false),
('ba000001-0000-0000-0000-000000000011', 'ac000001-0000-0000-0000-000000000009', 'ab000001-0000-0000-0000-000000000008', '2 years', NOW(), NULL, false);

-- =====================================================
-- DONE! Verify data
-- =====================================================
SELECT 'Permissions' as "Table", COUNT(*) as "Count" FROM "Permissions"
UNION ALL SELECT 'Roles', COUNT(*) FROM "Roles"
UNION ALL SELECT 'RolePermissions', COUNT(*) FROM "RolePermissions"
UNION ALL SELECT 'Accounts', COUNT(*) FROM "Accounts"
UNION ALL SELECT 'AccountDetails', COUNT(*) FROM "AccountDetails"
UNION ALL SELECT 'AccountRoles', COUNT(*) FROM "AccountRoles"
UNION ALL SELECT 'Categories', COUNT(*) FROM "Categories"
UNION ALL SELECT 'Brands', COUNT(*) FROM "Brands"
UNION ALL SELECT 'Tags', COUNT(*) FROM "Tags"
UNION ALL SELECT 'Products', COUNT(*) FROM "Products"
UNION ALL SELECT 'ProductImages', COUNT(*) FROM "ProductImages"
UNION ALL SELECT 'ProductVariants', COUNT(*) FROM "ProductVariants"
UNION ALL SELECT 'ProductVariantPrices', COUNT(*) FROM "ProductVariantPrices"
UNION ALL SELECT 'ProductTags', COUNT(*) FROM "ProductTags"
UNION ALL SELECT 'ProductAttributes', COUNT(*) FROM "ProductAttributes";