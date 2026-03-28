import { InputHTMLAttributes, forwardRef, TextareaHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  multiline?: boolean;
  rows?: number;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, label, error, multiline = false, rows = 3, ...props }, ref) => {
    if (multiline) {
      return (
        <div className="w-full">
          {label && (
            <label className="block text-sm font-medium text-dark-text mb-1">
              {label}
            </label>
          )}
          <textarea
            className={cn(
              'bg-dark-bg border border-dark-border rounded-lg px-4 py-2 text-dark-text placeholder-dark-muted focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-200 w-full resize-none',
              error && 'border-red-500 focus:ring-red-500',
              className
            )}
            rows={rows}
            ref={ref as any}
            {...(props as any)}
          />
          {error && (
            <p className="mt-1 text-sm text-red-500">
              {error}
            </p>
          )}
        </div>
      );
    }

    return (
      <div className="w-full">
        {label && (
          <label className="block text-sm font-medium text-dark-text mb-1">
            {label}
          </label>
        )}
        <input
          className={cn(
            'bg-dark-bg border border-dark-border rounded-lg px-4 py-2 text-dark-text placeholder-dark-muted focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-200 w-full',
            error && 'border-red-500 focus:ring-red-500',
            className
          )}
          ref={ref}
          {...props}
        />
        {error && (
          <p className="mt-1 text-sm text-red-500">
            {error}
          </p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

export default Input;
