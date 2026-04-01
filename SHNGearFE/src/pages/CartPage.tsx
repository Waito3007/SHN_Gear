import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { cartApi } from '../api/cart';
import { addressApi } from '../api/address';
import { useAuth } from '../context/AuthContext';
import type { AddressDto, CartDto } from '../types';
import { ShoppingCart, Trash2, Plus, Minus, ArrowLeft } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { animateFlyToCart, bumpCartIcon } from '../utils/cartAnimation';

function formatPrice(price: number, currency = 'VND') {
  const locale = currency === 'VND' ? 'vi-VN' : 'en-US';
  return new Intl.NumberFormat(locale, { style: 'currency', currency, maximumFractionDigits: 0 }).format(price);
}

export default function CartPage() {
  const { isAuthenticated } = useAuth();
  const { syncCartCountFromCart } = useCart();
  const navigate = useNavigate();
  const [cart, setCart] = useState<CartDto | null>(null);
  const [defaultAddress, setDefaultAddress] = useState<AddressDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    fetchCart();
  }, [isAuthenticated, navigate]);

  const fetchCart = async () => {
    setLoading(true);
    setActionError(null);
    try {
      const res = await cartApi.getCart();
      setCart(res.data.data);
      syncCartCountFromCart(res.data.data);
    } catch (error) {
      console.error('Failed to fetch cart:', error);
      setActionError('Could not load your cart. Please refresh or sign in again.');
    } finally {
      setLoading(false);
    }
  };

  const fetchDefaultAddress = async () => {
    try {
      const res = await addressApi.getMyAddresses();
      const addresses = res.data.data || [];
      const preferred = addresses.find((item) => item.isDefault) || addresses[0] || null;
      setDefaultAddress(preferred);
    } catch {
      setDefaultAddress(null);
    }
  };

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    fetchDefaultAddress();
  }, [isAuthenticated]);

  const handleUpdateQuantity = async (
    variantId: string,
    newQuantity: number,
    sourceEl?: HTMLElement | null,
    fallbackText?: string,
    previousQuantity?: number
  ) => {
    if (newQuantity < 1 || newQuantity > 99) return;
    
    setUpdating(variantId);
    setActionError(null);
    try {
      const res = await cartApi.updateItem(variantId, { quantity: newQuantity });
      setCart(res.data.data);
      syncCartCountFromCart(res.data.data);

      if ((previousQuantity ?? 0) < newQuantity) {
        animateFlyToCart({ fromElement: sourceEl ?? null, fallbackText: fallbackText || 'P' });
      } else if ((previousQuantity ?? 0) > newQuantity) {
        bumpCartIcon();
      }
    } catch (error) {
      console.error('Failed to update cart:', error);
      setActionError('Could not update item quantity. Please try again.');
    } finally {
      setUpdating(null);
    }
  };

  const handleRemoveItem = async (variantId: string) => {
    setUpdating(variantId);
    setActionError(null);
    try {
      const res = await cartApi.removeItem(variantId);
      setCart(res.data.data);
      syncCartCountFromCart(res.data.data);
      bumpCartIcon();
    } catch (error) {
      console.error('Failed to remove item:', error);
      setActionError('Could not remove this item from your cart.');
    } finally {
      setUpdating(null);
    }
  };

  const handleClearCart = async () => {
    if (!confirm('Are you sure you want to clear your cart?')) return;
    
    setLoading(true);
    setActionError(null);
    try {
      await cartApi.clearCart();
      setCart((prev) => ({
        accountId: prev?.accountId ?? '',
        items: [],
        totalAmount: 0,
        totalItems: 0,
        updatedAt: new Date().toISOString(),
      }));
      syncCartCountFromCart({
        accountId: '',
        items: [],
        totalAmount: 0,
        totalItems: 0,
        updatedAt: new Date().toISOString(),
      });
      bumpCartIcon();
    } catch (error) {
      console.error('Failed to clear cart:', error);
      setActionError('Could not clear cart. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-gray-100 rounded w-1/4" />
          <div className="h-32 bg-gray-100 rounded" />
          <div className="h-32 bg-gray-100 rounded" />
        </div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 text-center">
        <ShoppingCart className="w-16 h-16 text-gray-300 mx-auto mb-4" />
        <h2 className="text-2xl font-bold text-gray-900 mb-2">Your cart is empty</h2>
        <p className="text-gray-500 mb-6">Add some products to get started</p>
        <Link
          to="/products"
          className="inline-flex items-center gap-2 gradient-btn text-white px-6 py-3 rounded-full font-semibold text-sm transition-all hover:shadow-lg"
        >
          <ArrowLeft className="w-4 h-4" />
          Continue Shopping
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Shopping Cart</h1>
          <p className="text-gray-500 mt-1">{cart.totalItems} {cart.totalItems === 1 ? 'item' : 'items'}</p>
        </div>
        {cart.items.length > 0 && (
          <button
            onClick={handleClearCart}
            className="text-sm text-red-600 hover:text-red-700 font-medium"
          >
            Clear Cart
          </button>
        )}
      </div>

      {actionError && (
        <div className="mb-6 p-3 rounded-xl border border-red-200 bg-red-50 text-sm text-red-700">
          {actionError}
        </div>
      )}

      <div className="grid lg:grid-cols-3 gap-8">
        {/* Cart Items */}
        <div className="lg:col-span-2 space-y-4">
          {cart.items.map((item) => (
            <div
              key={item.productVariantId}
              className="bg-white border border-gray-200 rounded-2xl p-6 hover:shadow-md transition-shadow"
            >
              <div className="flex gap-4">
                <div className="flex-1">
                  <h3 className="font-semibold text-gray-900 mb-1">{item.productName}</h3>
                  {item.variantName && (
                    <p className="text-sm text-gray-500 mb-1">{item.variantName}</p>
                  )}
                  <p className="text-xs text-gray-400">SKU: {item.sku}</p>

                  <div className="mt-3 flex items-center gap-3">
                    <div className="flex items-baseline gap-2">
                      <span className="text-lg font-bold text-black">
                        {formatPrice(item.unitPrice, item.currency)}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Quantity Controls */}
                <div className="flex flex-col items-end gap-3">
                  <button
                    onClick={() => handleRemoveItem(item.productVariantId)}
                    disabled={updating === item.productVariantId}
                    className="text-gray-400 hover:text-red-600 transition-colors disabled:opacity-50"
                    title="Remove item"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>

                  <div className="flex items-center border border-gray-300 rounded-lg">
                    <button
                      onClick={(e) =>
                        handleUpdateQuantity(
                          item.productVariantId,
                          item.quantity - 1,
                          e.currentTarget,
                          item.productName.charAt(0),
                          item.quantity
                        )
                      }
                      disabled={updating === item.productVariantId || item.quantity <= 1}
                      className="p-1.5 hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    >
                      <Minus className="w-3 h-3" />
                    </button>
                    <span className="px-4 py-1 font-semibold text-sm min-w-[50px] text-center">
                      {item.quantity}
                    </span>
                    <button
                      onClick={(e) =>
                        handleUpdateQuantity(
                          item.productVariantId,
                          item.quantity + 1,
                          e.currentTarget,
                          item.productName.charAt(0),
                          item.quantity
                        )
                      }
                      disabled={updating === item.productVariantId || item.quantity >= 99 || item.quantity >= item.availableStock}
                      className="p-1.5 hover:bg-gray-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                    >
                      <Plus className="w-3 h-3" />
                    </button>
                  </div>

                  <p className="text-sm font-semibold text-gray-900">
                    {formatPrice(item.subTotal, item.currency)}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <div className="bg-gray-50 rounded-2xl p-6 sticky top-24">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Order Summary</h2>
            
            <div className="space-y-3 mb-6">
              <div className="bg-white border border-gray-200 rounded-xl p-3">
                <div className="flex items-center justify-between gap-2">
                  <span className="text-sm font-semibold text-gray-900">Delivery Address</span>
                  <Link to="/profile" className="text-xs font-medium text-black hover:underline">
                    Manage
                  </Link>
                </div>
                {defaultAddress ? (
                  <div className="mt-2 text-xs text-gray-600 space-y-1">
                    <p className="font-medium text-gray-800">
                      {defaultAddress.recipientName} - {defaultAddress.phoneNumber}
                    </p>
                    <p>
                      {defaultAddress.street}, {defaultAddress.ward}, {defaultAddress.district}, {defaultAddress.province}
                    </p>
                  </div>
                ) : (
                  <p className="mt-2 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-md px-2 py-1.5">
                    No delivery address found. Add one in profile to proceed checkout.
                  </p>
                )}
              </div>

              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Subtotal</span>
                <span className="font-semibold">{formatPrice(cart.totalAmount, cart.items[0]?.currency ?? 'VND')}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Shipping</span>
                <span className="text-green-600 font-medium">Free</span>
              </div>
              <div className="border-t border-gray-200 pt-3 flex justify-between">
                <span className="font-bold text-gray-900">Total</span>
                <span className="font-bold text-xl text-black">
                  {formatPrice(cart.totalAmount, cart.items[0]?.currency ?? 'VND')}
                </span>
              </div>
            </div>

            <button
              onClick={() => navigate('/checkout')}
              disabled={!defaultAddress}
              className="w-full gradient-btn text-white px-6 py-3 rounded-xl font-semibold text-sm transition-all hover:shadow-lg mb-3 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Proceed to Checkout
            </button>

            <Link
              to="/products"
              className="block text-center text-sm text-gray-600 hover:text-black transition-colors"
            >
              Continue Shopping
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
