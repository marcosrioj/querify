import { useEffect, useRef } from 'react';

type ReCaptchaInstance = {
  ready: (callback: () => void) => void;
  render: (
    container: HTMLElement,
    config: { sitekey: string; size: string; theme: string },
  ) => number;
  reset: (id: number) => void;
  getResponse: (id: number) => string;
};

const RECAPTCHA_SCRIPT_ID = 'recaptcha-v2-script';
let scriptLoadPromise: Promise<void> | null = null;

function loadRecaptchaScript(): Promise<void> {
  if (scriptLoadPromise) {
    return scriptLoadPromise;
  }

  scriptLoadPromise = new Promise((resolve) => {
    const recaptchaWindow = window as typeof window & {
      grecaptcha?: ReCaptchaInstance;
      onRecaptchaLoaded?: () => void;
    };

    if (document.getElementById(RECAPTCHA_SCRIPT_ID) && recaptchaWindow.grecaptcha) {
      resolve();
      return;
    }

    recaptchaWindow.onRecaptchaLoaded = () => {
      resolve();
    };

    const script = document.createElement('script');
    script.id = RECAPTCHA_SCRIPT_ID;
    script.src =
      'https://www.google.com/recaptcha/api.js?onload=onRecaptchaLoaded&render=explicit';
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);
  });

  return scriptLoadPromise;
}

export function useRecaptchaV2(siteKey: string) {
  const widgetId = useRef<number | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const isRendered = useRef(false);
  const isInitializing = useRef(false);

  const initializeRecaptcha = async () => {
    if (isInitializing.current || !containerRef.current || !siteKey) {
      return;
    }

    isInitializing.current = true;

    try {
      const recaptchaWindow = window as typeof window & {
        grecaptcha?: ReCaptchaInstance;
      };

      if (widgetId.current !== null && recaptchaWindow.grecaptcha) {
        recaptchaWindow.grecaptcha.reset(widgetId.current);
        widgetId.current = null;
        isRendered.current = false;
      }

      await loadRecaptchaScript();

      if (!recaptchaWindow.grecaptcha) {
        throw new Error('reCAPTCHA failed to load');
      }

      await new Promise<void>((resolve) => {
        recaptchaWindow.grecaptcha?.ready(() => resolve());
      });

      if (containerRef.current && !isRendered.current) {
        widgetId.current = recaptchaWindow.grecaptcha.render(containerRef.current, {
          sitekey: siteKey,
          size: 'normal',
          theme: 'light',
        });
        isRendered.current = true;
      }
    } catch (error) {
      console.error('Error initializing reCAPTCHA:', error);
    } finally {
      isInitializing.current = false;
    }
  };

  useEffect(() => {
    if (containerRef.current) {
      void initializeRecaptcha();
    }

    return () => {
      const recaptchaWindow = window as typeof window & {
        grecaptcha?: ReCaptchaInstance;
      };

      if (widgetId.current !== null && recaptchaWindow.grecaptcha) {
        recaptchaWindow.grecaptcha.reset(widgetId.current);
        widgetId.current = null;
        isRendered.current = false;
      }
    };
  }, [siteKey]);

  const getToken = () => {
    const recaptchaWindow = window as typeof window & {
      grecaptcha?: ReCaptchaInstance;
    };

    if (!recaptchaWindow.grecaptcha || widgetId.current === null) {
      throw new Error('reCAPTCHA not initialized');
    }

    return recaptchaWindow.grecaptcha.getResponse(widgetId.current);
  };

  const resetCaptcha = () => {
    const recaptchaWindow = window as typeof window & {
      grecaptcha?: ReCaptchaInstance;
    };

    if (!recaptchaWindow.grecaptcha || widgetId.current === null) {
      return;
    }

    recaptchaWindow.grecaptcha.reset(widgetId.current);
    widgetId.current = null;
    isRendered.current = false;
  };

  return {
    containerRef,
    getToken,
    resetCaptcha,
    initializeRecaptcha,
  };
}
