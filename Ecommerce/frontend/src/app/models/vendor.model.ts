export interface VendorModel {
  storeName: string;
  storeEmail?: string;
  gstNumber: string;
  panNumber: string;
  description?: string;
  logoUrl?: string;
}

export interface VendorBasicResponse {
  storeName: string;
  storeEmail: string;
  description: string;
  logoUrl: string;
}

export enum VendorStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Cancelled = 'Cancelled'
}

export interface VendorProfileResponse {
  id: number;
  userId: number;
  userFullName: string;
  userEmail: string;
  storeName: string;
  storeEmail: string;
  gstNumber: string;
  panNumber: string;
  description: string;
  logoUrl: string;
  status: VendorStatus;
}