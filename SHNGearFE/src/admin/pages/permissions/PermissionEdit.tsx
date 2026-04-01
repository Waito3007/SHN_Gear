import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import PermissionForm from './PermissionForm';
import { permissionApi } from '../../../api/permission';
import type { PermissionDto } from '../../../types/admin';

export default function PermissionEdit() {
  const { id } = useParams<{ id: string }>();
  const [permission, setPermission] = useState<PermissionDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    
    const fetchPermission = async () => {
      try {
        const { data } = await permissionApi.getById(id);
        setPermission(data.data);
      } catch (error) {
        console.error('Failed to load permission:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPermission();
  }, [id]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading permission...</div>
      </div>
    );
  }

  if (!permission) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Permission not found</p>
      </div>
    );
  }

  return <PermissionForm mode="edit" permission={permission} />;
}
