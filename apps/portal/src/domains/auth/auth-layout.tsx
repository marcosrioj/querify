import { Link, Outlet } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { toAbsoluteUrl } from '@/lib/helpers';
import { RuntimeEnv } from '@/platform/runtime/env';
import { usePortalI18n } from '@/shared/lib/i18n';
import { Card, CardContent } from '@/shared/ui';

export function AuthLayout() {
  const { t } = usePortalI18n();

  return (
    <>
      <Helmet>
        <title>{t('Sign in')} | {RuntimeEnv.appName}</title>
      </Helmet>

      <style>
        {`
          .portal-auth-bg {
            background-image: url('${toAbsoluteUrl('/media/app/auth-bg.png')}');
          }
        `}
      </style>

      <div className="grid grow lg:grid-cols-2">
        <div className="order-2 flex items-center justify-center p-8 lg:order-1 lg:p-10">
          <Card className="w-full max-w-[440px]">
            <CardContent className="p-6 lg:p-8">
              <Outlet />
            </CardContent>
          </Card>
        </div>

        <div className="portal-auth-bg order-1 m-5 hidden rounded-xl border border-border bg-cover bg-center bg-no-repeat lg:flex">
          <div className="flex w-full flex-col justify-between gap-6 p-8 lg:p-12">
            <Link to="/login">
              <img
                src={toAbsoluteUrl('/media/app/default-logo.svg')}
                className="h-[28px] dark:hidden"
                alt="BaseFAQ Portal"
              />
              <img
                src={toAbsoluteUrl('/media/app/default-logo-dark.svg')}
                className="hidden h-[28px] dark:block"
                alt="BaseFAQ Portal"
              />
            </Link>

            <div className="max-w-xl space-y-5">
              <div className="space-y-3">
                <h1 className="text-2xl font-semibold text-mono">
                  {t('Manage your BaseFAQ workspace')}
                </h1>
                <p className="text-base font-medium text-secondary-foreground">
                  {t(
                    'Sign in to manage FAQs, Q&A items, sources, billing, and AI settings.',
                  )}
                </p>
              </div>

              <img
                src={toAbsoluteUrl('/media/app/auth-screen.png')}
                alt="BaseFAQ Portal"
                className="w-full rounded-xl border border-border shadow-xs"
              />
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
