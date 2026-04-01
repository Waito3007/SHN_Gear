import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { categoryApi } from '../../../api/category';
import type { CategoryDto } from '../../../types';
import CategoryForm from './CategoryForm';
import toast from 'react-hot-toast';

export default function CategoryEdit() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [category, setCategory] = useState<CategoryDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadCategory = async () => {
      if (!id) {
        navigate('/admin/categories');
        return;
      }

      try {
        setLoading(true);
        const response = await categoryApi.getById(id);
        const found = response.data?.data;
        if (!found) {
          toast.error('Category not found');
          navigate('/admin/categories');
          return;
        }
        setCategory(found);
      } catch {
        toast.error('Failed to load category');
        navigate('/admin/categories');
      } finally {
        setLoading(false);
      }
    };

    loadCategory();
  }, [id, navigate]);

  if (loading) {
    return <div className="text-gray-500">Loading category...</div>;
  }

  return category ? <CategoryForm mode="edit" category={category} /> : null;
}
