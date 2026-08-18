export interface Category {
  id: number;
  name: string;
  slug: string;
  parentId?: number;
  productCount?: number;
}

export interface CategoryTreeNode {
  id: number;
  name: string;
  slug: string;
  parentId?: number;
  isActive: boolean;
  productCount: number;
  children: CategoryTreeNode[];
  expanded?: boolean;
  level?: number;
  visible?: boolean;
}
