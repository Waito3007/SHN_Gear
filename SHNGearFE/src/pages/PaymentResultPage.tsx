import { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { orderApi } from '../api/order';
import type { OrderResponse, PaymentStatus } from '../types';
import { CheckCircle2 } from 'lucide-react';

type FlowStatus = 'success' | 'failed' | 'cancel' | 'pending';

function getFlowStatus(raw: string | null): FlowStatus {
  if (raw === 'success' || raw === 'failed' || raw === 'cancel' || raw === 'pending') {
    return raw;
  }

  return 'pending';
}

function paymentStatusText(status: PaymentStatus) {
  switch (status) {
    case 0:
      return 'Pending';
    case 1:
      return 'Completed';
    case 2:
      return 'Failed';
    case 3:
      return 'Refunded';
    default:
      return 'Unknown';
  }
}

function orderStatusText(status: number) {
  switch (status) {
    case 0:
      return 'Pending';
    case 1:
      return 'Confirmed';
    case 2:
      return 'Processing';
    case 3:
      return 'Shipped';
    case 4:
      return 'Delivered';
    case 5:
      return 'Cancelled';
    default:
      return 'Unknown';
  }
}

export default function PaymentResultPage() {
  const [searchParams] = useSearchParams();
  const [order, setOrder] = useState<OrderResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);

  const status = getFlowStatus(searchParams.get('status'));
  const orderId = searchParams.get('orderId');
  const reason = searchParams.get('reason');

  useEffect(() => {
    const loadOrder = async () => {
      if (!orderId) return;

      setLoading(true);
      setLoadError(null);
      try {
        const res = await orderApi.getMyOrderById(orderId);
        setOrder(res.data.data);
      } catch {
        setLoadError('Could not load the latest order status.');
      } finally {
        setLoading(false);
      }
    };

    void loadOrder();
  }, [orderId]);

  const heading = useMemo(() => {
    switch (status) {
      case 'success':
        return 'Payment Successful';
      case 'failed':
        return 'Payment Failed';
      case 'cancel':
        return 'Payment Cancelled';
      default:
        return 'Payment Processing';
    }
  }, [status]);

  const message = useMemo(() => {
    if (status === 'success') {
      return 'Your PayPal payment was approved and your order has been created.';
    }

    if (status === 'failed') {
      return reason || 'Payment could not be completed. Please try again.';
    }

    if (status === 'cancel') {
      return 'You cancelled the PayPal payment. You can return to checkout anytime.';
    }

    return 'We are checking your latest payment state.';
  }, [reason, status]);

  return (
    <div className="max-w-3xl mx-auto px-4 py-12">
      <div className="rounded-xl border border-gray-200 bg-white p-6 md:p-8 space-y-4">
        {status === 'success' && (
          <div className="flex justify-center pb-1">
            <div className="flex h-20 w-20 items-center justify-center rounded-full bg-emerald-50 ring-8 ring-emerald-100/60">
              <CheckCircle2 className="h-11 w-11 text-emerald-500" />
            </div>
          </div>
        )}
        <h1 className="text-2xl md:text-3xl font-bold text-gray-900">{heading}</h1>
        <p className="text-sm md:text-base text-gray-600">{message}</p>

        {loading && <p className="text-sm text-gray-500">Loading transaction status...</p>}
        {loadError && <p className="text-sm text-red-600">{loadError}</p>}

        {order && (
          <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 space-y-2">
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Order Code</span>
              <span className="font-semibold text-gray-900">{order.code}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Payment Status</span>
              <span className="font-semibold text-gray-900">{paymentStatusText(order.paymentStatus)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Order Status</span>
              <span className="font-semibold text-gray-900">{orderStatusText(order.status)}</span>
            </div>
          </div>
        )}

        <div className="flex flex-wrap gap-3 pt-2">
          {orderId ? (
            <Link to={`/orders/${orderId}`} className="px-4 py-2 rounded-lg bg-black text-white text-sm">
              View Order Details
            </Link>
          ) : (
            <Link to="/checkout" className="px-4 py-2 rounded-lg bg-black text-white text-sm">
              Back to Checkout
            </Link>
          )}

          <Link to="/orders" className="px-4 py-2 rounded-lg border border-gray-300 text-sm text-gray-700">
            My Orders
          </Link>
        </div>
      </div>
    </div>
  );
}
