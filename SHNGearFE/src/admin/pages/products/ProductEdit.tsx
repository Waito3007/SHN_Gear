import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import ProductForm from './ProductForm';
import { productApi } from '../../../api/product';
import type { ProductDetail } from '../../../types';

export default function ProductEdit() {
  const { id } = useParams<{ id: string }>();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) {
      setLoading(false);
      return;
    }

    const fetchProduct = async () => {
      try {
        const response = await productApi.getById(id);
        setProduct(response.data);
      } catch (error) {
        console.error('Failed to load product:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading product...</div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Product not found</p>
      </div>
    );
  }

  return <ProductForm mode="edit" product={product} />;
}
