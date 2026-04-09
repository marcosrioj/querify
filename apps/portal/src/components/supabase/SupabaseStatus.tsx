import React, { useEffect, useState } from 'react';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';

/**
 * A simple component that displays the status of the Supabase connection.
 * This can be used during development to verify that Supabase is properly connected.
 */
export const SupabaseStatus: React.FC<{
  checkConnection?: () => Promise<boolean>;
}> = ({ checkConnection }) => {
  const { t } = usePortalI18n();
  const [status, setStatus] = useState<'checking' | 'connected' | 'error'>(
    'checking',
  );
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const runConnectionCheck = async () => {
      if (!checkConnection) {
        setStatus('error');
        setError(t('No Supabase adapter has been configured for this app.'));
        return;
      }

      try {
        const isAvailable = await checkConnection();
        if (isAvailable) {
          setStatus('connected');
        } else {
          setStatus('error');
          setError(t('Supabase connection failed. Check console for details.'));
        }
      } catch (e) {
        setStatus('error');
        setError(e instanceof Error ? t(e.message) : t('Unknown error'));
      }
    };

    void runConnectionCheck();
  }, [checkConnection]);

  return (
    <div className="p-4 rounded-md border">
      <h3 className="text-lg font-medium mb-2">{t('Supabase Status')}</h3>
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
            ? t('Checking connection...')
            : status === 'connected'
              ? t('Connected to Supabase')
              : t('Connection error')}
        </span>
      </div>
      {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
    </div>
  );
};
