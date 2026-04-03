import { Outlet } from 'react-router-dom';
import { Helmet } from 'react-helmet-async';
import { toAbsoluteUrl } from '@/lib/helpers';
import { RuntimeEnv } from '@/platform/runtime/env';

export function AuthLayout() {
  return (
    <>
      <Helmet>
        <title>Sign in | {RuntimeEnv.appName}</title>
      </Helmet>
      <div className="relative min-h-screen overflow-hidden bg-mono text-mono-foreground">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(60,122,255,0.25),transparent_28%),radial-gradient(circle_at_bottom_right,rgba(15,172,146,0.18),transparent_25%)]" />
        <div className="relative z-10 grid min-h-screen lg:grid-cols-[minmax(0,460px)_1fr]">
          <div className="flex items-center justify-center px-6 py-12 lg:px-12">
            <div className="w-full max-w-md">
              <Outlet />
            </div>
          </div>
          <div className="hidden items-center justify-center border-l border-white/10 px-8 lg:flex">
            <div className="max-w-xl">
              <img
                src={toAbsoluteUrl('/media/app/auth-screen.png')}
                alt="BaseFAQ Portal"
                className="w-full rounded-[28px] border border-white/10 shadow-2xl"
              />
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
