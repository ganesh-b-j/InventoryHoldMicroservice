import { InventoryItemResponse } from '../types';

type DashboardProps = {
  inventory: InventoryItemResponse[];
  loading: boolean;
};

export default function Dashboard({ inventory, loading }: DashboardProps) {
  return (
    <section>
      <h2>Inventory Dashboard</h2>
      {loading && <p>Loading inventory...</p>}
      <table>
        <thead>
          <tr>
            <th>Product Name</th>
            <th>Available Quantity</th>
          </tr>
        </thead>
        <tbody>
          {inventory.map((product) => (
            <tr key={product.productId}>
              <td>{product.productName}</td>
              <td>{product.availableQuantity}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
