import { Link } from 'react-router-dom';

export default function Unauthorized() {
  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-gray-800 mb-4">403</h1>
        <h2 className="text-2xl font-semibold text-gray-700 mb-4">
          Unauthorized Access
        </h2>
        <p className="text-gray-600 mb-8">
          You don't have permission to access this resource.
        </p>
        <Link
          to="/admin/dashboard"
          className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          Go to Dashboard
        </Link>
      </div>
    </div>
  );
}
