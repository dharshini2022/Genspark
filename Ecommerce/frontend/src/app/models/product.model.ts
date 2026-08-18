export interface ProductImageResponse {
  id: number;
  variantId: number;
  imageUrl: string;
  imageOrder: number;
}

export interface ProductVariantResponse {
  id: number;
  productId: number;
  stockQty: number;
  price: number;
  isDefault: boolean;
  isActive: boolean;
  orderCount: number;
  availableValues: { [key: string]: string };
  variantImages: ProductImageResponse[];
}

export interface ProductResponse {
  id: number;
  vendorId: number;
  categoryId: number;
  name: string;
  description?: string;
  status: string; 
  createdAt: string;
  storeName: string;
  categoryName: string;
  averageRating: number;
  reviewCount: number;
  variants: ProductVariantResponse[];
}

export interface PageResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface ProductWithDiscount extends ProductResponse {
  discountLabel?: string;
  discountedPrice?: number;
  discountPercent?: number;
}

export interface ProductFilterRequest {
  pageNumber?: number;
  pageSize?: number;
  categoryId?: number;
  sortBy?: string;
  sortOrder?: string;
  searchQuery?: string;
  minPrice?: number;
  maxPrice?: number;
}

export interface CreateProductRequest {
  categoryId: number;
  name: string;
  description: string;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  categoryId?: number;
}

export interface AddProductVariantRequest {
  stockQty: number;
  price: number;
  isDefault?: boolean;
  availableValues: { [key: string]: string };
}

export interface UpdateProductVariantRequest {
  price?: number;
  stockQty?: number;
  availableValues?: { [key: string]: string };
  isActive?: boolean;
  isDefault?: boolean;
}

export interface CreateProductImageRequest {
  imageUrl: string;
  imageOrder: number;
}

export interface ProductSearchResult {
  id: number;
  name: string;
}
