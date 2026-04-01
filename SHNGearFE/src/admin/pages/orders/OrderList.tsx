import { useEffect, useState } from 'react';
import { orderApi } from '../../../api/order';
import type { OrderResponse, OrderStatus } from '../../../types';
import { usePermission } from '../../../context/PermissionContext';
import ActionOverlay from '../../../components/ActionOverlay';

function paymentProviderText(provider: number) {
  switch (provider) {
    case 0: return 'COD';
    case 1: return 'VNPay';
    case 2: return 'Stripe';
    case 3: return 'PayPal';
    default: return 'Unknown';
  }
}

function paymentStatusText(status: number) {
  switch (status) {
    case 0: return 'Pending';
    case 1: return 'Paid';
    case 2: return 'Failed';
    case 3: return 'Refunded';
    default: return 'Unknown';
  }
}

function statusText(status: OrderStatus) {
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

const statusOptions: Array<{ value: OrderStatus; label: string }> = [
  { value: 0, label: 'Pending' },
  { value: 1, label: 'Confirmed' },
  { value: 2, label: 'Processing' },
  { value: 3, label: 'Shipped' },
  { value: 4, label: 'Delivered' },
  { value: 5, label: 'Cancelled' },
];

export default function AdminOrderList() {
  const { hasAnyPermission } = usePermission();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updatingId, setUpdatingId] = useState<string | null>(null);
  const [overlayOpen, setOverlayOpen] = useState(false);
  const [overlayPhase, setOverlayPhase] = useState<'loading' | 'success'>('loading');

  const canManage = hasAnyPermission(['order.manage', 'brand.manage', 'category.manage']);

  const loadOrders = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await orderApi.getAdminOrders(undefined, 1, 50);
      setOrders(res.data.data.items || []);
    } catch {
      setError('Failed to load orders');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadOrders();
  }, []);

  const handleStatusChange = async (orderId: string, status: OrderStatus) => {
    if (!canManage) return;
    setUpdatingId(orderId);
    setError(null);
    try {
      const res = await orderApi.updateOrderStatus(orderId, status);
      const updated = res.data.data;
      setOrders((prev) => prev.map((o) => (o.id === updated.id ? updated : o)));
    } catch {
      setError('Update status failed');
    } finally {
      setUpdatingId(null);
    }
  };

  const handleApproveRefund = async (order: OrderResponse) => {
    if (!canManage) return;
    setUpdatingId(order.id);
    setError(null);
    setOverlayPhase('loading');
    setOverlayOpen(true);

    try {
      const res = await orderApi.approveRefund(order.id);
      const updated = res.data.data;
      setOrders((prev) => prev.map((o) => (o.id === updated.id ? updated : o)));

      setOverlayPhase('success');
      await new Promise<void>((resolve) => {
        window.setTimeout(() => resolve(), 900);
      });
    } catch {
      setError('Refund failed');
    } finally {
      setOverlayOpen(false);
      setUpdatingId(null);
    }
  };

  const canShowRefundButton = (order: OrderResponse) => {
    return order.status === 5 && order.paymentProvider === 3 && order.paymentStatus === 1;
  };

  if (loading) {
    return <div className="text-gray-500">Loading orders...</div>;
  }

  return (
    <div>
      <ActionOverlay
        open={overlayOpen}
        phase={overlayPhase}
        loadingText="Dang xu ly hoan tien..."
        successText="Hoan tien thanh cong"
      />
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Orders</h1>
        <p className="text-gray-500 text-sm mt-1">Admin order management</p>
      </div>

      {error && <p className="text-sm text-red-600 mb-3">{error}</p>}

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 text-left text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">Code</th>
              <th className="px-4 py-3">Account</th>
              <th className="px-4 py-3">Total</th>
              <th className="px-4 py-3">Payment Method</th>
              <th className="px-4 py-3">Payment Status</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3">Created</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id} className="border-t border-gray-100">
                <td className="px-4 py-3 font-medium text-gray-900">{order.code}</td>
                <td className="px-4 py-3 text-gray-700">{order.accountId}</td>
                <td className="px-4 py-3 text-gray-700">{new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(order.totalAmount)}</td>
                <td className="px-4 py-3 text-gray-700">{paymentProviderText(order.paymentProvider)}</td>
                <td className="px-4 py-3 text-gray-700">{paymentStatusText(order.paymentStatus)}</td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap items-center gap-2">
                    {canManage ? (
                      <select
                        value={order.status}
                        onChange={(e) => handleStatusChange(order.id, Number(e.target.value) as OrderStatus)}
                        disabled={updatingId === order.id}
                        className="text-sm border border-gray-300 rounded px-2 py-1"
                      >
                        {statusOptions.map((opt) => (
                          <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                      </select>
                    ) : (
                      <span className="text-gray-700">{statusText(order.status)}</span>
                    )}

                    {canManage && canShowRefundButton(order) && (
                      <button
                        type="button"
                        onClick={() => void handleApproveRefund(order)}
                        disabled={updatingId === order.id}
                        className="rounded bg-emerald-600 px-2 py-1 text-xs font-medium text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
                      >
                        Refund
                      </button>
                    )}
                  </div>
                </td>
                <td className="px-4 py-3 text-gray-600">{new Date(order.createdAt).toLocaleString('vi-VN')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
