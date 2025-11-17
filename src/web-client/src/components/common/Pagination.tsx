
import React from 'react';

interface PaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (newPage: number) => void;
}

export const Pagination: React.FC<PaginationProps> = ({ pageNumber, pageSize, totalCount, onPageChange }) => {
  const totalPages = Math.ceil(totalCount / pageSize);

  if (totalPages <= 1) return null;

  const handlePrevious = () => {
    if (pageNumber > 1) {
      onPageChange(pageNumber - 1);
    }
  };

  const handleNext = () => {
    if (pageNumber < totalPages) {
      onPageChange(pageNumber + 1);
    }
  };

  return (
    <div className="flex items-center justify-between mt-4">
      <span className="text-sm text-gray-400">
        Showing page {pageNumber} of {totalPages} ({totalCount} items)
      </span>
      <div className="flex items-center gap-2">
        <button
          onClick={handlePrevious}
          disabled={pageNumber === 1}
          className="px-3 py-1 bg-gray-700 text-white rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-600 transition-colors"
        >
          Previous
        </button>
        <button
          onClick={handleNext}
          disabled={pageNumber === totalPages}
          className="px-3 py-1 bg-gray-700 text-white rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-600 transition-colors"
        >
          Next
        </button>
      </div>
    </div>
  );
};
