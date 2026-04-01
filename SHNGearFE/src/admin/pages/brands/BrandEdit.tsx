import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { brandApi } from '../../../api/brand';
import type { BrandDto } from '../../../types';
import BrandForm from './BrandForm';
import toast from 'react-hot-toast';

export default function BrandEdit() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [brand, setBrand] = useState<BrandDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadBrand = async () => {
      if (!id) {
        navigate('/admin/brands');
        return;
      }

      try {
        setLoading(true);
        const response = await brandApi.getById(id);
        const found = response.data?.data;
        if (!found) {
          toast.error('Brand not found');
          navigate('/admin/brands');
          return;
        }
        setBrand(found);
      } catch {
        toast.error('Failed to load brand');
        navigate('/admin/brands');
      } finally {
        setLoading(false);
      }
    };

    loadBrand();
  }, [id, navigate]);

  if (loading) {
    return <div className="text-gray-500">Loading brand...</div>;
  }

  return brand ? <BrandForm mode="edit" brand={brand} /> : null;
}
