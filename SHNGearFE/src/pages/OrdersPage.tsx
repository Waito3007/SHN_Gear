import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
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

export default function OrdersPage() {
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await orderApi.getMyOrders(1, 20);
        setOrders(res.data.data.items || []);
      } catch {
        setError('Failed to load orders');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  if (loading) {
    return <div className="max-w-5xl mx-auto px-4 py-10 text-gray-500">Loading orders...</div>;
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-10">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">My Orders</h1>

      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}

      {orders.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-xl p-6 text-gray-600">
          You have no orders yet.
        </div>
      ) : (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 text-left text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Payment</th>
                <th className="px-4 py-3">Total</th>
                <th className="px-4 py-3">Created</th>
                <th className="px-4 py-3 text-right">Action</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id} className="border-t border-gray-100">
                  <td className="px-4 py-3 font-medium text-gray-900">{order.code}</td>
                  <td className="px-4 py-3 text-gray-700">{statusText(order.status)}</td>
                  <td className="px-4 py-3 text-gray-700">{order.paymentStatus === 1 ? 'Paid' : 'Pending'}</td>
                  <td className="px-4 py-3 text-gray-700">{formatPrice(order.totalAmount)}</td>
                  <td className="px-4 py-3 text-gray-600">{new Date(order.createdAt).toLocaleString('vi-VN')}</td>
                  <td className="px-4 py-3 text-right">
                    <Link to={`/orders/${order.id}`} className="text-blue-600 hover:underline">View</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
