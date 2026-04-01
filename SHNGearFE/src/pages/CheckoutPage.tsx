import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { cartApi } from '../api/cart';
import { addressApi } from '../api/address';
import { orderApi } from '../api/order';
import type { AddressDto, CartDto, PaymentProvider } from '../types';

function formatPrice(price: number, currency = 'VND') {
  const locale = currency === 'VND' ? 'vi-VN' : 'en-US';
  return new Intl.NumberFormat(locale, { style: 'currency', currency, maximumFractionDigits: 0 }).format(price);
}

export default function CheckoutPage() {
  const navigate = useNavigate();
  const [cart, setCart] = useState<CartDto | null>(null);
  const [addresses, setAddresses] = useState<AddressDto[]>([]);
  const [selectedAddressId, setSelectedAddressId] = useState<string>('');
  const [paymentProvider, setPaymentProvider] = useState<PaymentProvider>(0);
  const [note, setNote] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const [cartRes, addressRes] = await Promise.all([cartApi.getCart(), addressApi.getMyAddresses()]);
        const cartData = cartRes.data.data;
        const addressData = addressRes.data.data || [];

        setCart(cartData);
        setAddresses(addressData);

        const preferred = addressData.find((a) => a.isDefault) || addressData[0];
        setSelectedAddressId(preferred?.id || '');
      } catch {
        setError('Could not load checkout data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const canCheckout = useMemo(() => {
    return !!cart && cart.items.length > 0 && !!selectedAddressId && !submitting;
  }, [cart, selectedAddressId, submitting]);

  const handlePlaceOrder = async () => {
    if (!canCheckout || !cart) return;

    setSubmitting(true);
    setError(null);

    try {
      const res = await orderApi.checkout({
        deliveryAddressId: selectedAddressId,
        paymentProvider,
        note: note.trim() || undefined,
      });

      const createdOrder = res.data.data;
      navigate(`/orders/${createdOrder.id}`);
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Place order failed. Please try again.';
      setError(message);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <div className="max-w-5xl mx-auto px-4 py-10 text-gray-500">Loading checkout...</div>;
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="max-w-5xl mx-auto px-4 py-10">
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <h1 className="text-2xl font-bold text-gray-900">Checkout</h1>
          <p className="mt-2 text-gray-600">Your cart is empty.</p>
          <Link to="/cart" className="inline-block mt-4 text-blue-600 hover:underline">
            Back to cart
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto px-4 py-10 grid lg:grid-cols-3 gap-6">
      <div className="lg:col-span-2 space-y-6">
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Checkout</h1>

          <label className="block text-sm font-semibold text-gray-700 mb-2">Delivery Address</label>
          {addresses.length === 0 ? (
            <div className="text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-md px-3 py-2">
              You have no saved address. Please add one in your profile.
            </div>
          ) : (
            <div className="space-y-2">
              {addresses.map((address) => (
                <label key={address.id} className="flex gap-3 items-start border border-gray-200 rounded-lg p-3 cursor-pointer">
                  <input
                    type="radio"
                    name="address"
                    checked={selectedAddressId === address.id}
                    onChange={() => setSelectedAddressId(address.id)}
                    className="mt-1"
                  />
                  <div className="text-sm">
                    <div className="font-medium text-gray-900">
                      {address.recipientName} - {address.phoneNumber} {address.isDefault ? '(Default)' : ''}
                    </div>
                    <div className="text-gray-600">
                      {address.street}, {address.ward}, {address.district}, {address.province}
                    </div>
                  </div>
                </label>
              ))}
            </div>
          )}

          <label className="block text-sm font-semibold text-gray-700 mt-6 mb-2">Payment Method</label>
          <div className="border border-gray-200 rounded-lg p-3">
            <label className="flex items-center gap-3 text-sm">
              <input
                type="radio"
                checked={paymentProvider === 0}
                onChange={() => setPaymentProvider(0)}
              />
              Cash on Delivery (COD)
            </label>
          </div>

          <label className="block text-sm font-semibold text-gray-700 mt-6 mb-2">Order Note (optional)</label>
          <textarea
            value={note}
            onChange={(e) => setNote(e.target.value)}
            rows={3}
            maxLength={500}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
            placeholder="Example: call before delivery"
          />

          {error && <p className="text-sm text-red-600 mt-3">{error}</p>}
        </div>
      </div>

      <div className="lg:col-span-1">
        <div className="rounded-xl border border-gray-200 bg-white p-6 sticky top-24">
          <h2 className="text-lg font-bold text-gray-900 mb-4">Order Summary</h2>
          <div className="space-y-2 text-sm mb-4">
            <div className="flex justify-between">
              <span className="text-gray-600">Items</span>
              <span className="font-medium">{cart.totalItems}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Subtotal</span>
              <span className="font-medium">{formatPrice(cart.totalAmount, cart.items[0]?.currency || 'VND')}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Shipping</span>
              <span className="font-medium">{formatPrice(0, cart.items[0]?.currency || 'VND')}</span>
            </div>
            <div className="border-t border-gray-200 pt-2 flex justify-between">
              <span className="font-semibold text-gray-900">Total</span>
              <span className="font-bold text-gray-900">{formatPrice(cart.totalAmount, cart.items[0]?.currency || 'VND')}</span>
            </div>
          </div>

          <button
            onClick={handlePlaceOrder}
            disabled={!canCheckout}
            className="w-full px-4 py-2 rounded-lg bg-black text-white disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {submitting ? 'Placing order...' : 'Place Order'}
          </button>

          <Link to="/cart" className="block text-center mt-3 text-sm text-gray-600 hover:text-black">
            Back to cart
          </Link>
        </div>
      </div>
    </div>
  );
}
