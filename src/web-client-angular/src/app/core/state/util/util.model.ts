export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface ListParams {
  pageNumber: number;
  pageSize: number;
}
