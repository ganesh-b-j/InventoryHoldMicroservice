import { useEffect, useMemo, useState } from 'react';
import { createHold, fetchInventory, releaseHold } from './api';
import {
  CreateHoldRequest,
  HoldResponse,
  InventoryItemResponse,
} from './types';
import ActiveHolds from './components/ActiveHolds';
import CreateHold from './components/CreateHold';
import Dashboard from './components/Dashboard';

function App() {
  const [page, setPage] = useState<'dashboard' | 'hold'>('dashboard');
  const [inventory, setInventory] = useState<InventoryItemResponse[]>([]);
  const [holds, setHolds] = useState<HoldResponse[]>([]);
  const [selectedProduct, setSelectedProduct] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [customerId, setCustomerId] = useState('guest');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [refreshSignal, setRefreshSignal] = useState(0);

  const availableProducts = useMemo(() => inventory, [inventory]);

  useEffect(() => {
    void refreshData();
  }, [refreshSignal]);

  const refreshData = async () => {
    setError(null);
    try {
      setLoading(true);
      const items = await fetchInventory();
      setInventory(items);
      setHolds((current) => current.filter((hold) => hold.status === 'Active'));
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedProduct) {
      setError('Choose a product first.');
      return;
    }

    const request: CreateHoldRequest = {
      customerId,
      items: [{ productId: selectedProduct, quantity }],
      durationMinutes: 15,
    };

    try {
      setLoading(true);
      const hold = await createHold(request);
      setHolds((current) => [hold, ...current]);
      await refreshData();
      setError(null);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const handleRelease = async (holdId: string) => {
    try {
      setLoading(true);
      await releaseHold(holdId);
      setHolds((current) => current.filter((hold) => hold.holdId !== holdId));
      await refreshData();
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-container">
      <div className="app-header">
        <h1>Inventory Hold Microservice</h1>
        {page === 'dashboard' ? (
          <button className="page-action" onClick={() => setPage('hold')}>
            Create Hold
          </button>
        ) : (
          <button className="page-action" onClick={() => setPage('dashboard')}>
            Back to Dashboard
          </button>
        )}
      </div>

      {page === 'dashboard' ? (
        <Dashboard inventory={inventory} loading={loading} />
      ) : (
        <div className="grid">
          <CreateHold
            availableProducts={availableProducts}
            customerId={customerId}
            selectedProduct={selectedProduct}
            quantity={quantity}
            loading={loading}
            error={error}
            onCustomerIdChange={setCustomerId}
            onSelectedProductChange={setSelectedProduct}
            onQuantityChange={setQuantity}
            onSubmit={handleSubmit}
          />
          <ActiveHolds holds={holds} loading={loading} onRelease={handleRelease} />
        </div>
      )}
    </div>
  );
}

export default App;
