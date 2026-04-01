import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { orderApi } from '../api/order';
import type { OrderResponse } from '../types';

function formatPrice(price: number) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(price);
}

function statusText(status: number) {
  switch (status) {
    case 0: return 'Pending';
    case 1: return 'Confirmed';
    case 2: return 'Processing';
    case 3: return 'Shipped';
    case 4: return 'Delivered';
    case 5: return 'Cancelled';
    default: return 'Unknown';
  }
}

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<OrderResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      setLoading(true);
      setError(null);
      try {
        const res = await orderApi.getMyOrderById(id);
        setOrder(res.data.data);
      } catch {
        setError('Failed to load order detail');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  const canCancel = useMemo(() => {
    if (!order) return false;
    return order.status === 0 || order.status === 1;
  }, [order]);

  const handleCancelOrder = async () => {
    if (!id || !canCancel) return;

    setCancelling(true);
    setError(null);
    try {
      const res = await orderApi.cancelMyOrder(id, 'Cancelled by customer');
      setOrder(res.data.data);
    } catch {
      setError('Cancel order failed');
    } finally {
      setCancelling(false);
    }
  };

  if (loading) {
    return <div className="max-w-5xl mx-auto px-4 py-10 text-gray-500">Loading order detail...</div>;
  }

  if (!order) {
    return (
      <div className="max-w-5xl mx-auto px-4 py-10">
        <p className="text-gray-600">Order not found.</p>
        <Link to="/orders" className="text-blue-600 hover:underline">Back to orders</Link>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-10 space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Order {order.code}</h1>
          <p className="text-sm text-gray-600 mt-1">Created: {new Date(order.createdAt).toLocaleString('vi-VN')}</p>
        </div>
        <div className="text-right">
          <p className="text-sm text-gray-600">Status</p>
          <p className="font-semibold text-gray-900">{statusText(order.status)}</p>
        </div>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 text-left text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">Product</th>
              <th className="px-4 py-3">SKU</th>
              <th className="px-4 py-3">Price</th>
              <th className="px-4 py-3">Qty</th>
              <th className="px-4 py-3">Subtotal</th>
            </tr>
          </thead>
          <tbody>
            {order.items.map((item) => (
              <tr key={item.id} className="border-t border-gray-100">
                <td className="px-4 py-3">
                  <div className="font-medium text-gray-900">{item.productName}</div>
                  <div className="text-xs text-gray-500">{item.variantName}</div>
                </td>
                <td className="px-4 py-3 text-gray-700">{item.sku}</td>
                <td className="px-4 py-3 text-gray-700">{formatPrice(item.unitPrice)}</td>
                <td className="px-4 py-3 text-gray-700">{item.quantity}</td>
                <td className="px-4 py-3 text-gray-900 font-medium">{formatPrice(item.subTotal)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-6">
        <div className="space-y-2 text-sm max-w-sm ml-auto">
          <div className="flex justify-between">
            <span className="text-gray-600">Subtotal</span>
            <span className="font-medium">{formatPrice(order.subTotal)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">Shipping</span>
            <span className="font-medium">{formatPrice(order.shippingFee)}</span>
          </div>
          <div className="border-t border-gray-200 pt-2 flex justify-between">
            <span className="font-semibold">Total</span>
            <span className="font-bold">{formatPrice(order.totalAmount)}</span>
          </div>
        </div>

        <div className="mt-6 flex items-center justify-between">
          <Link to="/orders" className="text-sm text-blue-600 hover:underline">
            Back to orders
          </Link>

          {canCancel && (
            <button
              onClick={handleCancelOrder}
              disabled={cancelling}
              className="px-4 py-2 rounded-lg bg-red-600 text-white disabled:opacity-50"
            >
              {cancelling ? 'Cancelling...' : 'Cancel Order'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
