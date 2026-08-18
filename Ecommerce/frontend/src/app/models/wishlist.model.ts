export interface AddToWishlistRequest {
    variantId: number;
}

export interface WishListResponse {
    id: number;
    totalItems: number;
    items: WishListItemResponse[];
}

export interface WishListItemResponse {
    id: number;
    variantId: number;
    productId: number;
    productName: string;
    unitPrice: number;
    isInStock: boolean;
    categoryName?: string;
    imageUrl?: string;
}