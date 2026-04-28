import { useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { fetchInventory } from './features/inventorySlice';
import { createHold, releaseHold } from './features/holdSlice';
import { AppDispatch, RootState } from './store';
import {
  CreateHoldRequest,
  InventoryItemResponse,
} from './types';
import ActiveHolds from './components/ActiveHolds';
import CreateHold from './components/CreateHold';
import Dashboard from './components/Dashboard';

function App() {
  const [page, setPage] = useState<'dashboard' | 'hold'>('dashboard');
  const [selectedProduct, setSelectedProduct] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [customerId, setCustomerId] = useState('guest');

  const dispatch = useDispatch<AppDispatch>();
  const inventory = useSelector((state: RootState) => state.inventory.items);
  const inventoryLoading = useSelector((state: RootState) => state.inventory.loading);
  const inventoryError = useSelector((state: RootState) => state.inventory.error);
  const holds = useSelector((state: RootState) => state.hold.items);
  const holdLoading = useSelector((state: RootState) => state.hold.loading);
  const holdError = useSelector((state: RootState) => state.hold.error);

  const availableProducts = useMemo(() => inventory, [inventory]);
  const loading = inventoryLoading || holdLoading;
  const error = holdError || inventoryError;

  useEffect(() => {
    void dispatch(fetchInventory());
  }, [dispatch]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedProduct) {
      return;
    }

    const request: CreateHoldRequest = {
      customerId,
      items: [{ productId: selectedProduct, quantity }],
      durationMinutes: 15,
    };

    try {
      await dispatch(createHold(request)).unwrap();
      await dispatch(fetchInventory()).unwrap();
    } catch {
      // Error state is handled by Redux.
    }
  };

  const handleRelease = async (holdId: string) => {
    try {
      await dispatch(releaseHold(holdId)).unwrap();
      await dispatch(fetchInventory()).unwrap();
    } catch {
      // Error state is handled by Redux.
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
        <div className="grid hold-grid">
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
