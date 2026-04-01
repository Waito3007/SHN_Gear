import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Navigate } from 'react-router-dom';
import Layout from './pages/Layout';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import VerifyEmailOtpPage from './pages/VerifyEmailOtpPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ProductsPage from './pages/ProductsPage';
import ProductDetailPage from './pages/ProductDetailPage';
import SearchPage from './pages/SearchPage';
import CartPage from './pages/CartPage';
import ProfilePage from './pages/ProfilePage';
import CheckoutPage from './pages/CheckoutPage';
import PaymentResultPage from './pages/PaymentResultPage';
import OrdersPage from './pages/OrdersPage';
import OrderDetailPage from './pages/OrderDetailPage';
import ProtectedRoute from './components/ProtectedRoute';
import PublicRoute from './components/PublicRoute';
import AdminLayout from './admin/layout/AdminLayout';
import AdminDashboard from './admin/pages/Dashboard';
import Unauthorized from './admin/pages/Unauthorized';
import AccountList from './admin/pages/accounts/AccountList';
import AccountRoles from './admin/pages/accounts/AccountRoles';
import RoleList from './admin/pages/roles/RoleList';
import RoleCreate from './admin/pages/roles/RoleCreate';
import RoleEdit from './admin/pages/roles/RoleEdit';
import RolePermissionsManager from './admin/pages/roles/RolePermissionsManager';
import PermissionList from './admin/pages/permissions/PermissionList';
import PermissionCreate from './admin/pages/permissions/PermissionCreate';
import PermissionEdit from './admin/pages/permissions/PermissionEdit';
import ProductList from './admin/pages/products/ProductList';
import ProductCreate from './admin/pages/products/ProductCreate';
import ProductEdit from './admin/pages/products/ProductEdit';
import BrandList from './admin/pages/brands/BrandList';
import BrandCreate from './admin/pages/brands/BrandCreate';
import BrandEdit from './admin/pages/brands/BrandEdit';
import CategoryList from './admin/pages/categories/CategoryList';
import CategoryCreate from './admin/pages/categories/CategoryCreate';
import CategoryEdit from './admin/pages/categories/CategoryEdit';
import AdminOrderList from './admin/pages/orders/OrderList';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route element={<Layout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
          <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />
          <Route path="/verify-email-otp" element={<PublicRoute><VerifyEmailOtpPage /></PublicRoute>} />
          <Route path="/forgot-password" element={<PublicRoute><ForgotPasswordPage /></PublicRoute>} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/product/:slug" element={<ProductDetailPage />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/profile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
          <Route path="/checkout" element={<ProtectedRoute><CheckoutPage /></ProtectedRoute>} />
          <Route path="/payment-result" element={<ProtectedRoute><PaymentResultPage /></ProtectedRoute>} />
          <Route path="/orders" element={<ProtectedRoute><OrdersPage /></ProtectedRoute>} />
          <Route path="/orders/:id" element={<ProtectedRoute><OrderDetailPage /></ProtectedRoute>} />
        </Route>
        
        {/* Admin routes */}
        <Route path="/admin" element={
          <ProtectedRoute requireAdmin>
            <AdminLayout />
          </ProtectedRoute>
        }>
          <Route index element={<Navigate to="/admin/dashboard" replace />} />
          <Route path="dashboard" element={<AdminDashboard />} />
          <Route path="unauthorized" element={<Unauthorized />} />
          
          {/* Account Management */}
          <Route path="accounts" element={
            <ProtectedRoute permission="account.view">
              <AccountList />
            </ProtectedRoute>
          } />
          <Route path="accounts/:id/roles" element={
            <ProtectedRoute permission="account.update">
              <AccountRoles />
            </ProtectedRoute>
          } />
          
          {/* Role Management */}
          <Route path="roles" element={
            <ProtectedRoute permission="role.view">
              <RoleList />
            </ProtectedRoute>
          } />
          <Route path="roles/create" element={
            <ProtectedRoute permission="role.create">
              <RoleCreate />
            </ProtectedRoute>
          } />
          <Route path="roles/edit/:id" element={
            <ProtectedRoute permission="role.update">
              <RoleEdit />
            </ProtectedRoute>
          } />
          <Route path="roles/:id/permissions" element={
            <ProtectedRoute permission="role.update">
              <RolePermissionsManager />
            </ProtectedRoute>
          } />
          
          {/* Permission Management */}
          <Route path="permissions" element={
            <ProtectedRoute permission="permission.view">
              <PermissionList />
            </ProtectedRoute>
          } />
          <Route path="permissions/create" element={
            <ProtectedRoute permission="permission.assign">
              <PermissionCreate />
            </ProtectedRoute>
          } />
          <Route path="permissions/edit/:id" element={
            <ProtectedRoute permission="permission.assign">
              <PermissionEdit />
            </ProtectedRoute>
          } />
          
          {/* Product Management */}
          <Route path="products" element={
            <ProtectedRoute permission="product.view">
              <ProductList />
            </ProtectedRoute>
          } />
          <Route path="products/create" element={
            <ProtectedRoute permission="product.create">
              <ProductCreate />
            </ProtectedRoute>
          } />
          <Route path="products/edit/:id" element={
            <ProtectedRoute permission="product.update">
              <ProductEdit />
            </ProtectedRoute>
          } />

          {/* Brand Management */}
          <Route path="brands" element={
            <ProtectedRoute permission="brand.view">
              <BrandList />
            </ProtectedRoute>
          } />
          <Route path="brands/create" element={
            <ProtectedRoute permission="brand.manage">
              <BrandCreate />
            </ProtectedRoute>
          } />
          <Route path="brands/edit/:id" element={
            <ProtectedRoute permission="brand.manage">
              <BrandEdit />
            </ProtectedRoute>
          } />

          {/* Category Management */}
          <Route path="categories" element={
            <ProtectedRoute permission="category.view">
              <CategoryList />
            </ProtectedRoute>
          } />
          <Route path="categories/create" element={
            <ProtectedRoute permission="category.manage">
              <CategoryCreate />
            </ProtectedRoute>
          } />
          <Route path="categories/edit/:id" element={
            <ProtectedRoute permission="category.manage">
              <CategoryEdit />
            </ProtectedRoute>
          } />

          {/* Order Management */}
          <Route path="orders" element={
            <ProtectedRoute>
              <AdminOrderList />
            </ProtectedRoute>
          } />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
