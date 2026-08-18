import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface OrderItemDTO {
  id: number;
  variantId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  vendorId: number;
}

export interface ShipmentResponseDTO {
  id: number;
  trackingNumber: string;
  userAddressId: number;
  estimatedFullfillement: string;
  shippedAt?: string;
  fulfilledAt?: string;
  status: number;
  shippingFee: number;
  orderItems: OrderItemDTO[];
}

@Injectable({
  providedIn: 'root'
})
export class ShipmentService {
  private baseUrl = `${environment.baseUrl}/Shipment`;

  constructor(private http: HttpClient) {}

  getVendorShipments(): Observable<ShipmentResponseDTO[]> {
    return this.http.get<ShipmentResponseDTO[]>(`${this.baseUrl}/vendor-shipments`);
  }

  getVendorShipmentsForAdmin(vendorId: number): Observable<ShipmentResponseDTO[]> {
    return this.http.get<ShipmentResponseDTO[]>(`${this.baseUrl}/all?vendorId=${vendorId}`);
  }

  getShipmentsByOrderId(orderId: number): Observable<ShipmentResponseDTO[]> {
    return this.http.get<ShipmentResponseDTO[]>(`${this.baseUrl}/order/${orderId}`);
  }
}
