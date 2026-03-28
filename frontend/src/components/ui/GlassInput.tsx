import React from "react";

interface GlassInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  icon?: React.ReactNode;
  error?: string;
}

const GlassInput = React.forwardRef<HTMLInputElement, GlassInputProps>(
  ({ label, icon, error, className, ...props }, ref) => {
    return (
      <div className="relative group">
        <div className={`glass rounded-2xl input-glow transition-all duration-300 ${error ? 'border-red-500/40' : ''}`}>
          <div className="flex items-center px-4 gap-3">
            {icon && (
              <span className="text-muted-foreground group-focus-within:text-primary transition-colors duration-300">
                {icon}
              </span>
            )}
            <div className="flex-1 relative py-3">
              <input
                ref={ref}
                placeholder=" "
                className="peer w-full bg-transparent text-foreground outline-none text-sm pt-3"
                {...props}
              />
              <label className="absolute left-0 top-1/2 -translate-y-1/2 text-muted-foreground text-sm transition-all duration-300 pointer-events-none peer-placeholder-shown:top-1/2 peer-placeholder-shown:text-sm peer-focus:top-1 peer-focus:text-xs peer-focus:text-primary peer-[:not(:placeholder-shown)]:top-1 peer-[:not(:placeholder-shown)]:text-xs peer-[:not(:placeholder-shown)]:text-xs">
                {label}
              </label>
            </div>
          </div>
          {error && (
            <div className="absolute -bottom-6 left-0 right-0 text-red-400 text-xs">
              {error}
            </div>
          )}
        </div>
      </div>
    );
  }
);

GlassInput.displayName = "GlassInput";

export { GlassInput };
