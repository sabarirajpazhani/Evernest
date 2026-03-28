import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Eye, EyeOff, Mail, Lock, ArrowRight, Sparkles } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/Button';
import { GlassInput } from '@/components/ui/GlassInput';
import FloatingParticles from '@/components/FloatingParticles';

interface LoginFormData {
  email: string;
  password: string;
}

const LoginPage = () => {
  const [showPassword, setShowPassword] = useState(false);
  const { login, isLoading } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
    setError,
  } = useForm<LoginFormData>();

  const onSubmit = async (data: LoginFormData) => {
    const result = await login(data.email, data.password);
    if (result.success) {
      navigate('/');
    } else {
      setError('root', { message: result.error });
    }
  };

  return (
    <div className="min-h-screen flex animated-gradient-bg relative overflow-hidden">
      <FloatingParticles count={50} />

      {/* Left — branding */}
      <div className="lg:flex lg:flex-1 lg:items-center lg:justify-center lg:relative lg:overflow-hidden hidden">
        {/* Glow orbs */}
        <div className="absolute w-72 h-72 rounded-full bg-neon-purple/20 blur-[100px] top-1/4 left-1/4 animate-float lg:block" />
        <div
          className="absolute w-96 h-96 rounded-full bg-neon-blue/15 blur-[120px] bottom-1/4 right-1/4 animate-float lg:block"
          style={{ animationDelay: '2s' }}
        />
        <div
          className="absolute w-64 h-64 rounded-full bg-neon-pink/10 blur-[80px] top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 animate-float lg:block"
          style={{ animationDelay: '4s' }}
        />

        <div className="relative z-10 text-center px-12 opacity-0 animate-fade-up lg:block">
          <div className="inline-flex items-center gap-2 glass rounded-full px-4 py-2 mb-8 animate-glow-pulse">
            <Sparkles className="w-4 h-4 text-neon-purple" />
            <span className="text-sm text-muted-foreground">Private social platform</span>
          </div>
          <h1
            className="text-7xl font-extrabold tracking-tight mb-4 text-balance"
            style={{ lineHeight: '1.05' }}
          >
            <span className="gradient-text">Evernest</span>
          </h1>
          <p className="text-xl text-muted-foreground max-w-md mx-auto leading-relaxed">
            Your private space for real connections. Curated, intimate, alive.
          </p>

          {/* Floating stats */}
          <div className="flex gap-8 justify-center mt-12">
            {[
              { label: 'Members', value: '2,847' },
              { label: 'Active now', value: '312' },
              { label: 'Posts today', value: '1.4k' },
            ].map((stat, i) => (
              <div
                key={stat.label}
                className="glass rounded-2xl px-5 py-3 opacity-0 animate-fade-up lg:block"
                style={{
                  animationDelay: `${0.4 + i * 0.15}s`,
                  animationFillMode: 'forwards',
                }}
              >
                <p className="text-lg font-bold gradient-text">{stat.value}</p>
                <p className="text-xs text-muted-foreground">{stat.label}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Right — form */}
      <div className="flex-1 flex items-center justify-center p-6 lg:p-12 relative">
        <div
          className="w-full max-w-md opacity-0 animate-fade-up"
          style={{ animationDelay: '0.15s', animationFillMode: 'forwards' }}
        >
          {/* Mobile logo */}
          <div className="lg:hidden text-center mb-10">
            <h1 className="text-5xl font-extrabold gradient-text mb-2">Evernest</h1>
            <p className="text-muted-foreground text-sm">Your private social platform</p>
          </div>

          <div className="glass-strong rounded-3xl p-8 neon-glow">
            <h2 className="text-2xl font-semibold mb-1">Welcome back</h2>
            <p className="text-muted-foreground text-sm mb-8">Sign in to your Evernest</p>

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Email */}
              <div
                className="opacity-0 animate-fade-up"
                style={{ animationDelay: '0.2s', animationFillMode: 'forwards' }}
              >
                <GlassInput
                  label="Email"
                  type="email"
                  icon={<Mail className="w-4 h-4" />}
                  {...register('email', {
                    required: 'Email is required',
                    pattern: {
                      value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                      message: 'Invalid email address',
                    },
                  })}
                  error={errors.email?.message}
                />
              </div>

              {/* Password */}
              <div
                className="opacity-0 animate-fade-up"
                style={{ animationDelay: '0.3s', animationFillMode: 'forwards' }}
              >
                <div className="relative">
                  <GlassInput
                    label="Password"
                    type={showPassword ? 'text' : 'password'}
                    icon={<Lock className="w-4 h-4" />}
                    {...register('password', {
                      required: 'Password is required',
                      minLength: {
                        value: 6,
                        message: 'Password must be at least 6 characters',
                      },
                    })}
                    error={errors.password?.message}
                  />
                  <button
                    type="button"
                    className="absolute right-3 top-9 text-muted-foreground hover:text-foreground transition-colors duration-200"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                  </button>
                </div>
              </div>

              {/* Root error */}
              {errors.root && (
                <div
                  className="opacity-0 animate-fade-up glass border border-red-500/40 text-red-400 px-4 py-3 rounded-xl text-sm"
                  style={{ animationFillMode: 'forwards' }}
                >
                  {errors.root.message}
                </div>
              )}

              {/* Submit */}
              <div
                className="opacity-0 animate-fade-up"
                style={{ animationDelay: '0.4s', animationFillMode: 'forwards' }}
              >
                <Button
                  type="submit"
                  variant="neon"
                  size="lg"
                  className="w-full mt-6 group"
                  disabled={isLoading}
                >
                  {isLoading ? (
                    <span className="flex items-center gap-2">
                      <span className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                      Signing in…
                    </span>
                  ) : (
                    <>
                      Sign In
                      <ArrowRight className="w-4 h-4 transition-transform duration-300 group-hover:translate-x-1" />
                    </>
                  )}
                </Button>
              </div>
            </form>

            <div className="mt-6 text-center">
              <p className="text-sm text-muted-foreground">
                Don't have an account?{' '}
                <Link
                  to="/signup"
                  className="text-primary hover:text-primary/80 transition-colors duration-300"
                >
                  Sign up
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;