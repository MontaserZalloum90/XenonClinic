export const ItemCategory = {
  Medical: 0,
  Surgical: 1,
  Laboratory: 2,
  Pharmaceutical: 3,
  Equipment: 4,
  Office: 5,
  Other: 6,
} as const;

export type ItemCategory = typeof ItemCategory[keyof typeof ItemCategory];

export const StockStatus = {
  InStock: 0,
  LowStock: 1,
  OutOfStock: 2,
  Discontinued: 3,
} as const;

export type StockStatus = typeof StockStatus[keyof typeof StockStatus];

export interface InventoryItem {
  id: number;
  itemCode: string;
  name: string;
  description?: string;
  category: ItemCategory;
  quantity: number;
  minStockLevel: number;
  maxStockLevel?: number;
  unitPrice: number;
  supplier?: string;
  location?: string;
  expiryDate?: string;
  status: StockStatus;
  lastRestocked?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface InventoryStatistics {
  totalItems: number;
  lowStockItems: number;
  outOfStockItems: number;
  totalValue: number;
  categoryDistribution?: Record<string, number>;
  monthlyConsumption?: number;
}

export interface InventoryItemFormData {
  itemCode: string;
  name: string;
  description?: string;
  category: ItemCategory;
  quantity: number;
  minStockLevel: number;
  maxStockLevel?: number;
  unitPrice: number;
  supplier?: string;
  location?: string;
  expiryDate?: string;
  status: StockStatus;
}
