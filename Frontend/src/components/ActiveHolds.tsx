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
      {holds.length === 0 && <p>No active holds yet.</p>}
      <ul>
        {holds.map((hold) => {
          const expiresAt = new Date(hold.expiresAt);
          const remaining = Math.max(0, Math.round((expiresAt.getTime() - Date.now()) / 1000 / 60));

          return (
            <li key={hold.holdId} className="hold-card">
              <div>
                <strong>{hold.customerId}</strong>
                <span className="status">{hold.status}</span>
              </div>
              <div>Expires in {remaining} min</div>
              <ul>
                {hold.items.map((item) => (
                  <li key={item.productId}>
                    {item.productName} × {item.quantity}
                  </li>
                ))}
              </ul>
              <button onClick={() => void onRelease(hold.holdId)} disabled={loading}>
                Release Hold
              </button>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
