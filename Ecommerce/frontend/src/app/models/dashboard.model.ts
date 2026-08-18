import { NumberValueAccessor } from "@angular/forms";

export interface StatCard {
    value: number;
    changePercent: number;
    changeDirection: 'up' | 'down';
}

export interface DashboardStats {
    totalRevenue: StatCard;
    totalOrders: StatCard;
    activeProducts: StatCard;
    activeVendors: StatCard;
}

export interface RevenuePoint {
    label: string;
    revenue: number;
}

export interface DonutSlice {
    label: string;
    percentage: number;
    color: string;
}

export interface OrderStatus {
    totalOrders: number;
    slices: DonutSlice[];
}

export interface DiscountDistribution {
    totalTypes: number;
    slices: DonutSlice[];
}

export type OrderStatusType = 'DELIVERED' | 'SHIPPED' | 'CONFIRMED' | 'CANCELLED' | 'PENDING';

export interface RecentOrder {
    id : number;
    customerName: string;
    amount: number;
    status: OrderStatusType;
}

export interface TopSellingProduct {
    rank: number;
    name: string;
    category: string;
    unitsSold: number;
    revenue: number;
}

export type NotificationType = 'info' | 'warning' | 'success' | 'error';

export interface RecentNotification {
    id: number;
    message: string;
    type: NotificationType;
    notifiedAt: string;
    timeAgo: string;
    read: boolean;
}

export interface CategoryPerformance {
    category: string;
    orders: number;
    percentage: number;
}

export interface VendorPerformance {
    rank: number;
    name: string;
    revenue: number;
    percentage: number;
}

export interface DashboardData {
    stats: DashboardStats;
    revenueBreakdown: {
        monthly: RevenuePoint[];
    };
    orderStatus: {
        totalOrders: number;
        slices: DonutSlice[];
    };
    recentOrders: RecentOrder[];
    topSellingProducts: TopSellingProduct[];
    discountDistribution: {
        totalTypes: number;
        slices: DonutSlice[];
    };
    recentNotifications: RecentNotification[];
    categoryPerformance: CategoryPerformance[];
    vendorPerformance: VendorPerformance[];
}

export interface PerformanceMetrics {
    topSellingProducts: TopSellingProduct[];
    categoryPerformance: CategoryPerformance[];
    vendorPerformance: VendorPerformance[];
}

export interface RecentActivity {
    recentOrders: RecentOrder[];
    recentNotifications: RecentNotification[];
}
