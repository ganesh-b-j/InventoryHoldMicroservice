import { HoldResponse } from '../types';

type ActiveHoldsProps = {
  holds: HoldResponse[];
  loading: boolean;
  onRelease: (holdId: string) => void;
};

export default function ActiveHolds({ holds, loading, onRelease }: ActiveHoldsProps) {
  return (
    <section>
      <h2>Active Holds</h2>
      {holds.length === 0 ? (
        <p>No active holds yet.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Customer</th>
              <th>Status</th>
              <th>Expires In</th>
              <th>Items</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {holds.map((hold) => {
              const expiresAt = new Date(hold.expiresAt);
              const remaining = Math.max(0, Math.round((expiresAt.getTime() - Date.now()) / 1000 / 60));
              const items = hold.items.map((item) => `${item.productName} × ${item.quantity}`).join(', ');

              return (
                <tr key={hold.holdId}>
                  <td>{hold.customerId}</td>
                  <td>
                    <span className="status">{hold.status}</span>
                  </td>
                  <td>{remaining} min</td>
                  <td>{items}</td>
                  <td>
                    <button onClick={() => void onRelease(hold.holdId)} disabled={loading}>
                      Release
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </section>
  );
}
