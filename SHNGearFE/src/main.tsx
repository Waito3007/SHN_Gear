import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App'
import { AuthProvider } from './context/AuthContext'
import { PermissionProvider } from './context/PermissionContext'
import { CartProvider } from './context/CartContext'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <CartProvider>
        <PermissionProvider>
          <App />
        </PermissionProvider>
      </CartProvider>
    </AuthProvider>
  </StrictMode>,
)
