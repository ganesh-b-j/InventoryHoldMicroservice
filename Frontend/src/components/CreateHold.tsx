import { FormEvent } from 'react';
import { InventoryItemResponse } from '../types';

type CreateHoldProps = {
  availableProducts: InventoryItemResponse[];
  customerId: string;
  selectedProduct: string;
  quantity: number;
  loading: boolean;
  error: string | null;
  onCustomerIdChange: (value: string) => void;
  onSelectedProductChange: (value: string) => void;
  onQuantityChange: (value: number) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
};

export default function CreateHold({
  availableProducts,
  customerId,
  selectedProduct,
  quantity,
  loading,
  error,
  onCustomerIdChange,
  onSelectedProductChange,
  onQuantityChange,
  onSubmit,
}: CreateHoldProps) {
  return (
    <section>
      <h2>Create Hold</h2>
      {error && <div className="error">{error}</div>}
      <form onSubmit={onSubmit}>
        <label>
          Customer ID
          <input value={customerId} onChange={(e) => onCustomerIdChange(e.target.value)} />
        </label>
        <label>
          Product
          <select value={selectedProduct} onChange={(e) => onSelectedProductChange(e.target.value)}>
            <option value="">Select a product</option>
            {availableProducts.map((item) => (
              <option key={item.productId} value={item.productId}>
                {item.productName}
              </option>
            ))}
          </select>
        </label>
        <label>
          Quantity
          <input
            type="number"
            min={1}
            value={quantity}
            onChange={(e) => onQuantityChange(Number(e.target.value))}
          />
        </label>
        <button type="submit" disabled={loading || !selectedProduct}>
          Place Hold
        </button>
      </form>
    </section>
  );
}
