import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  loading?: boolean;
}

export default function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  hasPreviousPage,
  hasNextPage,
  loading = false,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  // Generate page numbers with ellipsis
  const pageNumbers = [];
  const showEllipsisLeft = currentPage > 3;
  const showEllipsisRight = currentPage < totalPages - 2;

  if (showEllipsisLeft) {
    pageNumbers.push(1);
    pageNumbers.push('...');
  }

  const startPage = Math.max(1, currentPage - 2);
  const endPage = Math.min(totalPages, currentPage + 2);

  for (let i = startPage; i <= endPage; i++) {
    if (!pageNumbers.includes(i)) {
      pageNumbers.push(i);
    }
  }

  if (showEllipsisRight) {
    pageNumbers.push('...');
    pageNumbers.push(totalPages);
  }

  if (!showEllipsisLeft && !showEllipsisRight) {
    // Fill in gaps if no ellipsis
    for (let i = 1; i <= totalPages; i++) {
      if (!pageNumbers.includes(i)) {
        pageNumbers.push(i);
      }
    }
  }

  return (
    <div className="flex items-center justify-center gap-2 mt-12">
      <button
        disabled={!hasPreviousPage || loading}
        onClick={() => onPageChange(currentPage - 1)}
        className="p-2 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
        aria-label="Previous page"
      >
        <ChevronLeft className="w-5 h-5" />
      </button>

      {pageNumbers.map((page, idx) =>
        page === '...' ? (
          <span key={`ellipsis-${idx}`} className="px-1 text-gray-400">
            ...
          </span>
        ) : (
          <button
            key={page}
            onClick={() => onPageChange(page as number)}
            disabled={loading}
            className={`w-10 h-10 rounded-lg text-sm font-medium transition-colors ${
              page === currentPage
                ? 'bg-black text-white'
                : 'border border-gray-200 hover:bg-gray-50 disabled:opacity-30'
            }`}
          >
            {page}
          </button>
        )
      )}

      <button
        disabled={!hasNextPage || loading}
        onClick={() => onPageChange(currentPage + 1)}
        className="p-2 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
        aria-label="Next page"
      >
        <ChevronRight className="w-5 h-5" />
      </button>
    </div>
  );
}
