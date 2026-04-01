import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import RoleForm from './RoleForm';
import { roleApi } from '../../../api/role';
import type { RoleDto } from '../../../types/admin';

export default function RoleEdit() {
  const { id } = useParams<{ id: string }>();
  const [role, setRole] = useState<RoleDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    
    const fetchRole = async () => {
      try {
        const { data } = await roleApi.getById(id);
        setRole(data.data);
      } catch (error) {
        console.error('Failed to load role:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchRole();
  }, [id]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading role...</div>
      </div>
    );
  }

  if (!role) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Role not found</p>
      </div>
    );
  }

  return <RoleForm mode="edit" role={role} />;
}
