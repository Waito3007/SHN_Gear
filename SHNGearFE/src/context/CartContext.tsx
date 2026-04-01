import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { cartApi } from '../api/cart';
import type { CartDto } from '../types';
import { useAuth } from './AuthContext';

interface CartContextType {
  cartItemCount: number;
  refreshCartCount: () => Promise<void>;
  syncCartCountFromCart: (cart: CartDto | null | undefined) => void;
}

const CartContext = createContext<CartContextType | null>(null);

export function CartProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const [cartItemCount, setCartItemCount] = useState(0);
  const prevAuthStateRef = useRef(isAuthenticated);

  const syncCartCountFromCart = useCallback((cart: CartDto | null | undefined) => {
    const totalItems = cart?.items?.reduce((sum, item) => sum + item.quantity, 0) ?? 0;
    setCartItemCount(totalItems);
  }, []);

  const refreshCartCount = useCallback(async () => {
    if (!isAuthenticated) {
      return;
    }

    try {
      const res = await cartApi.getCart();
      syncCartCountFromCart(res.data.data);
    } catch {
      // Cart badge is non-blocking UI. Keep previous count on transient failure.
    }
  }, [isAuthenticated, syncCartCountFromCart]);

  useEffect(() => {
    if (prevAuthStateRef.current === isAuthenticated) {
      return;
    }
    prevAuthStateRef.current = isAuthenticated;

    if (!isAuthenticated) {
      setCartItemCount(0);
    } else {
      refreshCartCount();
    }
  }, [isAuthenticated, refreshCartCount]);

  const value = useMemo(
    () => ({ cartItemCount, refreshCartCount, syncCartCountFromCart }),
    [cartItemCount, refreshCartCount, syncCartCountFromCart]
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used within CartProvider');
  return ctx;
}
