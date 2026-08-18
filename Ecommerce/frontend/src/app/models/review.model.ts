export interface CreateReviewRequest {
  productId: number;
  orderId: number;
  rating: number;
  title: string;
  body?: string;
  imageUrls: string[];
}

export interface UpdateReviewRequest {
  rating: number;
  title: string;
  body?: string;
  imageUrls: string[];
}

export interface ReviewResponse {
  id: number;
  productId: number;
  productName: string;
  userId: number;
  userFullName: string;
  orderId: number;
  rating: number;
  title: string;
  body?: string;
  updatedAt: string;
  reviewImages: string[];
}
