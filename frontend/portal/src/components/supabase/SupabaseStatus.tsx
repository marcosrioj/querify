import React, { useEffect, useState } from 'react';

/**
 * A simple component that displays the status of the Supabase connection.
 * This can be used during development to verify that Supabase is properly connected.
 */
export const SupabaseStatus: React.FC<{
  checkConnection?: () => Promise<boolean>;
}> = ({ checkConnection }) => {
  const [status, setStatus] = useState<'checking' | 'connected' | 'error'>(
    'checking',
  );
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const runConnectionCheck = async () => {
      if (!checkConnection) {
        setStatus('error');
        setError('No Supabase adapter has been configured for this app.');
        return;
      }

      try {
        const isAvailable = await checkConnection();
        if (isAvailable) {
          setStatus('connected');
        } else {
          setStatus('error');
          setError('Supabase connection failed. Check console for details.');
        }
      } catch (e) {
        setStatus('error');
        setError(e instanceof Error ? e.message : 'Unknown error');
      }
    };

    void runConnectionCheck();
  }, [checkConnection]);

  return (
    <div className="p-4 rounded-md border">
      <h3 className="text-lg font-medium mb-2">Supabase Status</h3>
      <div className="flex items-center gap-2">
        <div
          className={`w-3 h-3 rounded-full ${
            status === 'checking'
              ? 'bg-accent'
              : status === 'connected'
                ? 'bg-green-500'
                : 'bg-red-500'
          }`}
        />
        <span>
          {status === 'checking'
            ? 'Checking connection...'
            : status === 'connected'
              ? 'Connected to Supabase'
              : 'Connection error'}
        </span>
      </div>
      {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
    </div>
  );
};
