export interface OrderItemDTO {
  id: number;
  orderId: number;
  variantId: number;
  productId: number;
  shipmentId?: number;
  quantity: number;
  unitPrice: number;
  productName?: string;
  imageUrl?: string;
  vendorId: number;
}

export interface OrderSummaryResponse {
  orderId: number;
  userId: number;
  fullName: string;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  shippingAmount: number;
  platformCommission: number;
  total: number;
  status: string | number;
  paymentStatus: string | number;
  placedAt: string;
  items: OrderItemDTO[];
  paymentMethod?: string;
  transactionId?: string;
}
