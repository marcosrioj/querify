import { Link } from 'react-router-dom';
import { Container } from '@/shared/layout/container';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';

export function PortalFooter() {
  const currentYear = new Date().getFullYear();
  const { t } = usePortalI18n();

  return (
    <footer className="footer">
      <Container>
        <div className="flex flex-col items-center justify-center gap-3 py-5 md:flex-row md:justify-between">
          <div className="order-2 flex gap-2 text-sm font-normal md:order-1">
            <span className="text-muted-foreground">{currentYear} &copy;</span>
            <span className="text-secondary-foreground">{t('BaseFAQ Portal')}</span>
          </div>
          <nav className="order-1 flex gap-4 text-sm font-normal text-muted-foreground md:order-2">
            <Link to="/app/settings/general" className="hover:text-primary">
              {t('Settings')}
            </Link>
            <Link to="/app/billing" className="hover:text-primary">
              {t('Billing')}
            </Link>
            <Link to="/app/ai" className="hover:text-primary">
              {t('AI')}
            </Link>
          </nav>
        </div>
      </Container>
    </footer>
  );
}
