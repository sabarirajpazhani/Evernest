import { useDispatch, useSelector } from 'react-redux';
import { useEffect } from 'react';
import { RootState } from '../app/store';
import { loginSuccess, logout, setLoading } from '../app/slices/authSlice';
import { useLoginMutation, useRegisterMutation, useValidateTokenMutation } from '../app/api/authApi';

export const useAuth = () => {
  const dispatch = useDispatch();
  const { user, token, isAuthenticated, isLoading } = useSelector((state: RootState) => state.auth);
  
  const [loginMutation] = useLoginMutation();
  const [registerMutation] = useRegisterMutation();
  const [validateTokenMutation] = useValidateTokenMutation();

  useEffect(() => {
    const validateAuth = async () => {
      if (token) {
        try {
          const result = await validateTokenMutation(token).unwrap();
          if (!result) {
            dispatch(logout());
          } else {
            // Fetch user profile
            // This would be handled by a separate API call
          }
        } catch (error) {
          dispatch(logout());
        }
      }
      dispatch(setLoading(false));
    };

    validateAuth();
  }, [token, dispatch, validateTokenMutation]);

  const login = async (email: string, password: string) => {
    try {
      dispatch(setLoading(true));
      const result = await loginMutation({ email, password }).unwrap();
      dispatch(loginSuccess({ user: result.user, token: result.token }));
      return { success: true };
    } catch (error: any) {
      dispatch(setLoading(false));
      return { 
        success: false, 
        error: error.data?.message || 'Login failed' 
      };
    }
  };

  const register = async (userData: {
    email: string;
    username: string;
    adminCode: string;
    password: string;
    confirmPassword: string;
    bio?: string;
  }) => {
    try {
      dispatch(setLoading(true));
      const result = await registerMutation(userData).unwrap();
      
      if (result.token) {
        dispatch(loginSuccess({ user: result.user, token: result.token }));
      }
      
      return { success: true, user: result.user };
    } catch (error: any) {
      dispatch(setLoading(false));
      return { 
        success: false, 
        error: error.data?.message || 'Registration failed' 
      };
    }
  };

  const logoutUser = () => {
    dispatch(logout());
  };

  return {
    user,
    token,
    isAuthenticated,
    isLoading,
    login,
    register,
    logout: logoutUser,
  };
};
