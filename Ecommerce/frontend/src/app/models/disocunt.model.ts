export enum DiscountType {
  Percentage = 'Percentage',
  Flat = 'Flat'
}

export enum DiscountScope {
  Common = 'Common',
  Vendor = 'Vendor',
  Category = 'Category',
  Product = 'Product'
}

export interface PageRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface CreateDiscountRequest {
  productId?: number;
  categoryId?: number;
  scope: string; 
  type: string; 
  value: number;
  minOrderValue: number;
  usageLimit: number;
  expiresAt: string | Date;
}

export interface UpdateDiscountRequest {
  value: number;
  minOrderValue: number;
  usageLimit: number;
  expiresAt: string | Date;
}

export interface DiscountResponse {
  id: number;
  vendorId?: number;
  productId?: number;
  categoryId?: number;
  code: string;
  scope: DiscountScope;
  type: DiscountType;
  value: number;
  minOrderValue: number;
  usageLimit: number;
  usedCount: number;
  isActive: boolean;
  expiresAt: string | Date;
}

export interface ToggleDiscountStatusResponse {
  id: number;
  code: string;
  isActive: boolean;
  expiredAt: string | Date;
}

export interface DiscountFilterRequest {
  pageNumber: number;
  pageSize: number;
  discountType?: DiscountType;
  categoryId?: number;
  productId?: number;
  vendorId?: number;
  minValue?: number;
  sortBy?: string;
  sortOrder?: string;
  searchQuery?: string;
}

export interface DiscountCartResponse {
  code: string;
  type: DiscountType;
  value: number;
}

export interface CartItemEvaluationResponse {
  id: number;
  vendorId: number;
  categoryId: number;
  productId: number;
  subTotal: number;
}

export interface CartEvaluationRequest {
  subTotal: number;
  items: CartItemEvaluationResponse[];
}