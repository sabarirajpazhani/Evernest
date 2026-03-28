import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Eye, EyeOff, UserPlus, Sparkles } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/Button';
import { GlassInput } from '@/components/ui/GlassInput';
import FloatingParticles from '@/components/FloatingParticles';

interface SignupFormData {
  email: string;
  username: string;
  password: string;
  confirmPassword: string;
}

const SignupPage = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const { register: registerUser, isLoading } = useAuth();
  const navigate = useNavigate();
  
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
    setError,
  } = useForm<SignupFormData>();

  const password = watch('password');

  const onSubmit = async (data: SignupFormData) => {
    const result = await registerUser(data);
    
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
            <span className="text-sm text-muted-foreground">Join the community</span>
          </div>
          <h1
            className="text-7xl font-extrabold tracking-tight mb-4 text-balance"
            style={{ lineHeight: '1.05' }}
          >
            <span className="gradient-text">Evernest</span>
          </h1>
          <p className="text-xl text-muted-foreground max-w-md mx-auto leading-relaxed">
            Start your journey with us. Connect, share, and grow together.
          </p>
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
            <h2 className="text-2xl font-semibold mb-1">Create account</h2>
            <p className="text-muted-foreground text-sm mb-8">Join Evernest today</p>

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Username */}
              <div
                className="opacity-0 animate-fade-up"
                style={{ animationDelay: '0.2s', animationFillMode: 'forwards' }}
              >
                <GlassInput
                  label="Username"
                  placeholder="johndoe"
                  {...register('username', {
                    required: 'Username is required',
                    minLength: {
                      value: 3,
                      message: 'Username must be at least 3 characters',
                    },
                    pattern: {
                      value: /^[a-zA-Z0-9_]+$/,
                      message: 'Username can only contain letters, numbers, and underscores',
                    },
                  })}
                  error={errors.username?.message}
                />
              </div>

              {/* Email */}
              <div
                className="opacity-0 animate-fade-up"
                style={{ animationDelay: '0.3s', animationFillMode: 'forwards' }}
              >
                <GlassInput
                  label="Email"
                  type="email"
                  placeholder="john@example.com"
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
                style={{ animationDelay: '0.4s', animationFillMode: 'forwards' }}
              >
                <div className="relative">
                  <GlassInput
                    label="Password"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Enter your password"
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

              {/* Confirm Password */}
              <div
                className="opacity-0 animate-fade-up"
                style={{ animationDelay: '0.5s', animationFillMode: 'forwards' }}
              >
                <div className="relative">
                  <GlassInput
                    label="Confirm Password"
                    type={showConfirmPassword ? 'text' : 'password'}
                    placeholder="Confirm your password"
                    {...register('confirmPassword', {
                      required: 'Please confirm your password',
                      validate: (value: string) => value === password || 'Passwords do not match',
                    })}
                    error={errors.confirmPassword?.message}
                  />
                  <button
                    type="button"
                    className="absolute right-3 top-9 text-muted-foreground hover:text-foreground transition-colors duration-200"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  >
                    {showConfirmPassword ? <EyeOff size={16} /> : <Eye size={16} />}
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
                style={{ animationDelay: '0.6s', animationFillMode: 'forwards' }}
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
                      Creating account…
                    </span>
                  ) : (
                    <>
                      <UserPlus className="w-4 h-4" />
                      Sign Up
                    </>
                  )}
                </Button>
              </div>
            </form>

            <div className="mt-6 text-center">
              <p className="text-sm text-muted-foreground">
                Already have an account?{' '}
                <Link
                  to="/login"
                  className="text-primary hover:text-primary/80 transition-colors duration-300"
                >
                  Sign in
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignupPage;
