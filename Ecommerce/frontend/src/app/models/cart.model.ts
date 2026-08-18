export interface CartItemResponse {
  id: number;
  variantId: number;
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  subTotal: number;
  isInStock: boolean;
  categoryName?: string;
  imageUrl?: string;
  categoryId: number;
  vendorId: number;
  stockQty: number;
  reservedStockQty: number;
}

export interface CartResponse {
  id: number;
  userId: number;
  updatedAt: string;
  items: CartItemResponse[];
  totalAmount: number;
  totalItems: number;
  shippingAmount: number;
  taxAmount: number;
  discountCode?: string | null;
  discountAmount?: number;
  isDiscountExpired?: boolean;
}

export interface AddToCartRequest {
  variantId: number;
  quantity: number;
}

export interface UpdateCartItemRequest {
  newQuantity: number;
}
